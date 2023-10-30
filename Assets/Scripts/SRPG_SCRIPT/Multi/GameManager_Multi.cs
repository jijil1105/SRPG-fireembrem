using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Cysharp.Threading.Tasks;
using UniRx;
using System;

public class GameManager_Multi : MonoBehaviourPunCallbacks
{
    //-------------------------------------------------------------------------
    //各マネージャークラスの宣言

    private MapManager mapManager;//フィールドのブロックを管理するマネージャークラス
    private CharactorManager charactorManager;//フィールドのキャラクターを管理するクラス
    private GUIManager_Multi guiManager_multi;//UIを管理するクラス

    //-------------------------------------------------------------------------

    private bool isGameSet;//ゲーム終了フラグ

    //-------------------------------------------------------------------------

    // 行動キャンセル処理用変数
    private int charaStartPos_X; // 選択キャラクターの移動前の位置(X方向)
    private int charaStartPos_Z; // 選択キャラクターの移動前の位置(Z方向)
    private MapBlock attackBlock; // 選択キャラクターの攻撃先のブロック

    //-------------------------------------------------------------------------

    private Character_Multi selectingChara;//選択中のキャラクター（マップフィールド上のキャラクターを選択していない時はnull）
    private List<MapBlock> reachableBlocks;//選択キャラの移動可能範囲
    private List<MapBlock> attackableBlocks;//選択キャラの攻撃可能範囲
    private SkillDefine.Skill selectingSkill;//選択中の特技(通常攻撃時はNONE固定

    private enum Phase
    {
        Myturn_Start,//自ターン：開始時
        Myturn_Moving,//自ターン：移動先選択中
        Myturn_Command,//自ターン：移動後のコマンド選択中（現在は攻撃と待機を選択可)
        Myturn_Targeting,//自ターン：攻撃対象を選択中
        Myturn_Result,//自ターン：行動結果表示中
        Enemyturn_Start,//敵ターン：開始時
        Enemyturn_Moving,//敵ターン：移動先選択中
        Enemyturn_Command,//敵ターン：移動後のコマンド選択中（現在は攻撃と待機を選択可）
        Enemyturn_Targeting,//敵ターン：攻撃対象を選択中
        Enemyturn_Result//敵ターン：行動結果表示中
    }

    private Phase nowPhase;//現在の進行モード

    //プレイヤー人数監視
    [SerializeField] ReactiveProperty<int> num_of_player;

    //------------------------------------------------------------------------

    //プレイヤーの人数が揃った場合True
    bool isAbleGame = false;

    //------------------------------------------------------------------------

    async void  Start()
    {
        //変数の初期化
        mapManager = GetComponent<MapManager>();//
        charactorManager = GetComponent<CharactorManager>();
        guiManager_multi = GetComponent<GUIManager_Multi>();
        reachableBlocks = new List<MapBlock>();
        attackableBlocks = new List<MapBlock>();
        nowPhase = Phase.Myturn_Start;
        AudioManager.instance.Play("BGM_1");

        //
        await UniTask.Delay(TimeSpan.FromMilliseconds(0.05), cancellationToken: this.GetCancellationTokenOnDestroy());

        //現在ルームにいるプレイヤー人数
        num_of_player.Value = PhotonNetwork.CurrentRoom.PlayerCount;

        //プレイヤー人数が２人ならゲーム可能
        num_of_player.Subscribe(x =>
        {
            if (x == 2)
                isAbleGame = true;
            else
                isAbleGame = false;
        }).AddTo(this);

        //プレイヤーが揃うまでの待機ウィンドウ表示
        guiManager_multi.ShowWaitingWindow();

        //プレイヤーが揃うまで待機
        await UniTask.WaitUntil(() => isAbleGame, cancellationToken: this.GetCancellationTokenOnDestroy());

        //PhotonviewIDから対戦相手のオブジェクトを取得する為、ID取得
        int[] photonviewID = new int[charactorManager.Charactors_Multis.Count];

        for (int i = 0; i < photonviewID.Length; i++)
            photonviewID[i] = charactorManager.Charactors_Multis[i].GetComponent<PhotonView>().ViewID;
        
        //対戦相手のオブジェクトをキャラクターマネージャーの管理変数に追加
        photonView.RPC(nameof(AddObject), RpcTarget.All, photonviewID);

        var chara_datas = charactorManager.Charactors_Multis;

        foreach (var Chara in chara_datas)
            Chara.SyncInfo();

        if(PhotonNetwork.MasterClient.UserId==PhotonNetwork.LocalPlayer.UserId)
        {
            //カメラの初期座標を自軍キャラを真ん中に写す様に設定
            var character = charactorManager.Charactors_Multis.FirstOrDefault(chara => chara.isIncapacitated != true && chara.isEnemy != true);
            Camera.main.GetComponent<CameraController>().get_chara_subject_Multi.OnNext(character);
        }
        else
        {
            //カメラの初期座標を自軍キャラを真ん中に写す様に設定
            var character = charactorManager.Charactors_Multis.FirstOrDefault(chara => chara.isIncapacitated != true && chara.isEnemy == true);
            Camera.main.GetComponent<CameraController>().get_chara_subject_Multi.OnNext(character);
        }

        //待機ウィンドウ非表示
        guiManager_multi.HideWaitingWindow();

        //戦闘BGM再生
        AudioManager.instance.Play("BGM_1");
    }

    //-------------------------------------------------------------------------

    //ボタンを押している間ずっと呼ばれてしまうのでボタンを押してから一度だけ処理を行うようにしています
    bool isCalledOnce = false;

    void Update()
    {
        //Playerの人数が２人じゃないならリターン
        if (!isAbleGame)
            return;

        //ゲーム終了後なら終了
        if (isGameSet)
            return;

        if (!isCalledOnce)
        {
            //入力の検出とUIへのタップ検出
            if (Input.GetMouseButton(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {//UIではない部分をタップした際にタップ先のブロックを取得
                isCalledOnce = true;
                AudioManager.instance.Play("SE_1");// Play SE
                GetMapBlockByTapPos();
            }
        }

        if (!Input.GetMouseButton(0)) { isCalledOnce = false; }
    }

    //-------------------------------------------------------------------------

    /// <summary>
    /// プレイヤーがルームに入室し時のコールバック
    /// </summary>
    /// <param name="newPlayer"></param>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        //プレイヤー人数を更新
        num_of_player.Value = PhotonNetwork.CurrentRoom.PlayerCount;
    }

    //-------------------------------------------------------------------------

    /// <summary>
    /// photonViewIDから検索して対戦相手のキャラクター取得
    /// </summary>
    /// <param name="senderView"></param>
    [PunRPC]
    void AddObject(int[] senderView)
    {
        foreach(var viewid in senderView)
        {
            //IDからオブジェクト検索
            var obj = PhotonView.Find(viewid).transform;

            //自分のオブジェクトならリターン
            if (obj.GetComponent<PhotonView>().IsMine)
                return;

            //対戦相手のキャラをキャラクターマネジャーの管理変数に追加
            Character_Multi chara = obj.GetComponent<Character_Multi>();
            charactorManager.Charactors_Multis.Add(chara);
        }
    }

    //-------------------------------------------------------------------------

    /// <summary>
    /// 現在のターンを同期
    /// </summary>
    /// <param name="phase"></param>
    [PunRPC]
    private void Synchronization(Phase phase)
    {
        nowPhase = phase;

        Debug.Log(phase);
    }

    //-------------------------------------------------------------------------

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
        if (targetObject != null)
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
        //ルームのホストはMyturn、子クライアントならEnemyTurnで行動可能
        //現在の進行モードによって異なる処理を開始する
        switch (nowPhase)
        {
            //自分のターン：開始時
            case Phase.Myturn_Start:
                //ルームのホストなら処理
                if (PhotonNetwork.MasterClient.UserId==PhotonNetwork.LocalPlayer.UserId)
                {
                    //全ブロックの選択状態解除
                    mapManager.AllSelectionModeClear();
                    //ブロックを選択状態として白色に強調表示にする
                    targetBlock.SetSelectionMode(MapBlock.Highlight.Select);

                    //選択したブロックの座標にキャラクターが居ればキャラクター情報取得、居なければnull
                    Character_Multi charaData = charactorManager.GetCharactor_Multi(targetBlock.XPos, targetBlock.ZPos);

                    //選択したブロックの座標にキャラクターが居た場合の処理
                    if (charaData)
                    {
                        //自分のオブジェクトなら処理
                        if (charaData.GetComponent<PhotonView>().IsMine)
                        {
                            //選択ブロックに居たキャラクターの記憶
                            selectingChara = charaData;
                            // 選択キャラクターの現在位置を記憶
                            charaStartPos_X = selectingChara.xPos;
                            charaStartPos_Z = selectingChara.zPos;
                            //選択キャラのステータスをUI表示する
                            guiManager_multi.ShowStatusWindow(charaData);

                            //行動不能状態なら処理しない
                            if (charaData.isIncapacitated)
                                return;

                            //選択キャラの移動可能ブロックリストを取得
                            reachableBlocks = mapManager.SearchReachableBlocks_Multi(charaData.xPos, charaData.zPos);

                            //移動可能ブロックを青色に強調表示
                            foreach (MapBlock mapblock in reachableBlocks)
                                mapblock.SetSelectionMode(MapBlock.Highlight.Reachable);

                            //移動キャンセルボタン表示
                            guiManager_multi.ShowMoveCancelButton(true);

                            //進行モード＜自分のターン：移動先選択中＞に変更
                            ChangePhase(Phase.Myturn_Moving);
                        }
                        //他プレイヤーのオブジェクトなら処理無し
                        else
                        {
                            //選択中キャラを初期化
                            ClearSelectingChara();
                        }
                    }

                    else
                    {//選択ブロック座標にキャラが居ない場合
                        //選択中キャラを初期化
                        ClearSelectingChara();
                    }
                }

                break;

            //自分のターン：移動先選択中
            case Phase.Myturn_Moving:

                //ルームのホストなら処理
                if (PhotonNetwork.MasterClient.UserId==PhotonNetwork.LocalPlayer.UserId)
                {
                    //選択キャラが敵キャラクターなら移動先選択状態を解除
                    if (selectingChara.isEnemy)
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
                        guiManager_multi.ShowMoveCancelButton(false);

                        // 0.5秒数経過後に処理を実行する
                        DOVirtual.DelayedCall(0.5f, () =>
                        {  //コマンドボタン表示
                            guiManager_multi.ShowCommandButtons(selectingChara);

                            //進行モード＜自分のターン：移動後のコマンド選択中＞に変更
                            ChangePhase(Phase.Myturn_Command);
                        });
                    }
                }
                break;
            //自分のターン：移動後のコマンド選択中
            case Phase.Myturn_Command:
                if(PhotonNetwork.MasterClient.UserId==PhotonNetwork.LocalPlayer.UserId)
                {
                    //ルームのホストなら処理
                    // キャラクター攻撃処理
                    // (攻撃可能ブロックを選択した場合に攻撃処理を呼び出す)
                    if (attackableBlocks.Contains(targetBlock))
                    {
                        // 攻撃先のブロック情報を記憶
                        attackBlock = targetBlock;

                        // 行動決定・キャンセルボタンを表示する
                        guiManager_multi.ShowDecideButtons();

                        // 攻撃可能な場所リストを初期化する
                        attackableBlocks.Clear();

                        // 全ブロックの選択状態を解除
                        mapManager.AllSelectionModeClear();

                        // 攻撃先のブロックを強調表示する
                        attackBlock.SetSelectionMode(MapBlock.Highlight.Attackable);

                        // 進行モードを進める：攻撃の対象を選択中
                        ChangePhase(Phase.Myturn_Targeting);
                    }
                }
                break;

            //敵ターン：開始時
            case Phase.Enemyturn_Start:

                //ルームのホスト出ない場合はEnemyTurnで処理
                if (PhotonNetwork.MasterClient.UserId!=PhotonNetwork.LocalPlayer.UserId)
                {
                    //全ブロックの選択状態解除
                    mapManager.AllSelectionModeClear();
                    //ブロックを選択状態として白色に強調表示にする
                    targetBlock.SetSelectionMode(MapBlock.Highlight.Select);

                    //選択したブロックの座標にキャラクターが居ればキャラクター情報取得、居なければnull
                    Character_Multi EnemyData = charactorManager.GetCharactor_Multi(targetBlock.XPos, targetBlock.ZPos);

                    //選択したブロックの座標にキャラクターが居た場合の処理
                    if (EnemyData)
                    {
                        if (EnemyData.GetComponent<PhotonView>().IsMine)
                        {
                            //選択ブロックに居たキャラクターの記憶
                            selectingChara = EnemyData;
                            // 選択キャラクターの現在位置を記憶
                            charaStartPos_X = selectingChara.xPos;
                            charaStartPos_Z = selectingChara.zPos;
                            //選択キャラのステータスをUI表示する
                            guiManager_multi.ShowStatusWindow(EnemyData);

                            //行動不能状態なら処理しない
                            if (EnemyData.isIncapacitated)
                                return;

                            //選択キャラの移動可能ブロックリストを取得
                            reachableBlocks = mapManager.SearchReachableBlocks_Multi(EnemyData.xPos, EnemyData.zPos);

                            //移動可能ブロックを青色に強調表示
                            foreach (MapBlock mapblock in reachableBlocks)
                                mapblock.SetSelectionMode(MapBlock.Highlight.Reachable);

                            //移動キャンセルボタン表示
                            guiManager_multi.ShowMoveCancelButton(true);

                            //進行モード＜自分のターン：移動先選択中＞に変更
                            ChangePhase(Phase.Enemyturn_Moving);
                        }
                        else
                        {
                            //選択中キャラを初期化
                            ClearSelectingChara();
                        }
                    }

                    else
                    {//選択ブロック座標にキャラが居ない場合
                        //選択中キャラを初期化
                        ClearSelectingChara();
                    }
                }

                break;

            //敵ターン：移動先選択中
            case Phase.Enemyturn_Moving:
                //ルームのホスト出ない場合はEnemyTurnで処理
                if (PhotonNetwork.MasterClient.UserId!=PhotonNetwork.LocalPlayer.UserId)
                {
                    //選択キャラが敵キャラクターなら移動先選択状態を解除
                    if (!selectingChara.isEnemy)
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
                        guiManager_multi.ShowMoveCancelButton(false);

                        // 0.5秒数経過後に処理を実行する
                        DOVirtual.DelayedCall(0.5f, () =>
                        {  //コマンドボタン表示
                            guiManager_multi.ShowCommandButtons(selectingChara);

                            //進行モード＜自分のターン：移動後のコマンド選択中＞に変更
                            ChangePhase(Phase.Enemyturn_Command);
                        });
                    }
                }
                break;
            //敵ターン：移動後のコマンド選択中
            case Phase.Enemyturn_Command:
                //ルームのホスト出ない場合はEnemyTurnで処理
                if (PhotonNetwork.MasterClient.UserId!=PhotonNetwork.LocalPlayer.UserId)
                {
                    // キャラクター攻撃処理
                    // (攻撃可能ブロックを選択した場合に攻撃処理を呼び出す)
                    if (attackableBlocks.Contains(targetBlock))
                    {
                        // 攻撃先のブロック情報を記憶
                        attackBlock = targetBlock;

                        // 行動決定・キャンセルボタンを表示する
                        guiManager_multi.ShowDecideButtons();

                        // 攻撃可能な場所リストを初期化する
                        attackableBlocks.Clear();

                        // 全ブロックの選択状態を解除
                        mapManager.AllSelectionModeClear();

                        // 攻撃先のブロックを強調表示する
                        attackBlock.SetSelectionMode(MapBlock.Highlight.Attackable);

                        // 進行モードを進める：攻撃の対象を選択中
                        ChangePhase(Phase.Enemyturn_Targeting);
                    }
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
        guiManager_multi.HideStatusWindow();
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

        //現在のターンを同期
        photonView.RPC(nameof(Synchronization), RpcTarget.All, nowPhase);


        // 特定のモードに切り替わったタイミングで行う処理
        switch (nowPhase)
        {
            // 自分のターン：開始時
            case Phase.Myturn_Start:
                //ルームのホストなら処理
                if (PhotonNetwork.MasterClient.UserId==PhotonNetwork.LocalPlayer.UserId)
                {
                    //行動可能な自キャラが居るかチェック
                    var chara = charactorManager.Charactors_Multis.FirstOrDefault(chara => chara.isIncapacitated != true && chara.isEnemy != true);
                    //動かせるキャラが居なかった場合
                    if (!chara)
                    {
                        //味方キャラ取得
                        var charas = charactorManager.Charactors_Multis.Where(chara => chara.isEnemy == false);
                        //行動可能にする
                        foreach (var charaData in charas)
                            charaData.SetInCapacitited(false);
                        //ターンを変更
                        ChangePhase(Phase.Enemyturn_Start);

                        guiManager_multi.Sync_Show_Logo(true);
                    }
                }

                break;

            // 敵のターン：開始時
            case Phase.Enemyturn_Start:
                //ルームのホストじゃないなら処理
                if (PhotonNetwork.MasterClient.UserId!=PhotonNetwork.LocalPlayer.UserId)
                {
                    //行動可能キャラ取得
                    var enemy = charactorManager.Charactors_Multis.FirstOrDefault(chara => chara.isIncapacitated != true && chara.isEnemy == true);

                    //動かせるキャラが居なかった場合
                    if (!enemy)
                    {
                        //敵キャラ取得
                        var charas = charactorManager.Charactors_Multis.Where(chara => chara.isEnemy == true);
                        //行動可能にする
                        foreach (var charaData in charas)
                            charaData.SetInCapacitited(false);
                        //ターンを変更
                        ChangePhase(Phase.Myturn_Start);

                        guiManager_multi.Sync_Show_Logo(false);
                    }
                }

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
        //SE再生
        AudioManager.instance.Play("SE_1");

        // コマンドボタンを非表示にする
        guiManager_multi.HideCommandButtons();

        //行動不能状態にする
        selectingChara.SetInCapacitited(true);

        //選択キャラ初期化
        selectingChara = null;

        //ルームのホストなら処理
        if (PhotonNetwork.MasterClient.UserId==PhotonNetwork.LocalPlayer.UserId)
        {
            // 進行モード＜自ターン：開始時＞に変更
            ChangePhase(Phase.Myturn_Start, true);
        }

        //ルームのホストで無いなら処理
        if (PhotonNetwork.MasterClient.UserId!=PhotonNetwork.LocalPlayer.UserId)
        {
            // 進行モード＜敵ターン：開始時＞に変更
            ChangePhase(Phase.Enemyturn_Start, true);
        }
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
        guiManager_multi.HideCommandButtons();

        // （特技：ファイアボールの場合はマップ全域に対応）
        if (selectingSkill == SkillDefine.Skill.FireBall)
            attackableBlocks = mapManager.MapBlocksToList();

        // 攻撃可能な場所リストを取得する
        else
            attackableBlocks = mapManager.SearchAttackableBlocks(selectingChara.xPos, selectingChara.zPos);

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
    private void CharaAttack(Character_Multi attackchara, Character_Multi defensechara)
    {
        // ダメージ計算処理
        int damagevalue;// ダメージ量
        int atkpoint = attackchara.atk;
        int defpoint = defensechara.def;
        float delay_time = 2.0f;//遅延時間

        if (!attackchara.isMagicAttac)//攻撃キャラが物理攻撃の場合
        {
            atkpoint = attackchara.atk;// 攻撃側の物理攻撃力
            defpoint = defensechara.def;// 防御側の物理防御力
        }
        else if (attackchara.isMagicAttac)//攻撃キャラが魔法攻撃の場合
        {
            atkpoint = attackchara.Int;// 攻撃側の魔法攻撃力
            defpoint = defensechara.Res;// 防御側の魔法防御力
        }

        //防御力０化デバフ状態なら防御力０でダメージ計算
        if (defensechara.isDefBreak)
            defpoint = 0;

        damagevalue = atkpoint - defpoint;// ダメージ＝攻撃力－防御力で計算

        //属性相性を反映したダメージ数取得
        float ratio = GetDamegeRatioByAttribute(attackchara, defensechara);
        damagevalue = (int)(damagevalue * ratio);

        // ダメージ量が0以下なら0にする
        if (damagevalue < 0)
            damagevalue = 0;

        //スキル選択ならスキルのダメージ倍率適応
        damagevalue = SkillAttack(damagevalue, atkpoint, attackchara, defensechara);

        // キャラクター攻撃アニメーション
        attackchara.AttackAnimation(defensechara, selectingSkill);
        //
        int id = defensechara.GetComponent<PhotonView>().ViewID;
        // バトル結果表示ウィンドウの表示設定
        guiManager_multi.battleWindowUI.Show_Window(id, damagevalue);

        // ダメージ量分防御側のHPを減少
        defensechara.nowHp -= damagevalue;
        // HPが0～最大値の範囲に収まるよう補正
        defensechara.nowHp = Mathf.Clamp(defensechara.nowHp, 0, defensechara.maxHP);

        //攻撃を受けるキャラのHPを同期する
        defensechara.SyncHp();

        // HP0になったキャラクターを削除する
        if (defensechara.nowHp == 0)
        {
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
                guiManager_multi.battleWindowUI.Hide_Window();
                // ターンを切り替える
                if (nowPhase == Phase.Myturn_Result)
                {
                    //行動不能状態
                    attackchara.SetInCapacitited(true);

                    //ターンを変更
                    ChangePhase(Phase.Myturn_Start, true);
                }

                else if (nowPhase == Phase.Enemyturn_Result)
                {
                    //行動不能状態
                    attackchara.SetInCapacitited(true);

                    //ターンを変更
                    ChangePhase(Phase.Enemyturn_Start, true);
                }
            });
    }

    //------------------------------------------------------------------------

    /// <summary>
    /// スキル反映
    /// </summary>
    /// <param name="damagevalue"></param>
    /// <param name="atkpoint"></param>
    /// <param name="attackchara"></param>
    /// <param name="defensechara"></param>
    /// <returns></returns>
    public int SkillAttack(int damagevalue, int atkpoint, Character_Multi attackchara, Character_Multi defensechara)
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


  
    //------------------------------------------------------------------------

    /// <summary>
    /// 攻撃側・防御側の属性の相性によるダメージ倍率を返す
    /// </summary>
    /// <param name="attackChara">攻撃側のキャラ</param>
    /// <param name="defenseChara">防御側のキャラ</param>
    /// <returns></returns>
    private float GetDamegeRatioByAttribute(Character_Multi attackChara, Character_Multi defenseChara)
    {
        const float RATIO_NORMAL = 1.0f;//通常
        const float RATIO_GOOD = 1.2f;//効果抜群
        const float RATIO_BAD = 0.8f;//効果いまひとつ

        Character_Multi.Attribute atkAttr = attackChara.attribute;//攻撃側の属性
        Character_Multi.Attribute defAttr = defenseChara.attribute;//防御側の属性

        switch (atkAttr)
        {
            case Character_Multi.Attribute.Water://攻撃側の属性：水属性
                if (defAttr == Character_Multi.Attribute.Fire)
                    return RATIO_GOOD;

                else if (defAttr == Character_Multi.Attribute.Soil)
                    return RATIO_BAD;

                else
                    return RATIO_NORMAL;

            case Character_Multi.Attribute.Fire://攻撃側の属性：火属性
                if (defAttr == Character_Multi.Attribute.Wind)
                    return RATIO_GOOD;

                else if (defAttr == Character_Multi.Attribute.Water)
                    return RATIO_BAD;

                else
                    return RATIO_NORMAL;

            case Character_Multi.Attribute.Wind://攻撃側の属性：風属性
                if (defAttr == Character_Multi.Attribute.Soil)
                    return RATIO_GOOD;

                else if (defAttr == Character_Multi.Attribute.Fire)
                    return RATIO_BAD;

                else
                    return RATIO_NORMAL;

            case Character_Multi.Attribute.Soil://攻撃側の属性：土属性
                if (defAttr == Character_Multi.Attribute.Water)
                    return RATIO_GOOD;

                else if (defAttr == Character_Multi.Attribute.Wind)
                    return RATIO_BAD;

                else
                    return RATIO_NORMAL;

            default:
                return RATIO_NORMAL;
        }
    }

    //------------------------------------------------------------------------

    /// <summary>
    /// 行動キャンセル
    /// </summary>
    /// <param name="isMyturn"></param>
    public void CancelMoving()
    {
        mapManager.AllSelectionModeClear();

        reachableBlocks.Clear();

        ClearSelectingChara();

        guiManager_multi.ShowMoveCancelButton(false);

        if(PhotonNetwork.MasterClient.UserId==PhotonNetwork.LocalPlayer.UserId)
            ChangePhase(Phase.Myturn_Start, true);

        else
            ChangePhase(Phase.Enemyturn_Start, true);
    }

    //------------------------------------------------------------------------

    //------------------------------------------------------------------------

    /// <summary>
    /// ゲーム終了かどうかを他プレイヤーと同時にチェックする
    /// </summary>
    public void gamecheck_RPC()
    {
        photonView.RPC(nameof(CheckGameSet_Multi), RpcTarget.All);
    }

    /// <summary>
    /// マルチモード時にゲーム終了条件が揃っているかチェック
    /// </summary>
    [PunRPC]
    public void CheckGameSet_Multi()
    {
        Debug.Log("Check GameSet");
        //敵キャラが居るかチェック
        var Enemychara = charactorManager.Charactors_Multis.FirstOrDefault(chara => chara.isEnemy == true);

        //味方キャラが居るかチェック
        var Chara = charactorManager.Charactors_Multis.FirstOrDefault(chara => chara.isEnemy == false);

        if (!Chara || !Enemychara)
        {
            //ゲーム終了フラグをtrue
            isGameSet = true;
            if (Chara)
            {
                List<Character_Multi> charalist = new List<Character_Multi>();
                foreach (var chara in charactorManager.Charactors_Multis)
                {
                    if (!chara.isEnemy)
                        charalist.Add(chara);
                }
            }

            //ゲーム終了フラグを遅延処理
            DOVirtual.DelayedCall(
                1.5f, () =>
                {
                    //ホストなら処理
                    if (PhotonNetwork.MasterClient.UserId==PhotonNetwork.LocalPlayer.UserId)
                    {
                        if (Chara)//味方キャラがいる場合ゲームクリア時のロゴ表示
                            guiManager_multi.ShowLogo_GameClear();

                        if (Enemychara)//敵キャラがいる場合ゲームオーバー時のロゴ表示
                            guiManager_multi.ShowLogo_gameOver();
                        //画面をフェードイン
                        guiManager_multi.FadeIn(5.0f);
                    }
                    else
                    {
                        if (Chara)//味方キャラがいる場合ゲームオーバーのロゴ表示
                            guiManager_multi.ShowLogo_gameOver();

                        if (Enemychara)//敵キャラがいる場合ゲームクリアのロゴ表示
                            guiManager_multi.ShowLogo_GameClear();

                        guiManager_multi.FadeIn(5.0f);
                    }
                });

            // Gameシーンの再読み込み(遅延実行)
            DOVirtual.DelayedCall(
                7.0f, () =>
                {
                    //guiManager.FadeIn_FadeOut(false, 1.0f);

                    Camera.main.GetComponent<CameraController>().get_chara_subject.OnCompleted();

                    PhotonNetwork.Disconnect();

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
        guiManager_multi.HideDecideButtons();
        // 攻撃先のブロックの強調表示を解除する
        attackBlock.SetSelectionMode(MapBlock.Highlight.Off);

        // 攻撃対象の位置に居るキャラクターのデータを取得
        var targetChara =
            charactorManager.GetCharactor_Multi(attackBlock.XPos, attackBlock.ZPos);


        if(PhotonNetwork.MasterClient.UserId==PhotonNetwork.LocalPlayer.UserId)
        {
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

        if(PhotonNetwork.MasterClient.UserId!=PhotonNetwork.LocalPlayer.UserId)
        {
            if (targetChara != null)
            {// 攻撃対象のキャラクターが存在する
             // キャラクター攻撃処理
                CharaAttack(selectingChara, targetChara);

                // 進行モードを進める(行動結果表示へ)
                ChangePhase(Phase.Enemyturn_Result);
                return;
            }
            else
            {// 攻撃対象が存在しない

                //行動不能状態
                selectingChara.SetInCapacitited(true);
                // 進行モードを進める(敵のターンへ)
                ChangePhase(Phase.Enemyturn_Start, true);
            }
        }
    }

    /// <summary>
    /// 行動内容リセットボタン処理
    /// </summary>
    public void ActionCancelButton()
    {
        // 行動決定・キャンセルボタンを非表示にする
        guiManager_multi.HideDecideButtons();
        // 攻撃先のブロックの強調表示を解除する
        attackBlock.SetSelectionMode(MapBlock.Highlight.Off);

        // キャラクターを移動前の位置に戻す
        selectingChara.MovePosition(charaStartPos_X, charaStartPos_Z);
        // キャラクターの選択を解除する
        ClearSelectingChara();

        if(PhotonNetwork.MasterClient.UserId==PhotonNetwork.LocalPlayer.UserId)
        {
            // 進行モードを戻す(ターンの最初へ)
            ChangePhase(Phase.Myturn_Start, true);
        }

        if(PhotonNetwork.MasterClient.UserId!=PhotonNetwork.LocalPlayer.UserId)
        {
            // 進行モードを戻す(ターンの最初へ)
            ChangePhase(Phase.Enemyturn_Start, true);
        }
    }
}

/// <summary>
/// PhotonViewRPCでCharacter＿Multiを使用可能にするシリアライザー
/// </summary>
public static class CharacterSerializer
{
    public static void Register()
    {
        ExitGames.Client.Photon.PhotonPeer.RegisterType(typeof(Character_Multi), (byte)'C', SerializeCharacter_Muliti, DeserializeCharacterMulti);
    }

    private static byte[] SerializeCharacter_Muliti(object i_customobject)
    {
        Character_Multi character_Multi = (Character_Multi)i_customobject;

        var bytes = new byte[(10 * sizeof(int))];
        int index = 0;
        ExitGames.Client.Photon.Protocol.Serialize(character_Multi.initPos_X, bytes, ref index);
        ExitGames.Client.Photon.Protocol.Serialize(character_Multi.initPos_Z, bytes, ref index);
        ExitGames.Client.Photon.Protocol.Serialize(character_Multi.maxHP, bytes, ref index);
        ExitGames.Client.Photon.Protocol.Serialize(character_Multi.atk, bytes, ref index);
        ExitGames.Client.Photon.Protocol.Serialize(character_Multi.def, bytes, ref index);
        ExitGames.Client.Photon.Protocol.Serialize(character_Multi.Int, bytes, ref index);
        ExitGames.Client.Photon.Protocol.Serialize(character_Multi.Res, bytes, ref index);
        ExitGames.Client.Photon.Protocol.Serialize(character_Multi.xPos, bytes, ref index);
        ExitGames.Client.Photon.Protocol.Serialize(character_Multi.zPos, bytes, ref index);
        ExitGames.Client.Photon.Protocol.Serialize(character_Multi.nowHp, bytes, ref index);

        return bytes;
    }

    private static object DeserializeCharacterMulti(byte[] i_bytes)
    {
        var character_multi = new Character_Multi();
        int index = 0;
        ExitGames.Client.Photon.Protocol.Deserialize(out character_multi.initPos_X, i_bytes, ref index);
        ExitGames.Client.Photon.Protocol.Deserialize(out character_multi.initPos_Z, i_bytes, ref index);
        ExitGames.Client.Photon.Protocol.Deserialize(out character_multi.maxHP, i_bytes, ref index);
        ExitGames.Client.Photon.Protocol.Deserialize(out character_multi.atk, i_bytes, ref index);
        ExitGames.Client.Photon.Protocol.Deserialize(out character_multi.def, i_bytes, ref index);
        ExitGames.Client.Photon.Protocol.Deserialize(out character_multi.Int, i_bytes, ref index);
        ExitGames.Client.Photon.Protocol.Deserialize(out character_multi.Res, i_bytes, ref index);
        ExitGames.Client.Photon.Protocol.Deserialize(out character_multi.xPos, i_bytes, ref index);
        ExitGames.Client.Photon.Protocol.Deserialize(out character_multi.zPos, i_bytes, ref index);
        ExitGames.Client.Photon.Protocol.Deserialize(out character_multi.nowHp, i_bytes, ref index);

        return character_multi;
    }
}

/// <summary>
/// 
/// </summary>
public static class ColorSerializer
{
    public static void Register()
    {
        ExitGames.Client.Photon.PhotonPeer.RegisterType(typeof(Color), (byte)'C', SerializeColor, DeserializeColor);
    }

    private static byte[] SerializeColor(object i_customobject)
    {
        Color color = (Color)i_customobject;

        var bytes = new byte[4 * sizeof(float)];
        int index = 0;
        ExitGames.Client.Photon.Protocol.Serialize(color.r, bytes, ref index);
        ExitGames.Client.Photon.Protocol.Serialize(color.g, bytes, ref index);
        ExitGames.Client.Photon.Protocol.Serialize(color.b, bytes, ref index);
        ExitGames.Client.Photon.Protocol.Serialize(color.a, bytes, ref index);

        return bytes;
    }

    private static object DeserializeColor(byte[] i_bytes)
    {
        var color = new Color();
        int index = 0;
        ExitGames.Client.Photon.Protocol.Deserialize(out color.r, i_bytes, ref index);
        ExitGames.Client.Photon.Protocol.Deserialize(out color.g, i_bytes, ref index);
        ExitGames.Client.Photon.Protocol.Deserialize(out color.b, i_bytes, ref index);
        ExitGames.Client.Photon.Protocol.Deserialize(out color.a, i_bytes, ref index);

        return color;
    }

} // class ColorSerializer 