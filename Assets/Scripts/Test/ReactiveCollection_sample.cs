using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class ReactiveCollection_sample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var collection = new ReactiveCollection<string>();

        collection
            .ObserveAdd()
            .Subscribe(x =>
            {
                Debug.Log(string.Format("Add [{0}] = {1}", x.Index, x.Value));
            });

        collection.ObserveRemove()
            .Subscribe(x =>
            {
                Debug.Log(string.Format("Remove [{0}] = {1}"));
            });

        collection.Add("Apple");
        collection.Add("Baseball");
        collection.Add("Cherry");
        collection.Remove("Apple");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
