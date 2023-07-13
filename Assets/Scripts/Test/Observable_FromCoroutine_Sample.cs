using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class Observable_FromCoroutine_Sample : MonoBehaviour
{
    public bool IsPaused { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        Observable.FromCoroutine<int>(observer => GameTimerCoro(observer, 60))
            .Subscribe(po => Debug.Log(po));

        Observable_EveryUpdate_Sample.FpsReactiveProperty.Subscribe(fps => Debug.Log(fps));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator GameTimerCoro(IObserver<int> observer, int initializeCount)
    {
        var pp = initializeCount;

        while(pp > 0)
        {
            if(!IsPaused)
            {
                observer.OnNext(pp--);
            }
            yield return new WaitForSeconds(1);
        }

        observer.OnNext(0);
        observer.OnCompleted();
    }
}
