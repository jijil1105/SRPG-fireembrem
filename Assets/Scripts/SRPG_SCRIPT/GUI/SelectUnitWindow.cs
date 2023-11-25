using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class SelectUnitWindow : MonoBehaviour
{
    [SerializeField] Button select_unit_button;
    [SerializeField] Button setting_button;
    [SerializeField] Button start_button;

    enum Buttons
    {
        SelectUnitBtn,
        SettingBtn,
        StartBtn
    }

    void Start()
    {
        select_unit_button.OnClickAsObservable().Subscribe(
            _ => OnClick(Buttons.SelectUnitBtn)).AddTo(this);

        setting_button.OnClickAsObservable().Subscribe(
            _ => OnClick(Buttons.SettingBtn));

        start_button.OnClickAsObservable().Subscribe(
            _ => OnClick(Buttons.StartBtn));
    }

    void Update()
    {
        
    }

    void OnClick(Buttons buttons)
    {
        switch(buttons)
        {
            case Buttons.SelectUnitBtn:
                break;

            case Buttons.SettingBtn:
                break;

            case Buttons.StartBtn:
                break;
        }
    }
}
