using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class Observable_EveryUpdate_Sample
{
    [SerializeField] const int BufferSize = 5;
    public static IReadOnlyReactiveProperty<float> FpsReactiveProperty;

    static Observable_EveryUpdate_Sample()
    {
        FpsReactiveProperty = Observable.EveryUpdate()
            .Select(_ => Time.deltaTime)   
            .Buffer(BufferSize, 1)         
            .Select(x => 1.0f / x.Average()) 
            .ToReadOnlyReactiveProperty();

        FpsReactiveProperty.Subscribe(x => Debug.Log(x));
    }
}
