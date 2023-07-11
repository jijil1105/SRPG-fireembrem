using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

/*  GameManagerで色々なクラスを使用してゲームの進行を行っています
 *  ここの処理ではゲームマネージャーのシングルトン化を行っています。
 */

 /*   private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else
        {
            Destroy(gameObject);
        }
    }

    public static GameManager instance;*/

    
    //-------------------------------------------------------------------------
    //各マネージャークラスの宣言

    private MapManager mapManager;//フィールドのブロックを管理するマネージャークラス
    private CharactorManager charactorManager;//フィールドのキャラクターを管理するクラス
    private GUIManager guiManager;//UIを管理するクラス

    //-------------------------------------------------------------------------

    private bool isGameSet;//ゲーム終了フラグ

    //-------------------------------------------------------------------------

    // 行動キャンセル処理用変数
    private int charaStartPos_X; // 選択キャラクターの移動前の位置(X方向)
    private int charaStartPos_Z; // 選択キャラクターの移動前の位置(Z方向)
    private MapBlock attackBlock; // 選択キャラクターの攻撃先のブロック

    //-------------------------------------------------------------------------

    private Charactor selectingChara;//選択中のキャラクター（マップフィールド上のキャラクターを選択していない時はnull）
    private List<MapBlock> reachableBlocks;//選択キャラの移動可能範囲
    private List<MapBlock> attackableBlocks;//選択キャラの攻撃可能範囲
    private SkillDefine.Skill selectingSkill;//選択中の特技(通常攻撃時はNONE固定)

    private enum Phase
    {
        Myturn_Start,//自ターン：開始時
        Myturn_Moving,//自ターン：移動先選択中
        Myturn_Command,//自ターン：移動後のコマンド選択中（現在は攻撃と待機を選択可）
        Myturn_Targeting,//自ターン：攻撃対象を選択中
        Myturn_Result,//自ターン：行動結果表示中
        Enemyturn_Start,//敵ターン：開始時
        Enemyturn_Result//敵ターン：行動結果表示中
    }

    private Phase nowPhase;//現在の進行モード

    //------------------------------------------------------------------------
   

    //------------------------------------------------------------------------

    //変数の初期化

    public void Start()
    {
        mapManager = GetComponent<MapManager>();//
        charactorManager = GetComponent<CharactorManager>();
        guiManager = GetComponent<GUIManager>();
        reachableBlocks = new List<MapBlock>();
        attackableBlocks = new List<MapBlock>();
        nowPhase = Phase.Myturn_Start;
        AudioManager.instance.Play("BGM_1");

        DOVirtual.DelayedCall(0.05f, () =>
        {
            var chara = charactorManager.Charactors.FirstOrDefault(chara => chara.isIncapacitated != true && chara.isEnemy != true);
            Camera.main.GetComponent<CameraController>().get_chara_subject.OnNext(chara);
        });
    }

    //-------------------------------------------------------------------------

    //ボタンを押している間ずっと呼ばれてしまうのでボタンを押してから一度だけ処理を行うようにしています

    bool isCalledOnce = false;

    void Update()
    {
        //ゲーム終了後なら終了
        if(isGameSet)
            return;
        
        if(!isCalledOnce)
        {
            //入力の検出とUIへのタップ検出
            if (Input.GetMouseButton(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {//UIではない部分をタップした際にタップ先のブロックを取得
                isCalledOnce = true;
                AudioManager.instance.Play("SE_1");// Play SE
                GetMapBlockByTapPos();
            }
        }

        if(!Input.GetMouseButton(0)) { isCalledOnce = false; }
    }

    //-------------------------------------------------------------------------

    //プレイヤーがタッチしたフィールド上のブロックをRayを飛ばして取得しています

    /// <summary>
    /// タップ先のオブジェクト取得、選択処理を開始
    /// </summary>
    private void GetMapBlockByTapPos()
    {
        GameObject targetObject = null;//タップ先のブロック

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);//タップ方向にRayを飛ばす
        RaycastHit hit = new RaycastHit();//Rayの当たり判定初期化
        if (Physics.Raycast(ray, out hit))
        {//Rayに当たったオブジェクト取得
            targetObject = hit.collider.gameObject;
        }

        //Rayに当たったオブジェクトが存在すれば処理
        if (targetObject!=null)
        {
            //ブロック選択処理
            SelectBlock(targetObject.GetComponent<MapBlock>());
        }
    }

    //------------------------------------------------------------------------

    /// <summary>
    /// 選択処理：選択ブロックにキャラクターが居ればキャラクター選択処理開始、居なければブロック選択処理開始
    /// </summary>
    /// <param name="targetBlock"></param>タップしたオブジェクト情報
    private void SelectBlock(MapBlock targetBlock)
    {
        //現在の進行モードによって異なる処理を開始する
        switch (nowPhase)
        {
            //自分のターン：開始時
            case Phase.Myturn_Start:

                //全ブロックの選択状態解除
                mapManager.AllSelectionModeClear();
                //ブロックを選択状態として白色に強調表示にする
                targetBlock.SetSelectionMode(MapBlock.Highlight.Select);

                //選択したブロックの座標にキャラクターが居ればキャラクター情報取得、居なければnull
                Charactor charaData = charactorManager.GetCharactor(targetBlock.XPos, targetBlock.ZPos);

                //選択したブロックの座標にキャラクターが居た場合の処理
                if (charaData)
                {
                    //選択ブロックに居たキャラクターの記憶
                    selectingChara = charaData;
                    // 選択キャラクターの現在位置を記憶
                    charaStartPos_X = selectingChara.XPos;
                    charaStartPos_Z = selectingChara.ZPos;
                    //選択キャラのステータスをUI表示する
                    guiManager.ShowStatusWindow(charaData);

                    //行動不能状態なら処理しない
                    if (charaData.isIncapacitated)
                        return;

                    //選択キャラの移動可能ブロックリストを取得
                    reachableBlocks = mapManager.SearchReachableBlocks(charaData.XPos, charaData.ZPos);

                    //移動可能ブロックを青色に強調表示
                    foreach (MapBlock mapblock in reachableBlocks)
                        mapblock.SetSelectionMode(MapBlock.Highlight.Reachable);

                    //移動キャンセルボタン表示
                    guiManager.ShowMoveCancelButton(true);

                    //進行モード＜自分のターン：移動先選択中＞に変更
                    ChangePhase(Phase.Myturn_Moving);

                    //選択キャラのデバッグ出力
                    Debug.Log("Select Charactor :" + charaData.gameObject.name + " : position :" + charaData.XPos + " : " + charaData.ZPos);
                }

               else
                {//選択ブロック座標にキャラが居ない場合

                    //選択中キャラを初期化
                    ClearSelectingChara();
                    //選択ブロックのデバッグ出力
                    Debug.Log("Tapped on Block  Position : " + targetBlock.XPos + ", " + targetBlock.ZPos);
                }
                
                break;

            //自分のターン：移動先選択中
            case Phase.Myturn_Moving:

                //選択キャラが敵キャラクターなら移動先選択状態を解除
                if(selectingChara.isEnemy)
                {
                    CancelMoving();
                    break;
                }

                // 選択ブロックが移動可能な場所リスト内にある場合、移動処理を開始
                if (reachableBlocks.Contains(targetBlock))
                {
                    //選択キャラを選択ブロックへ移動
                    selectingChara.MovePosition(targetBlock.XPos, targetBlock.ZPos);

                    //移動可能ブロックリストを初期化
                    reachableBlocks.Clear();

                    //全ブロックの選択状態解除
                    mapManager.AllSelectionModeClear();

                    //キャンセルボタン非表示
                    guiManager.ShowMoveCancelButton(false);

                    // 0.5秒数経過後に処理を実行する
                    DOVirtual.DelayedCall(0.5f, () =>
                    {　　//コマンドボタン表示
                        guiManager.ShowCommandButtons(selectingChara);

                        //進行モード＜自分のターン：移動後のコマンド選択中＞に変更
                        ChangePhase(Phase.Myturn_Command);
                    });
                }
                break;
            //自分のターン：移動後のコマンド選択中
            case Phase.Myturn_Command:
                // キャラクター攻撃処理
                // (攻撃可能ブロックを選択した場合に攻撃処理を呼び出す)
                if (attackableBlocks.Contains(targetBlock))
                {
                    // 攻撃先のブロック情報を記憶
                    attackBlock = targetBlock;

                    // 行動決定・キャンセルボタンを表示する
                    guiManager.ShowDecideButtons();

                    // 攻撃可能な場所リストを初期化する
                    attackableBlocks.Clear();

                    // 全ブロックの選択状態を解除
                    mapManager.AllSelectionModeClear();

                    // 攻撃先のブロックを強調表示する
                    attackBlock.SetSelectionMode(MapBlock.Highlight.Attackable);

                    // 進行モードを進める：攻撃の対象を選択中
                    ChangePhase(Phase.Myturn_Targeting);
                }
                break;
        } 
    }

    //------------------------------------------------------------------------

    /// <summary>
	/// 選択中のキャラクター情報を初期化する
	/// </summary>
    private void ClearSelectingChara()
    {
        // 選択中のキャラクターを初期化する
        selectingChara = null;
        // キャラクターのステータスUIを非表示にする
        guiManager.HideStatusWindow();
    }

    //------------------------------------------------------------------------

    /// <summary>
	/// ターン進行モードを変更する
	/// </summary>
	/// <param name="NowPhase">変更先モード</param>
    /// <param name="noLogos">trueだとロゴを表示しない</param>
    private void ChangePhase(Phase NowPhase, bool noLogos = false)
    {
        //ゲーム終了後なら終了
        if (isGameSet)
            return;

        // モード変更を保存
        nowPhase = NowPhase;
        Debug.Log("Change" + nowPhase);

        // 特定のモードに切り替わったタイミングで行う処理
        switch (nowPhase)
        {
            // 自分のターン：開始時
            case Phase.Myturn_Start :
                //行動可能な自キャラが居るかチェック
                var chara = charactorManager.Charactors.FirstOrDefault(chara => chara.isIncapacitated != true && chara.isEnemy != true);
                //動かせるキャラが居なかった場合
                if (!chara)
                {
                    //味方キャラ取得
                    var charas = charactorManager.Charactors.Where(chara => chara.isEnemy == false);
                    //行動可能にする
                    foreach (var charaData in charas)          
                        charaData.SetInCapacitited(false);
                                           
                    ChangePhase(Phase.Enemyturn_Start);
                }

                // 自分のターン開始時のロゴを表示
                if(!noLogos)
                    guiManager.ShowLogoChangeTurn(true);
                break;

            // 敵のターン：開始時
            case Phase.Enemyturn_Start :
                var enemy = charactorManager.Charactors.FirstOrDefault(chara => chara.isEnemy == true && chara.isIncapacitated != true);
                Debug.Log("" + enemy);
                //動かせるキャラが居なかった場合
                if(!enemy)
                {
                    //敵キャラ取得
                    var charas = charactorManager.Charactors.Where(chara => chara.isEnemy == true);
                    //行動可能にする
                    foreach (var charaData in charas)
                        charaData.SetInCapacitited(false);

                    ChangePhase(Phase.Myturn_Start);

                    var chara_data = charactorManager.Charactors.FirstOrDefault(chara => chara.isIncapacitated != true && chara.isEnemy != true);
                    Camera.main.GetComponent<CameraController>().get_chara_subject.OnNext(chara_data);
                    return;
                }

                // 敵のターン開始時のロゴを表示
                if (!noLogos)
                    guiManager.ShowLogoChangeTurn(false);
                
                // 敵の行動を開始する処理
                // (ロゴ表示後に開始させる為、遅延処理にする)
                DOVirtual.DelayedCall(1.0f, () =>
                {　　//1秒遅延実行する内容
                    EnemyCommand();
                });
                break;
        }
    }

    //------------------------------------------------------------------------

    /// <summary>
	/// 攻撃コマンドボタン処理
	/// </summary>
    public void AttackCommand()
    {
        AudioManager.instance.Play("SE_1");//Play SE

        // 特技の選択をオフにする
        selectingSkill = SkillDefine.Skill._None;

        // 攻撃範囲を取得して表示する
        GetAttackableBlocks();
    }

    //------------------------------------------------------------------------

    /// <summary>
    /// 待機コマンドボタン処理
    /// </summary>
    public void StandbyCommand()
    {
        AudioManager.instance.Play("SE_1");

        // コマンドボタンを非表示にする
        guiManager.HideCommandButtons();

        //行動不能状態にする
        selectingChara.SetInCapacitited(true);

        selectingChara = null;

        // 進行モード＜自ターン：開始時＞に変更
        ChangePhase(Phase.Myturn_Start, true);
    }

    //------------------------------------------------------------------------

    /// <summary>
    /// スキルコマンドボタン処理
    /// </summary>
    public void SkillCommand()
    {
        // キャラクターの持つ特技を選択状態にする
        selectingSkill = selectingChara.skill;

        // 攻撃範囲を取得して表示する
        GetAttackableBlocks();
    }

    //------------------------------------------------------------------------

    /// <summary>
    /// 攻撃・スキルコマンド選択後に対象ブロックを表示する処理
    /// </summary>
    private void GetAttackableBlocks()
    {
        // コマンドボタンを非表示にする
        guiManager.HideCommandButtons();

        // （特技：ファイアボールの場合はマップ全域に対応）
        if (selectingSkill == SkillDefine.Skill.FireBall)
            attackableBlocks = mapManager.MapBlocksToList();

        // 攻撃可能な場所リストを取得する
        else
            attackableBlocks = mapManager.SearchAttackableBlocks(selectingChara.XPos, selectingChara.ZPos);
        
        // 攻撃可能な場所リストを表示する
        foreach (MapBlock block in attackableBlocks)
            block.SetSelectionMode(MapBlock.Highlight.Attackable);
    }

    //------------------------------------------------------------------------

    /// <summary>
	/// キャラクターが他のキャラクターに攻撃する処理
	/// </summary>
	/// <param name="attackchara">攻撃側キャラデータ</param>
	/// <param name="defensechara">防御側キャラデータ</param>
    private void CharaAttack(Charactor attackchara, Charactor defensechara)
    {
        // ダメージ計算処理
        int damagevalue;// ダメージ量
        int atkpoint = attackchara.atk;
        int defpoint = defensechara.def;
        float delay_time = 2.0f;

        if (!attackchara.isMagicAttac)//攻撃キャラが物理攻撃の場合
        {
            atkpoint = attackchara.atk;// 攻撃側の物理攻撃力
            defpoint = defensechara.def;// 防御側の物理防御力
        }
        else if(attackchara.isMagicAttac)//攻撃キャラが魔法攻撃の場合
        {
            atkpoint = attackchara.Int;// 攻撃側の魔法攻撃力
            defpoint = defensechara.Res;// 防御側の魔法防御力
        }

        //防御力０化デバフ状態なら防御力０でダメージ計算
        if (defensechara.isDefBreak)
            defpoint = 0;

        damagevalue = atkpoint - defpoint;// ダメージ＝攻撃力－防御力で計算

        float ratio = GetDamegeRatioByAttribute(attackchara, defensechara);
        damagevalue = (int)(damagevalue * ratio);

        // ダメージ量が0以下なら0にする
        if (damagevalue < 0)
            damagevalue = 0;

        //スキル選択ならスキルのダメージ倍率適応
        damagevalue = SkillAttack(damagevalue, atkpoint, attackchara, defensechara);
        
        // キャラクター攻撃アニメーション
        attackchara.AttackAnimation(defensechara, selectingSkill);
        
        // バトル結果表示ウィンドウの表示設定
        guiManager.battleWindowUI.ShowWindow(defensechara, damagevalue);

        // ダメージ量分防御側のHPを減少
        defensechara.NowHp -= damagevalue;
        // HPが0～最大値の範囲に収まるよう補正
        defensechara.NowHp = Mathf.Clamp(defensechara.NowHp, 0, defensechara.maxHP);

        // HP0になったキャラクターを削除する
        if (defensechara.NowHp == 0)
        {
            //味方キャラなら経験値更新処理
            if(!attackchara.isEnemy)
            {
                //経験値を更新する前の値を記憶
                float startvalue = (float)attackchara.nowExp / attackchara.ExpPerLv;

                //キャラの現在の経験値を更新
                attackchara.nowExp += (int)GetComponent<LevelManager>().GetExp(100, 1.5f, defensechara.Lv);

                //更新した経験値量が次レベルに必要な経験値量を下回っていたら
                if (attackchara.nowExp < attackchara.ExpPerLv)
                {
                    delay_time = 3;

                    //経験値更新後の値を記憶
                    float endvalue = (float)attackchara.nowExp / (float)attackchara.ExpPerLv;

                    DOVirtual.DelayedCall(1.0f, () =>
                    {
                        // ウィンドウを非表示化
                        guiManager.battleWindowUI.HideWindow();
                        //取得経験値をアニメーション再生する経験値バーを表示
                        guiManager.ShowGetExpWindow(attackchara);
                        //経験値更新前から経験値更新後の値まで経験値バーをアニメーション再生する
                        guiManager.moveExpbar(startvalue, endvalue, 1.0f);
                    });

                    //経験値バーをアニメーション再生後、経験値バーを非表示
                    DOVirtual.DelayedCall(2.5f, () =>
                    {
                        //経験値バーを非表示
                        guiManager.HideGetExpWindow();
                        //経験値更新後のステータスを表示
                        guiManager.ShowStatusWindow(attackchara);
                    });
                }

                //キャラの現在の経験値が次レベルに必要な経験値を上回っていたら
                else
                {
                    delay_time = 5;
                    //現在の経験値から次レベルに必要な経験値の差を記憶
                    float EndExp = (float)attackchara.nowExp - (float)attackchara.ExpPerLv;
                    //キャラのレベルをアップ
                    var up_list = GetComponent<LevelManager>().LevelUp(attackchara);
                    //レベルアップ後の次レベルに必要な経験値で経験値バーのfillamountに適応する値を求める
                    float endvalue = EndExp / (float)attackchara.ExpPerLv;

                    DOVirtual.DelayedCall(1.0f, () =>
                    {
                        // ウィンドウを非表示化
                        guiManager.battleWindowUI.HideWindow();
                        //取得経験値をアニメーション再生する経験値バーを表示
                        guiManager.ShowGetExpWindow(attackchara);
                        //経験値バーをアニメーション再生
                        guiManager.moveExpbar(startvalue, 1, 0.5f);
                    });

                    DOVirtual.DelayedCall(1.6f, () =>
                    {
                        //経験値バーを表示するウィンドウを更新
                        guiManager.ShowGetExpWindow(attackchara);
                        //ステータスウィンドウを更新
                        guiManager.ShowStatusWindow(attackchara);
                        //経験値バーをアニメーション再生
                        guiManager.moveExpbar(0, endvalue, 0.5f);
                    });

                    //経験値バーのアニメーション再生終了後に非表示
                    DOVirtual.DelayedCall(2.6f, () =>
                    {
                        guiManager.HideGetExpWindow();
                        guiManager.ShowLeveUpWindow(attackchara, up_list);
                    });

                    DOVirtual.DelayedCall(4.5f, () =>
                    {
                        guiManager.HideLevelUpWindow();
                    });
                }
            }

            //倒したキャラクターを削除
            charactorManager.DeleteCharaData(defensechara);
        }



        // Skillの選択状態を解除する
        selectingSkill = SkillDefine.Skill._None;

        // ターン切り替え処理(遅延実行)
        DOVirtual.DelayedCall(
            delay_time,// 遅延時間(秒)
            () =>
            {// 遅延実行する内容

                // ウィンドウを非表示化
                guiManager.battleWindowUI.HideWindow();
                // ターンを切り替える
                if (nowPhase == Phase.Myturn_Result)
                {
                    //行動不能状態
                    attackchara.SetInCapacitited(true);

                    ChangePhase(Phase.Myturn_Start, true);
                }

                else if(nowPhase == Phase.Enemyturn_Result)
                {
                    //行動不能状態
                    attackchara.SetInCapacitited(true);

                    ChangePhase(Phase.Enemyturn_Start, true);
                }
                
        });

        Debug.Log("Atk:" + attackchara.charaName + " Def:" + defensechara.charaName);
    }

    //------------------------------------------------------------------------

    public int SkillAttack(int damagevalue, int atkpoint, Charactor attackchara, Charactor defensechara)
    {
        // 選択したスキルによるダメージ値補正および効果処理
        switch (selectingSkill)
        {
            case SkillDefine.Skill.Critical:// スキル：会心の一撃

                // ダメージ２倍
                damagevalue *= 2;

                //スキル使用不可状態にする
                attackchara.isSkillLock = true;
                break;

            case SkillDefine.Skill.DefBreak:// スキル：防御破壊

                //ダメージ０
                damagevalue = 0;

                //防御力０化デバフON
                defensechara.isDefBreak = true;
                break;

            case SkillDefine.Skill.Heal:// スキル：ヒール

                // 回復(回復量は攻撃力の半分。負数にする事でダメージ計算時に回復する)
                damagevalue = (int)(atkpoint * -0.5f);
                break;

            case SkillDefine.Skill.FireBall:// スキル：ファイアボール

                // ダメージ半減
                damagevalue /= 2;

                //スキル使用不可状態にする
                attackchara.isSkillLock = true;

                break;

            default:// スキル無しor通常攻撃時
                break;
        }

        return damagevalue;
    }

    //------------------------------------------------------------------------

    /// <summary>
	/// (敵のターン開始時に呼出)
	/// 敵キャラクターのうちいずれか一体を行動させてターンを終了する
	/// </summary>
    private void EnemyCommand()
    {
        // 生存中の敵キャラクター&&行動可能キャラクターのリストを作成する
        var enemyCharas = charactorManager.Charactors.Where(chara => chara.isEnemy && chara.isIncapacitated != true);
        var enemycharas = new List<Charactor>();

        // 全生存キャラクターから敵フラグの立っているキャラクターをリストに追加
        foreach (var enemyChara in enemyCharas)
            enemycharas.Add(enemyChara);

        // 攻撃可能なキャラクター・位置の組み合わせの内１つをランダムに取得
        var actionPlan = TargetFinder.GetRandomActionPlan(mapManager, charactorManager, enemycharas);

        // 組み合わせのデータが存在すれば攻撃開始
        if (actionPlan != null)
        {
            // 敵キャラクター移動処理
            actionPlan.charaData.MovePosition(actionPlan.toMoveBlock.XPos, actionPlan.toMoveBlock.ZPos);
            // 敵キャラクター攻撃処理

            // (移動後のタイミングで攻撃開始するよう遅延実行)
            DOVirtual.DelayedCall(1.0f, () =>
            {
                CharaAttack(actionPlan.charaData, actionPlan.toAttackChara);
            });

            // 進行モード＜敵ターン：行動結果表示中＞へ変更
            ChangePhase(Phase.Enemyturn_Result);
            return;
        }

        // 攻撃可能な相手が見つからなかった場合
        // 移動させる１体をランダムに選ぶ
        int randValue = UnityEngine.Random.Range(0, enemycharas.Count);
        var chara = enemycharas[randValue];

        // 対象の移動可能場所リストの中から1つの場所をランダムに選ぶ
        reachableBlocks = mapManager.SearchReachableBlocks(chara.XPos, chara.ZPos);
        if(reachableBlocks.Count > 0)
        {
            randValue = UnityEngine.Random.Range(0, reachableBlocks.Count-1);
            MapBlock toMoveblock = reachableBlocks[randValue];// 移動対象のブロックデータ
            chara.MovePosition(toMoveblock.XPos, toMoveblock.ZPos);// 敵キャラクター移動処理
        }

        // 移動場所・攻撃場所リストをクリアする
        reachableBlocks.Clear();
        attackableBlocks.Clear();

        // (移動後のタイミングで処理するよう遅延実行)
        DOVirtual.DelayedCall(1.0f, () =>
        {
            //行動不能状態に
            chara.SetInCapacitited(true);
            // 進行モード＜敵ターン：開始時＞に変更
            ChangePhase(Phase.Enemyturn_Start, true);
        });
    }

    //------------------------------------------------------------------------

    /// <summary>
    /// 攻撃側・防御側の属性の相性によるダメージ倍率を返す
    /// </summary>
    /// <param name="attackChara">攻撃側のキャラ</param>
    /// <param name="defenseChara">防御側のキャラ</param>
    /// <returns></returns>
    private float GetDamegeRatioByAttribute(Charactor attackChara, Charactor defenseChara)
    {
        const float RATIO_NORMAL = 1.0f;//通常
        const float RATIO_GOOD = 1.2f;//効果抜群
        const float RATIO_BAD = 0.8f;//効果いまひとつ

        Charactor.Attribute atkAttr = attackChara.attribute;//攻撃側の属性
        Charactor.Attribute defAttr = defenseChara.attribute;//防御側の属性

        switch(atkAttr)
        {
            case Charactor.Attribute.Water://攻撃側の属性：水属性
                if (defAttr == Charactor.Attribute.Fire)
                    return RATIO_GOOD;

                else if (defAttr == Charactor.Attribute.Soil)
                    return RATIO_BAD;

                else
                    return RATIO_NORMAL;

            case Charactor.Attribute.Fire://攻撃側の属性：火属性
                if (defAttr == Charactor.Attribute.Wind)
                    return RATIO_GOOD;

                else if (defAttr == Charactor.Attribute.Water)
                    return RATIO_BAD;

                else
                    return RATIO_NORMAL;

            case Charactor.Attribute.Wind://攻撃側の属性：風属性
                if (defAttr == Charactor.Attribute.Soil)
                    return RATIO_GOOD;

                else if (defAttr == Charactor.Attribute.Fire)
                    return RATIO_BAD;

                else
                    return RATIO_NORMAL;

            case Charactor.Attribute.Soil://攻撃側の属性：土属性
                if (defAttr == Charactor.Attribute.Water)
                    return RATIO_GOOD;

                else if (defAttr == Charactor.Attribute.Wind)
                    return RATIO_BAD;

                else
                    return RATIO_NORMAL;

            default:
                return RATIO_NORMAL;
        }
    }

    //------------------------------------------------------------------------

    public void CancelMoving()
    {
        mapManager.AllSelectionModeClear();

        reachableBlocks.Clear();

        ClearSelectingChara();

        guiManager.ShowMoveCancelButton(false);

        ChangePhase(Phase.Myturn_Start, true);
    }

    //------------------------------------------------------------------------

    public void CheckGameSet()
    {
        //敵キャラが居るかチェック
        var Enemychara = charactorManager.Charactors.FirstOrDefault(chara => chara.isEnemy == true);

        //味方キャラが居るかチェック
        var Chara = charactorManager.Charactors.FirstOrDefault(chara => chara.isEnemy == false);

        if(!Chara || !Enemychara)
        {
            //ゲーム終了フラグをtrue
            isGameSet = true;
            if(Chara)
            {
                List<Charactor> charalist = new List<Charactor>();
                foreach (var chara in charactorManager.Charactors)
                {
                    if (!chara.isEnemy)
                        charalist.Add(chara);
                }
                //セーブデータ
                DataManager._instance.WriteSaveData(charalist, SceneManager.GetActiveScene().name);
            }
            

            //ゲーム終了フラグを遅延処理
            DOVirtual.DelayedCall(
                1.5f, () =>
                 {
                     if (Chara)//味方キャラがいる場合ゲームクリア時のロゴ表示
                     {
                         guiManager.ShowLogo_GameClear();
                     }
                         

                     if (Enemychara)//敵キャラがいる場合ゲームオーバー時のロゴ表示
                         guiManager.ShowLogo_gameOver();

                     guiManager.FadeIn(5.0f);
                 });

            // Gameシーンの再読み込み(遅延実行)
            DOVirtual.DelayedCall(
                7.0f, () =>
                {
                    //guiManager.FadeIn_FadeOut(false, 1.0f);

                    SceneManager.LoadScene("MainMenu");
                });
        }
    }

    /// <summary>
	/// 行動内容決定ボタン処理
	/// </summary>
    public void ActionDecideButton()
    {
        // 行動決定・キャンセルボタンを非表示にする
        guiManager.HideDecideButtons();
        // 攻撃先のブロックの強調表示を解除する
        attackBlock.SetSelectionMode(MapBlock.Highlight.Off);

        // 攻撃対象の位置に居るキャラクターのデータを取得
        var targetChara =
            charactorManager.GetCharactor(attackBlock.XPos, attackBlock.ZPos);
        if (targetChara != null)
        {// 攻撃対象のキャラクターが存在する
         // キャラクター攻撃処理
            CharaAttack(selectingChara, targetChara);

            // 進行モードを進める(行動結果表示へ)
            ChangePhase(Phase.Myturn_Result);
            return;
        }
        else
        {// 攻撃対象が存在しない

            //行動不能状態
            selectingChara.SetInCapacitited(true);
            // 進行モードを進める(敵のターンへ)
            ChangePhase(Phase.Myturn_Start, true);
        }
    }

    /// <summary>
    /// 行動内容リセットボタン処理
    /// </summary>
    public void ActionCancelButton()
    {
        // 行動決定・キャンセルボタンを非表示にする
        guiManager.HideDecideButtons();
        // 攻撃先のブロックの強調表示を解除する
        attackBlock.SetSelectionMode(MapBlock.Highlight.Off);

        // キャラクターを移動前の位置に戻す
        selectingChara.MovePosition(charaStartPos_X, charaStartPos_Z);
        // キャラクターの選択を解除する
        ClearSelectingChara();

        // 進行モードを戻す(ターンの最初へ)
        ChangePhase(Phase.Myturn_Start, true);
    }
}