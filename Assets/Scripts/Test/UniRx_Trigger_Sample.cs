using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx.Triggers;
using UniRx;

public class UniRx_Trigger_Sample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var isForceEnabled = false;
        var rigitBody = GetComponent<Rigidbody>();

        this.FixedUpdateAsObservable()
            .Where(_ => isForceEnabled)
            .Subscribe( _ =>
            {
                rigitBody.AddForce(Vector3.up);
                Debug.Log("Enter");
            });

        this.OnTriggerEnterAsObservable()
            .Where(x => x.gameObject.tag == "WarpZone")
            .Subscribe(_ => isForceEnabled = true);

        this.OnTriggerExitAsObservable()
            .Where(x => x.gameObject.tag == "WarpZone")
            .Subscribe(_ =>
            {
                isForceEnabled = false;
                Debug.Log("Exit");
            });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
