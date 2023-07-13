using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

public class Observable_Return_Sample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var souce = Observable.Return(10);

        var Subscribe = souce.Subscribe(
            i => Console.WriteLine("OnNext{0}", i),
            ex => Console.WriteLine("OnError{0}", ex),
            () => Console.WriteLine("Completed()"));

        souce.Retry();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
