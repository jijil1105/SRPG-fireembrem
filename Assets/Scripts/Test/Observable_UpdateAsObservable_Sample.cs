using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class Observable_UpdateAsObservable_Sample : MonoBehaviour
{
    [SerializeField]
    private float intervalSecound = 2;
    // Start is called before the first frame update
    void Start()
    {
        /*this.UpdateAsObservable()
            .Subscribe(
            _ => Debug.Log(_),
            () => Debug.Log("OnCompleted"));

        this.OnDestroyAsObservable()
            .Subscribe(_ => Debug.Log(_));

        Destroy(gameObject, 2.0f);*/

        //---------------------------------------------------------------------------

        this.UpdateAsObservable()
            .Where(_ => Input.GetKey(KeyCode.Z))
            .ThrottleFirst(TimeSpan.FromSeconds(intervalSecound))
            .Subscribe(_ => Attack());
    }

    void Attack()
    {
        Debug.Log("Attack");
    }
}
