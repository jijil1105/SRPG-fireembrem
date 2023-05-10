using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Charactor : MonoBehaviour
{
    // Start is called before the first frame update

    //Set charactor's init position from inspector
    [Header("Init Position(-4~4)"), SerializeField]
    public int initPos_X;
    [Header("Init Position(-4~4)"), SerializeField]
    public int initPos_Z;

    private int xPos;
    private int zPos;

    //Main Camera
    private Camera MainCamera;

    public int XPos { get => xPos; set => xPos = value; }
    public int ZPos { get => zPos; set => zPos = value; }

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

        //Set camera to MainCamera
        MainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.LookAt(MainCamera.transform);
        //MainCamera.transform.LookAt(this.transform);
    }

    public void MovePosition(int targetXPos, int targetZPos)
    {
        Vector3 movePos = Vector3.zero;
        movePos.x = targetXPos - XPos;
        movePos.z = targetZPos - ZPos;

        transform.position += movePos;

        XPos = targetXPos;
        ZPos = targetZPos;
    }
}
