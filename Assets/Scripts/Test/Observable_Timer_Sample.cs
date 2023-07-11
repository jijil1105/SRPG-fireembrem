using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class Observable_Timer_Sample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Observable.Timer(System.TimeSpan.FromSeconds(5))
            .Subscribe(x =>
            {
                Debug.Log(x);
            });

        Observable.Timer(System.TimeSpan.FromSeconds(5), System.TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                Debug.Log("Play One Secound Span agin");
            })
            .AddTo(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
