using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Charactor : MonoBehaviour
{
    // キャラクター初期設定(インスペクタから入力)
    [Header("Init Position(-4~4)"), SerializeField]
    public int initPos_X;
    [Header("Init Position(-4~4)"), SerializeField]
    public int initPos_Z;
    [Header("EnemyFlg true: EnemyCharactor")]
    public bool isEnemy;// 敵フラグ
    [Header("Charactor's Name")]
    public string charaName;
    [Header("maxHP")]
    public int maxHP;
    [Header("atk")]
    public int atk;
    [Header("def")]
    public int def;
    [Header("Attribute")]
    public Attribute attribute;// 属性
    [Header("移動方法")]
    public MoveType moveType;
    [Header("Skill")]
    public SkillDefine.Skill skill;

    //-------------------------------------------------------------------------
    // ゲーム中に変化するキャラクターデータ

    private int xPos;
    private int zPos;
    private int nowHp;

    public int XPos { get => xPos; set => xPos = value; }// 現在のx座標
    public int ZPos { get => zPos; set => zPos = value; }// 現在のz座標
    public int NowHp { get => nowHp; set => nowHp = value; }

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

    //-------------------------------------------------------------------------

    void Start()
    {
        // Set object's init position from chractor's data
        Vector3 pos = new Vector3();
        pos.x = initPos_X;
        XPos = initPos_X;

        pos.y = 1.0f;

        pos.z = initPos_Z;
        ZPos = initPos_Z;
        transform.position = pos;

        NowHp = maxHP;

        //Set camera from MainCamera
        MainCamera = Camera.main;
    }

    //-------------------------------------------------------------------------

    // ビルボード処理
    // (スプライトオブジェクトをメインカメラの方向に向ける)
    void Update()
    {
        Vector3 camerPos = MainCamera.transform.position;
        camerPos.y = transform.position.y;
        transform.LookAt(MainCamera.transform);
        //MainCamera.transform.LookAt(this.transform);
    }

    //-------------------------------------------------------------------------

    /// <summary>
	/// 対象の座標へとキャラクターを移動させる
	/// </summary>
	/// <param name="targetXPos">x座標</param>
	/// <param name="targetZPos">z座標</param>
    public void MovePosition(int targetXPos, int targetZPos)
    {
        Vector3 movePos = Vector3.zero;
        movePos.x = targetXPos - XPos;
        movePos.z = targetZPos - ZPos;

        // DoTweenのTweenを使用して徐々に位置が変化するアニメーションを行う
        transform.DOMove(movePos, 0.5f).SetEase(Ease.Linear).SetRelative();

        // キャラクターデータに位置を保存
        XPos = targetXPos;
        ZPos = targetZPos;
    }

    /// <summary>
	/// キャラクターの近接攻撃アニメーション
	/// </summary>
	/// <param name="targetCharaData">相手キャラクター</param>
    public void AttackAnimation(Charactor targetCharaData, SkillDefine.Skill skill)
    {
        switch(skill)
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

    public void SetInCapacitited(bool cap)
    {
        isIncapacitated = cap;

        if (!cap)
            this.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);

        else
            this.GetComponent<SpriteRenderer>().color = new Color32(100, 100, 100, 255);
    }
}