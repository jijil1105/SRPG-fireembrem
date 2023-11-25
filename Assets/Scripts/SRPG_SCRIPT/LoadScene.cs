using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;
using DG.Tweening;

public class LoadScene : MonoBehaviour
{
    [SerializeField] AudioManager audioManager;
    [SerializeField] Image fadeimage;
    [SerializeField] Button ContinueButton;

    // Start is called before the first frame update
    void Start()
    {
        var data = DataManager._instance.Load();

        if (data != null && data.SceneName != "Delete Data")
        {
            ContinueButton.gameObject.SetActive(true);
        }
        else
        {
            ContinueButton.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// シーン遷移
    /// </summary>
    /// <param name="name">遷移するシーン名</param>
    public async void ChangeScene(string name)
    {

        await UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken: this.GetCancellationTokenOnDestroy());
        SceneManager.LoadScene(name);
    }

    /// <summary>
    /// ゲームを最初から始める
    /// </summary>
    public async void NewGaeme()
    {
        //ニューゲームボタンのクリックSE再生
        AudioManager.instance.Play("SE_1");
        
        //セーブデータ初期化
        DataManager._instance.DeleteData();

        FadeOut(2);

        await UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken: this.GetCancellationTokenOnDestroy());

        //最初のマップに遷移
        SceneManager.LoadScene("Battle_1");
    }

    /// <summary>
    /// ゲームを続きから始める
    /// </summary>
    public async void LoadGame()
    {
        //ニューゲームボタンのクリックSE再生
        AudioManager.instance.Play("SE_1");

        //セーブデータ読み込み
        SaveData data = DataManager._instance.Load();

        //セーブデータからキャラステータス反映（仮）、セーブデータに保存されているマップへ遷移
        if (data != null && data.SceneName != "Delete Data")
        {
            for(int i = 0; i < data.atk.Count; i++)
            {
                Debug.Log(
                    data.name[i] + ":" +
                    data.maxHp[i] + ":" +
                    data.atk[i] + ":" +
                    data.def[i] + ":" +
                    data.Int[i] + ":" +
                    data.res[i] + ":" +
                    data.atrr[i] + ":" +
                    data.movetype[i] + ":" +
                    data.skill[i] + ":" +
                    data.isMagicAttack[i] + ":" +
                    data.Lv[i] + ":" +
                    data.nowExp[i]
                    );                  
            }
            /*
		    public string charaName;//キャラ名
		    public int maxHP;//最大Hp
		    public int atk;//物理攻撃力
		    public int def;//物理防御力
		    public int Int;//魔法攻撃力
		    public int Res;//魔法防御力
		    public Attribute attribute;// 属性
		    public MoveType moveType;//移動タイプ
		    public SkillDefine.Skill skill;//スキル
		    public bool isMagicAttac;//魔法攻撃flg

		    public int Lv;//レベル
		    public int nowExp;//現在の経験値
		    public int ExpPerLv;//次のレベルに必要な経験値
            */

            FadeOut(2);

            await UniTask.Delay(TimeSpan.FromSeconds(3),cancellationToken: this.GetCancellationTokenOnDestroy());
            SceneManager.LoadScene(data.SceneName);
        }
        else
            Debug.Log("Dont have Data");
    }

    private void FadeIn(float duration, Action on_completed = null)
    {
        StartCoroutine(FadeCoroutine(duration, on_completed, true));
    }

    private void FadeOut(float duration, Action on_completed = null)
    {
        StartCoroutine(FadeCoroutine(duration, on_completed));
    }

    private IEnumerator FadeCoroutine(float duration, Action on_compleated, bool is_reversing = false)
    {
        if (!is_reversing) fadeimage.enabled = true;

        var elapsed_time = 0.0f;
        var color = fadeimage.color;

        while(elapsed_time < duration)
        {
            var elapased_rate = Mathf.Min(elapsed_time / duration, 1.0f);
            color.a = is_reversing ? 1.0f - elapased_rate : elapased_rate;
            fadeimage.color = color;

            yield return null;
            elapsed_time += Time.deltaTime;
        }
    }
}
