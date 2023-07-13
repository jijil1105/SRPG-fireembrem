using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class OnClickAsObservable_Sample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var button = GetComponent<Button>();
        button.OnClickAsObservable().Subscribe();

        var _inputField = GetComponent<InputField>();
        _inputField.OnValueChangedAsObservable().Subscribe();
        _inputField.OnEndEditAsObservable().Subscribe();

        var slider = GetComponent<Slider>();
        slider.OnValueChangedAsObservable().Subscribe();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
