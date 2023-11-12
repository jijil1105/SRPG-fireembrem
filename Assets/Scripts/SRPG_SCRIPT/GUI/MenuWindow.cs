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
    [SerializeField]
    Button menu_button;
    [SerializeField]
    Button soundsetting_button;
    [SerializeField]
    GameObject menubuttons;
    [SerializeField]
    Button close_soundsetting_button;
    [SerializeField]
    Image soundsetting_background;

    enum MenueButton
    {
        MenuButton,
        ResumeButton,
        RetireButton,
        SoundSetting,
        CloseSoundSettingWindow
    }

    public bool isRetire = false;

    public bool isMulti = false;

    //IDisposable disposable1;
    //IDisposable disposable2;

    void Start()
    {
        menu_button.onClick.AsObservable().Subscribe(
            _ => Click_Button(MenueButton.MenuButton)).AddTo(this);

        resum_button.onClick.AsObservable().Subscribe(
            _ => Click_Button(MenueButton.ResumeButton)).AddTo(this);

        retire_button.onClick.AsObservable().Subscribe(
            _ => isRetire = true).AddTo(this);

        retire_button.onClick.AsObservable().Subscribe(
            _ => Click_Button(MenueButton.RetireButton)).AddTo(this);

        soundsetting_button.onClick.AsObservable().Subscribe(
            _ => Click_Button(MenueButton.SoundSetting)).AddTo(this);

        close_soundsetting_button.onClick.AsObservable().Subscribe(
            _ => Click_Button(MenueButton.CloseSoundSettingWindow)).AddTo(this);

        menubuttons.SetActive(false);
        soundsetting_background.gameObject.SetActive(false);
    }

    void Update()
    {
        
    }

    void Click_Button(MenueButton menueButton)
    {
        AudioManager.instance.Play("SE_1");

        switch (menueButton)
        {
            case MenueButton.MenuButton:
                menubuttons.SetActive(true);
                break;

            case MenueButton.ResumeButton:
                menubuttons.SetActive(false);
                break;

            case MenueButton.RetireButton:
                menubuttons.SetActive(false);

                if (!isMulti)
                {
                    var manager = FindObjectOfType<GameManager>();
                    if (manager)
                    {
                        manager.Retire();
                    }
                }

                if (isMulti)
                {
                    var manager_multi = FindObjectOfType<GameManager_Multi>();
                    if (manager_multi)
                    {
                        manager_multi.Retire_multi_game();
                    }
                }
                break;

            case MenueButton.SoundSetting:
                soundsetting_background.gameObject.SetActive(true);
                break;

            case MenueButton.CloseSoundSettingWindow:
                soundsetting_background.gameObject.SetActive(false);
                break;

            default:
                break;
        }
    }
}
