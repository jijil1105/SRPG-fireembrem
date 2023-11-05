using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;

public class MenuWindow : MonoBehaviour
{
    [SerializeField]
    Button resum_button;

    [SerializeField]
    Button retire_button;

    //IDisposable disposable1;
    //IDisposable disposable2;

    void Start()
    {
        resum_button.onClick.AsObservable().Subscribe(
            _ => Click_Button()).AddTo(this);

        retire_button.onClick.AsObservable().Subscribe(
            _ => Click_Button()).AddTo(this);
    }

    void Update()
    {
        
    }

    void Click_Button()
    {
        Debug.Log("OnClick!");
    }

    /*private void OnDestroy()
    {
        disposable1?.Dispose();
        disposable2?.Dispose();
    }*/
}
