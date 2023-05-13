using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Charactor : MonoBehaviour
{
    //Set charactor's init position from inspector
    [Header("Init Position(-4~4)"), SerializeField]
    public int initPos_X;
    [Header("Init Position(-4~4)"), SerializeField]
    public int initPos_Z;

    //-------------------------------------------------------------------------

    [Header("EnemyFlg true: EnemyCharactor")]
    public bool isEnemy;
    [Header("Charactor's Name")]
    public string charaName;
    [Header("maxHP")]
    public int maxHP;
    [Header("atk")]
    public int atk;
    [Header("def")]
    public int def;
    [Header("Attribute")]
    public Attribute attribute;

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
        // Set object's init position from chractor's position
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

    // Update is called once per frame
    void Update()
    {
        Vector3 camerPos = MainCamera.transform.position;
        camerPos.y = transform.position.y;
        transform.LookAt(MainCamera.transform);
        //MainCamera.transform.LookAt(this.transform);
    }

    //-------------------------------------------------------------------------

    public void MovePosition(int targetXPos, int targetZPos)
    {
        Vector3 movePos = Vector3.zero;
        movePos.x = targetXPos - XPos;
        movePos.z = targetZPos - ZPos;

        transform.DOMove(movePos, 0.5f).SetEase(Ease.Linear).SetRelative();

        XPos = targetXPos;
        ZPos = targetZPos;
    }

    public void AttackAnimation(Charactor targetCharaData)
    {
        transform.DOJump(targetCharaData.transform.position, 1.0f, 1, 0.5f).SetEase(Ease.Linear).SetLoops(2, LoopType.Yoyo);
    }
}
