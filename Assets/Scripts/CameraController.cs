using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UniRx;

public class CameraController : MonoBehaviour
{
    // カメラ移動用変数
    private bool isCameraRotate; // カメラ回転中フラグ
    private bool isMirror; // 回転方向反転フラグ

    private bool isForward;
    private bool isBack;
    private bool isRight;
    private bool isLeft;

    // 定数定義
    const float SPEED = 30.0f; // 回転速度

    [SerializeField]
    public Vector3 offset = Vector3.zero;

    private Subject<Charactor> chara_subject = new Subject<Charactor>();

    public Subject<Charactor> get_chara_subject
    {
        get { return chara_subject; }
    }

    private void Start()
    {
        offset = this.transform.position - Vector3.zero;

        chara_subject.Subscribe(chara => this.transform.position = chara.transform.position + offset);
    }

    void Update()
    {
        // カメラ回転処理
        if (isCameraRotate)
        {
            float speed = SPEED * Time.deltaTime;

            if (isMirror)
                speed *= -1.0f;

            transform.RotateAround(Vector3.zero, Vector3.up, speed);
        }

        if(isForward)
        {
            float speed = SPEED * Time.deltaTime;

            var velocity = Vector3.zero;
            var rotation = transform.rotation;
            rotation.x = 0;
            velocity.z = speed;
            transform.position += rotation * velocity;
        }

        if(isBack)
        {
            float speed = SPEED * Time.deltaTime;

            var velocity = Vector3.zero;
            var rotation = transform.rotation;
            rotation.x = 0;
            velocity.z = -speed;
            transform.position += rotation * velocity;
        }

        if(isRight)
        {
            float speed = SPEED * Time.deltaTime;

            transform.position += transform.TransformDirection(Vector3.right) * speed;
        }

        if(isLeft)
        {
            float speed = SPEED * Time.deltaTime;

            transform.position += transform.TransformDirection(Vector3.left) * speed;
        }
    }

    /// <summary>
	/// カメラ移動ボタンが押し始められた時に呼び出される処理
	/// </summary>
	/// <param name="rightMode">右向きフラグ(右移動ボタンから呼ばれた時trueになっている)</param>
    public void CameraRotate_Start(bool rightMode)
    {
        isCameraRotate = true;

        isMirror = rightMode;
    }

    /// <summary>
	/// カメラ移動ボタンが押されなくなった時に呼び出される処理
	/// </summary>
    public void CameraRotate_End()
    {
        isCameraRotate = false;
    }

    public void CameraMove_Start(string dir)
    {
        switch(dir)
        {
            case "isForward":
                isForward = true;
                break;

            case "isBack":
                isBack = true;
                break; 

            case "isRight":
                isRight = true;
                break; 

            case "isLeft":
                isLeft = true;
                break; 
        }
    }

    public void CameraMove_End(string dir)
    {
        switch (dir)
        {
            case "isForward":
                isForward = false;
                break;

            case "isBack":
                isBack = false;
                break;

            case "isRight":
                isRight = false;
                break;

            case "isLeft":
                isLeft = false;
                break;
        }
    }
}