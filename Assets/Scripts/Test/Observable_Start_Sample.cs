using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Net;
using System.IO;

public class Observable_Start_Sample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Observable.Start(() =>
        {
            var req = (HttpWebRequest)WebRequest.Create("http://google.com");
            var res = (HttpWebResponse)req.GetResponse();
            using (var reader = new StreamReader(res.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        })
        .ObserveOnMainThread()
        .Subscribe(x => Debug.Log(x));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
