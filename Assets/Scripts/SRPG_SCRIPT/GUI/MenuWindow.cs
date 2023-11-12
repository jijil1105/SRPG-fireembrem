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
    Button close_soundsetting_button;

    //-------------------------------------------------------------------------

    [SerializeField]
    GameObject menubuttons;

    //-------------------------------------------------------------------------

    [SerializeField]
    Image soundsetting_background;
    [SerializeField]
    Slider bgm_slider;
    [SerializeField]
    Slider se_slider;
    [SerializeField]
    Slider master_slider;

    //-------------------------------------------------------------------------

    enum MenueButton
    {
        MenuButton,
        ResumeButton,
        RetireButton,
        SoundSetting,
        CloseSoundSettingWindow
    }

    //-------------------------------------------------------------------------

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

        bgm_slider.value = 1;
        se_slider.value = 1;
        master_slider.value = 1;

        bgm_slider.onValueChanged.AddListener(SetVolumeBGM);
        se_slider.onValueChanged.AddListener(SetVolumeSE);
        master_slider.onValueChanged.AddListener(SetVolumeMaser);
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

    void SetVolumeBGM(float value)
    {
        value /= 5;

        var volume = Mathf.Clamp(Mathf.Log10(value) * 20f, -80f, 0f);
        volume += AudioManager.instance.initVolume;
        AudioManager.instance.audioMixer.SetFloat("BGM", volume);
        Debug.Log($"BGM:{volume}");
    }

    void SetVolumeSE(float value)
    {
        value /= 5;

        var volume = Mathf.Clamp(Mathf.Log10(value) * 20f, -80f, 0f);
        volume += AudioManager.instance.initVolume;
        AudioManager.instance.audioMixer.SetFloat("SE", volume);
        Debug.Log($"SE:{volume}");
    }

    void SetVolumeMaser(float value)
    {
        value /= 5;

        var volume = Mathf.Clamp(Mathf.Log10(value) * 20f, -80f, 0f);
        volume += AudioManager.instance.initVolume;
        AudioManager.instance.audioMixer.SetFloat("Master", volume);
        Debug.Log($"Master:{volume}");
    }
}
