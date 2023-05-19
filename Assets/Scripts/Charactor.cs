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

    //-------------------------------------------------------------------------

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

    //-------------------------------------------------------------------------

    private int xPos;
    private int zPos;
    private int nowHp;

    public int XPos { get => xPos; set => xPos = value; }
    public int ZPos { get => zPos; set => zPos = value; }
    public int NowHp { get => nowHp; set => nowHp = value; }

    //-------------------------------------------------------------------------

    public enum Attribute
    {
        Water,
        Fire,
        Wind,
        Soil
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
    public void AttackAnimation(Charactor targetCharaData)
    {
        // 攻撃アニメーション(DoTween)
        // 相手キャラクターの位置へジャンプで近づき、同じ動きで元の場所に戻る
        transform.DOJump(targetCharaData.transform.position, 1.0f, 1, 0.5f)
            .SetEase(Ease.Linear) // イージング(変化の度合)を設定
            .SetLoops(2, LoopType.Yoyo);// ループ回数・ループ方式を指定
    }
}