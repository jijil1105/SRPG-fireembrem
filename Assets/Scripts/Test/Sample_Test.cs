using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class Sample_Test : MonoBehaviour
{
    private CharacterController charactercontroller;

    private BoolReactiveProperty isJumping = new BoolReactiveProperty();

    void Start()
    {
        charactercontroller = GetComponent<CharacterController>();

        this.UpdateAsObservable()
            .Where(_ => !isJumping.Value)
            .Select(_ => new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")))
            .Where(x => x.magnitude > 0.1f)
            .Subscribe(x => Move(x.normalized));

        //ジャンプ中でないならジャンプする
        this.UpdateAsObservable()
            .Where(_ => Input.GetKeyDown(KeyCode.Space) && !isJumping.Value && charactercontroller.isGrounded)
            .Subscribe(_ =>
            {
                Jump();
                isJumping.Value = true;
            });

        //着地フラグが変化したときにジャンプ中フラグを戻す
        charactercontroller
            .ObserveEveryValueChanged(x => x.isGrounded)
            .Where(x => x && isJumping.Value)
            .Subscribe(_ => isJumping.Value = false)
            .AddTo(gameObject);

        //ジャンプ中フラグがfalseになったら効果音を鳴らす
        isJumping.Where(x => !x)
            .Subscribe(_ => PlaySound());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Jump()
    {
        this.transform.position += Vector3.up;
        Debug.Log("Jump");
    }

    private void Move(Vector3 direction)
    {
        Debug.Log("X : " + direction.x + " Y : " + direction.y + " Z : " + direction.z);
    }

    private void PlaySound()
    {
        Debug.Log("Play Sound");
    }
}
