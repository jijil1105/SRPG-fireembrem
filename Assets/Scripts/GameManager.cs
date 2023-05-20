using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class GameManager : MonoBehaviour
{

/*  GameManagerで色々なクラスを使用してゲームの進行を行っています
 *  ここの処理ではゲームマネージャーのシングルトン化を行っています。
 */

    private void Awake()
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

    public static GameManager instance;

    //-------------------------------------------------------------------------
    //各マネージャークラスの宣言

    private MapManager mapManager;//フィールドのブロックを管理するマネージャークラス
    private CharactorManager charactorManager;//フィールドのキャラクターを管理するクラス
    private GUIManager guiManager;//UIを管理するクラス

    //-------------------------------------------------------------------------

    private Charactor selectingChara;//選択中のキャラクター（マップフィールド上のキャラクターを選択していない時はnull）
    private List<MapBlock> reachableBlocks;//選択キャラの移動可能範囲
    private List<MapBlock> attackableBlocks;//選択キャラの攻撃可能範囲

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

    //変数の初期化

    private void Start()
    {
        mapManager = GetComponent<MapManager>();//
        charactorManager = GetComponent<CharactorManager>();
        guiManager = GetComponent<GUIManager>();

        reachableBlocks = new List<MapBlock>();
        attackableBlocks = new List<MapBlock>();

        nowPhase = Phase.Myturn_Start;

        AudioManager.instance.Play("BGM_1");
    }

    //-------------------------------------------------------------------------

    //ボタンを押している間ずっと呼ばれてしまうのでボタンを押してから一度だけ処理を行うようにしています

    bool isCalledOnce = false;

    void Update()
    {
        if (Input.GetKey(KeyCode.C))
            AudioManager.instance.Play("BGM_1");
        
        if(!isCalledOnce)
        {
            //入力の検出とUIへのタップ検出
            if (Input.GetMouseButton(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {//UIではない部分をタップした際にタップ先のブロックを取得
                isCalledOnce = true;

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
                    //選択キャラのステータスをUI表示する
                    guiManager.ShowStatusWindow(charaData);
                    //選択キャラの移動可能ブロックリストを取得
                    reachableBlocks = mapManager.SearchReachableBlocks(charaData.XPos, charaData.ZPos);

                    //移動可能ブロックを青色に強調表示
                    foreach (MapBlock mapblock in reachableBlocks)
                        mapblock.SetSelectionMode(MapBlock.Highlight.Reachable);

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
                    Debug.Log("Tapped on Block  Position : " + targetBlock.transform.position);
                }

                break;

            //自分のターン：移動先選択中
            case Phase.Myturn_Moving:
                // 選択ブロックが移動可能な場所リスト内にある場合、移動処理を開始
                if (reachableBlocks.Contains(targetBlock))
                {
                    //選択キャラを選択ブロックへ移動
                    selectingChara.MovePosition(targetBlock.XPos, targetBlock.ZPos);
                    //移動可能ブロックリストを初期化
                    reachableBlocks.Clear();
                    //全ブロックの選択状態解除
                    mapManager.AllSelectionModeClear();
                    // 0.5秒数経過後に処理を実行する
                    DOVirtual.DelayedCall(0.5f, () =>
                    {　　//コマンドボタン表示
                        guiManager.ShowCommandButtons();
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
                    // 攻撃可能ブロックをタップした時
                 　 // 攻撃可能な場所リストを初期化する
                    attackableBlocks.Clear();
                    // 全ブロックの選択状態を解除
                    mapManager.AllSelectionModeClear();

                    // 攻撃対象の位置に居るキャラクターのデータを取得
                    var targetChara = charactorManager.GetCharactor(targetBlock.XPos, targetBlock.ZPos);

                    // 攻撃対象のキャラクターが存在する場合の処理
                    if (targetChara != null)
                    {
                        // キャラクター攻撃処理
                        CharaAttack(selectingChara, targetChara);
                        //進行モード＜自分のターン：行動結果表示中＞に変更
                        ChangePhase(Phase.Myturn_Result);

                        return;
                    }

                    // 攻撃対象が存在しない
                    // 進行モード＜敵ターン：開始時＞に変更
                    else { ChangePhase(Phase.Enemyturn_Start); }
                }
                break;
        }
        
    }

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

    /// <summary>
	/// ターン進行モードを変更する
	/// </summary>
	/// <param name="NowPhase">変更先モード</param>
    private void ChangePhase(Phase NowPhase)
    {
        // モード変更を保存
        nowPhase = NowPhase;
        Debug.Log("Change" + nowPhase);

        // 特定のモードに切り替わったタイミングで行う処理
        switch (nowPhase)
        {
            // 自分のターン：開始時
            case Phase.Myturn_Start :
                // 自分のターン開始時のロゴを表示
                guiManager.ShowLogoChangeTurn(true);
                break;

            // 敵のターン：開始時
            case Phase.Enemyturn_Start :
                // 敵のターン開始時のロゴを表示
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

    /// <summary>
	/// 攻撃コマンドボタン処理
	/// </summary>
    public void AttackCommand()
    {
        // コマンドボタンを非表示にする
        guiManager.HideCommandButtons();

        // 攻撃可能な場所リストを取得する
        attackableBlocks = mapManager.SearchAttackableBlocks(selectingChara.XPos, selectingChara.ZPos);
        // 攻撃可能な場所リストを赤色に強調表示する
        foreach (MapBlock block in attackableBlocks)
        {
            block.SetSelectionMode(MapBlock.Highlight.Attackable);
        }
    }

    /// <summary>
    /// 待機コマンドボタン処理
    /// </summary>
    public void StandbyCommand()
    {
        // コマンドボタンを非表示にする
        guiManager.HideCommandButtons();
        // 進行モード＜敵ターン：開始時＞に変更
        ChangePhase(Phase.Enemyturn_Start);
    }

    /// <summary>
	/// キャラクターが他のキャラクターに攻撃する処理
	/// </summary>
	/// <param name="attackchara">攻撃側キャラデータ</param>
	/// <param name="defensechara">防御側キャラデータ</param>
    private void CharaAttack(Charactor attackchara, Charactor defensechara)
    {
        // ダメージ計算処理
        int damagevalue;// ダメージ量
        int atkpoint = attackchara.atk;// 攻撃側の攻撃力
        int defpoint = defensechara.def;// 防御側の防御力

        damagevalue = atkpoint - defpoint;// ダメージ＝攻撃力－防御力で計算

        float ratio = GetDamegeRatioByAttribute(attackchara, defensechara);
        damagevalue = (int)(damagevalue * ratio);

        // ダメージ量が0以下なら0にする
        if (damagevalue < 0)
            damagevalue = 0;

        // キャラクター攻撃アニメーション
        attackchara.AttackAnimation(defensechara);

        // バトル結果表示ウィンドウの表示設定
        guiManager.battleWindowUI.ShowWindow(defensechara, damagevalue);

        // ダメージ量分防御側のHPを減少
        defensechara.NowHp -= damagevalue;
        // HPが0～最大値の範囲に収まるよう補正
        defensechara.NowHp = Mathf.Clamp(defensechara.NowHp, 0, defensechara.maxHP);

        // HP0になったキャラクターを削除する
        if (defensechara.NowHp == 0)
            charactorManager.DeleteCharaData(defensechara);

        // ターン切り替え処理(遅延実行)
        DOVirtual.DelayedCall(
            2.0f,// 遅延時間(秒)
            () =>
            {// 遅延実行する内容

                // ウィンドウを非表示化
                guiManager.battleWindowUI.HideWindow();
                // ターンを切り替える
                if (nowPhase == Phase.Myturn_Result) { ChangePhase(Phase.Enemyturn_Start); }

                else if(nowPhase == Phase.Enemyturn_Result) { ChangePhase(Phase.Myturn_Start); }
                
        });

        Debug.Log("Atk:" + attackchara.charaName + " Def:" + defensechara.charaName);
    }

    /// <summary>
	/// (敵のターン開始時に呼出)
	/// 敵キャラクターのうちいずれか一体を行動させてターンを終了する
	/// </summary>
    private void EnemyCommand()
    {
        // 生存中の敵キャラクターのリストを作成する
        var enemyCharas = charactorManager.Charactors.Where(chara => chara.isEnemy);
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
        int randValue = Random.Range(0, enemycharas.Count);
        var chara = enemycharas[randValue];

        // 対象の移動可能場所リストの中から1つの場所をランダムに選ぶ
        reachableBlocks = mapManager.SearchReachableBlocks(chara.XPos, chara.ZPos);
        if(reachableBlocks.Count > 0)
        {
            randValue = Random.Range(0, reachableBlocks.Count-1);
            MapBlock toMoveblock = reachableBlocks[randValue];// 移動対象のブロックデータ
            chara.MovePosition(toMoveblock.XPos, toMoveblock.ZPos);// 敵キャラクター移動処理
        }

        // 移動場所・攻撃場所リストをクリアする
        reachableBlocks.Clear();
        attackableBlocks.Clear();

        // (移動後のタイミングで処理するよう遅延実行)
        DOVirtual.DelayedCall(1.0f, () =>
        {
            // 進行モード＜自ターン：開始時＞に変更
            ChangePhase(Phase.Myturn_Start);
        });
    }

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
}