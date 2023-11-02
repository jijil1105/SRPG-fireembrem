using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;
using UniRx;
using System;

public class Character_Multi : MonoBehaviourPunCallbacks
{
    // キャラクター初期設定(インスペクタから入力)
    [Header("Init Position(-4~4)"), SerializeField]
    public int initPos_X;//初期位置：X/
    [Header("Init Position(-4~4)"), SerializeField]
    public int initPos_Z;//初期位置：Z/
    [Header("EnemyFlg true: EnemyCharactor")]
    public bool isEnemy;// 敵フラグ/
    [Header("Charactor's Name")]
    public string charaName;//キャラ名
    [Header("maxHP")]
    public int maxHP;//最大Hp
    [Header("atk")]
    public int atk;//物理攻撃力
    [Header("def")]
    public int def;//物理防御力
    [Header("magic atk")]
    public int Int;//魔法攻撃力
    [Header("magic def")]
    public int Res;//魔法防御力
    [Header("Attribute")]
    public Attribute attribute;// 属性
    [Header("移動方法")]
    public MoveType moveType;//移動タイプ
    [Header("Skill")]
    public SkillDefine.Skill skill;//スキル
    [Header("魔法攻撃フラグ")]
    public bool isMagicAttac;//魔法攻撃フラグ

    //-------------------------------------------------------------------------
    // ゲーム中に変化するキャラクターデータ

    public int xPos;// 現在のx座標
    public int zPos;// 現在のz座標
    public int nowHp;//現在のHp

    public int Lv;//レベル
    public int nowExp;//現在の経験値
    public int ExpPerLv;//次のレベルに必要な経験値

    // 各種状態異常
    public bool isSkillLock;// 特技使用不可状態
    public bool isDefBreak;//　防御力０化デバフ
    public bool isIncapacitated;// 行動不能状態

    //-------------------------------------------------------------------------

    // キャラクター属性定義(列挙型)
    public enum Attribute
    {
        Water,
        Fire,
        Wind,
        Soil
    }

    //-------------------------------------------------------------------------

    // キャラクター移動方法定義(列挙型)
    public enum MoveType
    {
        Rook,
        Bishop,
        Queen,
    }

    //-------------------------------------------------------------------------

    //Main Camera
    private Camera MainCamera;

    //カメラがキャラクターを追従するかのフラグ
    bool following_to_chara;

    //-------------------------------------------------------------------------

    void Start()
    {
        // Set object's init position from chractor's data
        //キャラクターの初期位置
        Vector3 pos = new Vector3();
        pos.x = initPos_X;
        xPos = initPos_X;

        pos.y = 1.0f;

        pos.z = initPos_Z;
        zPos = initPos_Z;
        transform.position = pos;

        //キャラクターのHP初期化
        nowHp = maxHP;

        //Set camera from MainCamera
        //メインカメラ取得
        MainCamera = Camera.main;

        //マルチモードではキャラクターを追従はしない
        following_to_chara = false;

        //ルームのホストの場合は自軍キャラ、ホストでない場合は敵軍キャラ
        if (PhotonNetwork.MasterClient.UserId==PhotonNetwork.LocalPlayer.UserId)
        {
            if(photonView.IsMine)
            {
                isEnemy = false;
            }            
        }
        else
        {
            if (photonView.IsMine)
            {
                isEnemy = true;
            }
        }
    }

    //-------------------------------------------------------------------------

    // ビルボード処理
    // (スプライトオブジェクトをメインカメラの方向に向ける)
    void Update()
    {
        /*Vector3 camerPos = MainCamera.transform.position;
        camerPos.y = transform.position.y;
        transform.LookAt(MainCamera.transform);
        //MainCamera.transform.LookAt(this.transform);
        */

        if(following_to_chara)
        {
            MainCamera.GetComponent<CameraController>().get_chara_subject_Multi.OnNext(this);
        }
    }

    //-------------------------------------------------------------------------

    /// <summary>
    /// キャラの情報を同期、photonの仕様で親オブジェクトから子オブジェクトのPunRPC属性の関数を呼べないので一度通常の関数を経由
    /// </summary>
    public void SyncInfo()
    {
        photonView.RPC(nameof(Syncinfo), RpcTarget.All, this);
    }

    /// <summary>
    /// キャラの初期情報を同期
    /// </summary>
    /// <param name="chara"></param>
    [PunRPC]
    void Syncinfo(Character_Multi chara)
    {
        if(!photonView.IsMine)
        {
            this.initPos_X = chara.initPos_X;
            this.initPos_Z = chara.initPos_Z;
            this.maxHP = chara.maxHP;
            this.atk = chara.atk;
            this.def = chara.def;
            this.Int = chara.Int;
            this.Res = chara.Res;
            this.xPos = chara.xPos;
            this.zPos = chara.zPos;
            this.nowHp = chara.nowHp;
        }
    }

    //-------------------------------------------------------------------------

    /// <summary>
    /// キャラ座標変数を同期
    /// </summary>
    /// <param name="pos_x"></param>
    /// <param name="pos_z"></param>
    [PunRPC]
    void SyncPos(int pos_x, int pos_z)
    {
        //他プレイヤーのキャラだった場合に座標を同期させる
        if (!photonView.IsMine)
        {
            xPos = pos_x;
            zPos = pos_z;
        }
    }

    //-------------------------------------------------------------------------

    /// <summary>
    /// 攻撃を受けた時などにHPを同期、photonの仕様で親オブジェクトから子オブジェクトのPunRPC属性の関数を呼べないので一度通常の関数を経由
    /// </summary>
    public void SyncHp()
    {
        photonView.RPC(nameof(SyncHP), RpcTarget.All, this);
    }

    /// <summary>
    /// キャラのステータスを同期
    /// </summary>
    /// <param name="chara"></param>
    [PunRPC]
    void SyncHP(Character_Multi chara)
    {
        this.nowHp = chara.nowHp;
    }

    //-------------------------------------------------------------------------

    /// <summary>
	/// 対象の座標へとキャラクターを移動させる
	/// </summary>
	/// <param name="targetXPos">x座標</param>
	/// <param name="targetZPos">z座標</param>
    public void MovePosition(int targetXPos, int targetZPos)
    {
        //カメラに移動中キャラを追従させる
        following_to_chara = true;

        //移動距離計算
        Vector3 movePos = Vector3.zero;
        movePos.x = targetXPos - xPos;
        movePos.z = targetZPos - zPos;


        // DoTweenのTweenを使用して徐々に位置が変化するアニメーションを行う
        transform.DOMove(movePos, 0.5f).SetEase(Ease.Linear).SetRelative();

        // キャラクターデータに位置を保存
        xPos = targetXPos;
        zPos = targetZPos;

        //移動完了まで遅延
        DOVirtual.DelayedCall(0.5f, () =>
        {
            //キャラ追従をやめる
            following_to_chara = false;
        });

        //キャラクターの座標を変数を同期
        photonView.RPC(nameof(SyncPos), RpcTarget.All, xPos, zPos);
    }

    /// <summary>
	/// キャラクターの近接攻撃アニメーション
	/// </summary>
	/// <param name="targetCharaData">相手キャラクター</param>
    public void AttackAnimation(Charactor targetCharaData, SkillDefine.Skill skill)
    {
        switch (skill)
        {
            case SkillDefine.Skill._None:
                // 攻撃アニメーション(DoTween)
                // 相手キャラクターの位置へジャンプで近づき、同じ動きで元の場所に戻る
                transform.DOJump(targetCharaData.transform.position, 1.0f, 1, 0.5f)
                    .SetEase(Ease.Linear) // イージング(変化の度合)を設定
                    .SetLoops(2, LoopType.Yoyo);// ループ回数・ループ方式を指定

                // アニメーション内で攻撃が当たったくらいのタイミングでSEを再生
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    AudioManager.instance.Play("SE_2");
                });
                break;

            case SkillDefine.Skill.Critical:
                // 攻撃アニメーション(DoTween)
                // 相手キャラクターの位置へジャンプで近づき、同じ動きで元の場所に戻る
                transform.DOJump(targetCharaData.transform.position, 1.0f, 1, 0.5f)
                    .SetEase(Ease.Linear) // イージング(変化の度合)を設定
                    .SetLoops(2, LoopType.Yoyo);// ループ回数・ループ方式を指定

                // アニメーション内で攻撃が当たったくらいのタイミングでSEを再生
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    AudioManager.instance.Play("SE_2");
                });
                break;

            case SkillDefine.Skill.DefBreak:
                // 攻撃アニメーション(DoTween)
                // 相手キャラクターの位置へジャンプで近づき、同じ動きで元の場所に戻る
                transform.DOJump(targetCharaData.transform.position, 1.0f, 1, 0.5f)
                    .SetEase(Ease.Linear) // イージング(変化の度合)を設定
                    .SetLoops(2, LoopType.Yoyo);// ループ回数・ループ方式を指定

                // アニメーション内で攻撃が当たったくらいのタイミングでSEを再生
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    AudioManager.instance.Play("SE_2");
                });
                break;

            case SkillDefine.Skill.Heal:
                /*// 攻撃アニメーション(DoTween)
                // 相手キャラクターの位置へジャンプで近づき、同じ動きで元の場所に戻る
                transform.DOJump(targetCharaData.transform.position, 1.0f, 1, 0.5f)
                    .SetEase(Ease.Linear) // イージング(変化の度合)を設定
                    .SetLoops(2, LoopType.Yoyo);// ループ回数・ループ方式を指定

                // アニメーション内で攻撃が当たったくらいのタイミングでSEを再生
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    AudioManager.instance.Play("SE_2");
                });*/
                break;

            case SkillDefine.Skill.FireBall:
                /*// 攻撃アニメーション(DoTween)
                // 相手キャラクターの位置へジャンプで近づき、同じ動きで元の場所に戻る
                transform.DOJump(targetCharaData.transform.position, 1.0f, 1, 0.5f)
                    .SetEase(Ease.Linear) // イージング(変化の度合)を設定
                    .SetLoops(2, LoopType.Yoyo);// ループ回数・ループ方式を指定

                // アニメーション内で攻撃が当たったくらいのタイミングでSEを再生
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    AudioManager.instance.Play("SE_2");
                });*/
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetCharaData"></param>
    /// <param name="skill"></param>
    public void AttackAnimation(Character_Multi targetCharaData, SkillDefine.Skill skill)
    {
        switch (skill)
        {
            case SkillDefine.Skill._None:
                // 攻撃アニメーション(DoTween)
                // 相手キャラクターの位置へジャンプで近づき、同じ動きで元の場所に戻る
                transform.DOJump(targetCharaData.transform.position, 1.0f, 1, 0.5f)
                    .SetEase(Ease.Linear) // イージング(変化の度合)を設定
                    .SetLoops(2, LoopType.Yoyo);// ループ回数・ループ方式を指定

                // アニメーション内で攻撃が当たったくらいのタイミングでSEを再生
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    AudioManager.instance.Play("SE_2");
                });
                break;

            case SkillDefine.Skill.Critical:
                // 攻撃アニメーション(DoTween)
                // 相手キャラクターの位置へジャンプで近づき、同じ動きで元の場所に戻る
                transform.DOJump(targetCharaData.transform.position, 1.0f, 1, 0.5f)
                    .SetEase(Ease.Linear) // イージング(変化の度合)を設定
                    .SetLoops(2, LoopType.Yoyo);// ループ回数・ループ方式を指定

                // アニメーション内で攻撃が当たったくらいのタイミングでSEを再生
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    AudioManager.instance.Play("SE_2");
                });
                break;

            case SkillDefine.Skill.DefBreak:
                // 攻撃アニメーション(DoTween)
                // 相手キャラクターの位置へジャンプで近づき、同じ動きで元の場所に戻る
                transform.DOJump(targetCharaData.transform.position, 1.0f, 1, 0.5f)
                    .SetEase(Ease.Linear) // イージング(変化の度合)を設定
                    .SetLoops(2, LoopType.Yoyo);// ループ回数・ループ方式を指定

                // アニメーション内で攻撃が当たったくらいのタイミングでSEを再生
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    AudioManager.instance.Play("SE_2");
                });
                break;

            case SkillDefine.Skill.Heal:
                /*// 攻撃アニメーション(DoTween)
                // 相手キャラクターの位置へジャンプで近づき、同じ動きで元の場所に戻る
                transform.DOJump(targetCharaData.transform.position, 1.0f, 1, 0.5f)
                    .SetEase(Ease.Linear) // イージング(変化の度合)を設定
                    .SetLoops(2, LoopType.Yoyo);// ループ回数・ループ方式を指定

                // アニメーション内で攻撃が当たったくらいのタイミングでSEを再生
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    AudioManager.instance.Play("SE_2");
                });*/
                break;

            case SkillDefine.Skill.FireBall:
                /*// 攻撃アニメーション(DoTween)
                // 相手キャラクターの位置へジャンプで近づき、同じ動きで元の場所に戻る
                transform.DOJump(targetCharaData.transform.position, 1.0f, 1, 0.5f)
                    .SetEase(Ease.Linear) // イージング(変化の度合)を設定
                    .SetLoops(2, LoopType.Yoyo);// ループ回数・ループ方式を指定

                // アニメーション内で攻撃が当たったくらいのタイミングでSEを再生
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    AudioManager.instance.Play("SE_2");
                });*/
                break;
        }
    }

    /// <summary>
    /// 移行を行ったキャラを行動不能状態にする
    /// </summary>
    /// <param name="cap">trueなら行動不能：falseなら行動可能</param>
    public void SetInCapacitited(bool cap)
    {
        isIncapacitated = cap;

        if (!cap)
            this.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);

        else
            this.GetComponent<SpriteRenderer>().color = new Color32(100, 100, 100, 255);
    }
}