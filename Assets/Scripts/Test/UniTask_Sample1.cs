using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UniRx;

public class UniTask_Sample1 : MonoBehaviour
{
    private CancellationTokenSource _ct = new CancellationTokenSource();

    [SerializeField] private Button button;
    [SerializeField] private Text text;

    [SerializeField] BoolReactiveProperty isDead = new BoolReactiveProperty(false);

    private 

    // Start is called before the first frame update
    async void Start()
    {
        /*await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space),cancellationToken: this.GetCancellationTokenOnDestroy());
        Debug.Log("hogehoge");

        await UniTask.Delay(TimeSpan.FromSeconds(_waitTime), cancellationToken: this.GetCancellationTokenOnDestroy());
        Debug.Log("Hello universe");

        var text = await Work2(this.GetCancellationTokenOnDestroy());
        Debug.Log($"Text:{text}");

        work().Forget();

        await work3();
        Debug.Log("END");

        await MoveCoroutine();
        Debug.Log("MoveCoroutine");

        Debug.Log(Time.time);
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        Debug.Log(Time.time);

        Debug.Log(Time.frameCount);
        await UniTask.DelayFrame(5);
        Debug.Log(Time.frameCount);

        await UniTask.DelayFrame(5, PlayerLoopTiming.FixedUpdate);
        Debug.Log("End");*/

        /*DoAysnc().Forget();

        DoAsync2().Forget();

        await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

        isDead.Value = true;

        var result = await Observable.Return(1);
        Debug.Log(result);

        Debug.Log(Thread.CurrentThread.ManagedThreadId);

        await UniTask.SwitchToTaskPool();

        Debug.Log(Thread.CurrentThread.ManagedThreadId);

        await UniTask.Yield();

        Debug.Log(Thread.CurrentThread.ManagedThreadId);

        button.BindToOnClick(_ =>
        {
            return GetHtmlAsync().ToObservable().ForEachAsync(x => { text.text = x.Substring(0, 100); });
        });

        var request = UnityWebRequest.Get("https://unity3d.com");

        var result = await request.SendWebRequest().ToUniTask().Timeout(TimeSpan.FromSeconds(3));

        Debug.Log(result.downloadHandler.text);*/

        var task1 = GetHtmlAsync("https://github.com");
        var task2 = GetHtmlAsync("https://www.yahoo.co.jp");

        var (github, yahoo) = await UniTask.WhenAll(task1, task2);

        Debug.Log(github);
        Debug.Log(yahoo);
    }

    async UniTask<string> GetHtmlAsync(string uri)
    {
        var r = UnityWebRequest.Get(uri);
        var result = await r.SendWebRequest();
        return result.downloadHandler.text;
    }

    async UniTask DoAsync2()
    {
        await isDead;

        Debug.Log("Dead");
    }

    async UniTask DoAysnc()
    {
        var token = this.GetCancellationTokenOnDestroy();

        var asyncEventHandler = button.onClick.GetAsyncEventHandler(token);

        await asyncEventHandler.OnInvokeAsync();

        Debug.Log("1");

        await asyncEventHandler.OnInvokeAsync();

        Debug.Log("2");
    }

    async UniTask<string> Work2(CancellationToken ct = default)
    {
        var req = UnityWebRequest.Get("https://www.google.com/");
        await req.SendWebRequest().ToUniTask(cancellationToken: ct);
        return req.downloadHandler.text;
    }

    async UniTask work3()
    {
        await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.P), cancellationToken: this.GetCancellationTokenOnDestroy());
        Debug.Log("EEEEE");
    }

    async UniTask work()
    {
        await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space), cancellationToken: this.GetCancellationTokenOnDestroy());
        Debug.Log("DIDID");
    }

    public void OnDestroy()
    {
        _ct?.Cancel();
    }

    public IEnumerator UniTaskCompletionSourceTest()
    {
        var uts = new UniTaskCompletionSource();
        var isSuccess = false;

        UniTask.Void(async () =>
        {
            await uts.Task;
            isSuccess = true;
        });

        uts.TrySetResult();

        yield return null;

        Assert.AreEqual(isSuccess, true);
    }

    public IEnumerator UniTaskCompletionTest2()
    {
        var uts = new UniTaskCompletionSource<int>();

        UniTask.Void(async () =>
        {
            var result = await uts.Task;
            Assert.AreEqual(10, result);
        });

        uts.TrySetResult(10);

        yield return null;
        yield return null;
    }

    public IEnumerator UniTaskCompletionTest3()
    {
        var uts = new UniTaskCompletionSource();
        var isSuccess = false;

        UniTask.Void(async () =>
        {
            try
            {
                await uts.Task;
            }
            catch (OperationCanceledException)
            {
                isSuccess = true;
            }
        });

        yield return null;
        Assert.AreEqual(isSuccess, true);
    }

    public IEnumerator UniTaskCompletionTest4()
    {
        var uts = new UniTaskCompletionSource();
        var isSuccess = false;

        UniTask.Void(async () =>
        {
            try
            {
                await uts.Task;
            }
            catch (NotImplementedException)
            {
                isSuccess = true;
            }
        });

        uts.TrySetException(new NotImplementedException());

        yield return null;

        Assert.AreEqual(isSuccess, true);
    }

    public UniTask<string> WrapMyAsync()
    {
        var utcs = new UniTaskCompletionSource<string>();

        HogeAsycAction(result =>
        {
            utcs.TrySetResult(result);
        },
        ex =>
        {
            utcs.TrySetException(ex);
        });

        return utcs.Task;
    }

    private void HogeAsycAction(Action<string> onSuccess, Action<Exception> onError)
    {

    }

    IEnumerator MoveCoroutine()
    {
        var start = Time.time;
        while(Time.time - start < 2)
        {
            transform.position += Vector3.forward * Time.deltaTime;
            yield return null;
        }
    }

}

namespace Assets
{
    class ResourceLoadAsyncSample : MonoBehaviour
    {
        /// <summary>
        /// 初回アクセス時のみ非同期的に初期化する
        /// 2度目以降、処理が完了しているならキャッシュを利用する
        /// </summary>
        public UniTask<Texture> PlayerTexture { get; private set; }

        void Awake()
        {
            PlayerTexture = new UniTask<Texture>();
            PlayerTexture = LoadAsSprite();
            
        }

        async UniTask<Texture> LoadAsSprite()
        {
            var resource = await Resources.LoadAsync<Texture>("Player");
            return (resource as Texture);
        }
    }
}

/*public class Sample2 : MonoBehaviour
{
    async void Start()
    {
        var tex = await Resources.LoadAsync<Texture>("Player");

        gameObject.GetComponent<Renderer>().material.mainTexture = tex as Texture; 
    }
}*/

/*public class Sample3 : MonoBehaviour
{
    async void Start()
    {
        var url = "https://2.bp.blogspot.com/-tcLjNKJqOIQ/WkXHUuSC4qI/AAAAAAABJX0/ArQTS8DS9SEOJI4Mb5tvZg4GXuoED8iIQCLcBGAs/s800/otaku_winter.png";

        var tex = await DownLoadTexture(url);

        gameObject.GetComponent<Renderer>().material.mainTexture = tex;
    }

    async UniTask<Texture> DownLoadTexture(string url)
    {
        var r = UnityWebRequestTexture.GetTexture(url);

        await r.SendWebRequest();

        return DownloadHandlerTexture.GetContent(r);
    }
}*/

public class Sample4 : MonoBehaviour
{

}