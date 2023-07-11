using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class TimeCounter : MonoBehaviour
{
    [SerializeField] private int TimeLeft = 3;

    private Subject<int> timersubject = new Subject<int>();

    public Subject<int> OnTimeChanged
    {
        get { return timersubject; }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(TimerCoroutine());

        timersubject.Subscribe(x => Debug.Log(x));
    }

    IEnumerator TimerCoroutine()
    {
        yield return null;

        var time = TimeLeft;
        while(time >= 0)
        {
            timersubject.OnNext(time--);
            yield return new WaitForSeconds(1);
        }

        timersubject.OnCompleted();
    }
}
