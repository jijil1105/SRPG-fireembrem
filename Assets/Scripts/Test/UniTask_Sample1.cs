using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading;

public class UniTask_Sample1 : MonoBehaviour
{
    float _waitTime = 3f;
    private CancellationTokenSource _ct = new CancellationTokenSource();

    // Start is called before the first frame update
    async void Start()
    {
        await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space),cancellationToken: this.GetCancellationTokenOnDestroy());
        Debug.Log("hogehoge");

        await UniTask.Delay(TimeSpan.FromSeconds(_waitTime));
        Debug.Log("Hello universe");

        var text = await Work2(this.GetCancellationTokenOnDestroy());
        Debug.Log($"Text:{text}");
    }

    async UniTask<string> Work2(CancellationToken ct = default)
    {
        var req = UnityWebRequest.Get("https://www.google.com/");
        await req.SendWebRequest().ToUniTask(cancellationToken: ct);
        return req.downloadHandler.text;
    }

    public void OnDestroy()
    {
        _ct?.Cancel();
    }
}
