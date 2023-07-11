using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class ReactiveProperty_TesSample : MonoBehaviour
{
//--------------------------------------------------------------------------------

    ReactiveProperty<int> samplereactive = new ReactiveProperty<int>();

//--------------------------------------------------------------------------------

    [SerializeField]
    private IntReactiveProperty playerHealth = new IntReactiveProperty(100);

//--------------------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {

//--------------------------------------------------------------------------------

        var rp = new ReactiveProperty<int>(10);

        samplereactive = rp;

        rp.Value = 20;
        var currentvlue = rp.Value;

        rp.Value = 30;

        rp.Subscribe(x => Debug.Log(x));

        //--------------------------------------------------------------------------------

        playerHealth.Subscribe(x => Debug.Log("IntType : " + x));

//--------------------------------------------------------------------------------

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.A))
        {
            samplereactive.Value += 10;
        }
    }
}
