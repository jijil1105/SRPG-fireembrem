using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// シーン遷移
    /// </summary>
    /// <param name="name">遷移するシーン名</param>
    public void ChangeScene(string name)
    {
        SceneManager.LoadScene(name);
    }

    /// <summary>
    /// ゲームを最初から始める
    /// </summary>
    public void NewGaeme()
    {
        //セーブデータ初期化
        DataManager._instance.DeleteData();

        //最初のマップに遷移
        SceneManager.LoadScene("Battle_1");
    }

    /// <summary>
    /// ゲームを続きから始める
    /// </summary>
    public void LoadGame()
    {
        //セーブデータ読み込み
        SaveData data = DataManager._instance.Load();

        //セーブデータからキャラステータス反映（仮）、セーブデータに保存されているマップへ遷移
        if (data != null && data.SceneName != "Delete Data")
        {
            Debug.Log(data.SceneName);

            for(int i = 0; i < data.atk.Count; i++)
            {
                Debug.Log(
                    data.name[i] + ":" +
                    data.maxHp[i] + ":" +
                    data.atk[i] + ":" +
                    data.def[i] + ":" +
                    data.atrr[i] + ":" +
                    data.movetype[i] + ":" +
                    data.skill[i]);
            }

            SceneManager.LoadScene(data.SceneName);
        }
        else
            Debug.Log("Dont have Data");
    }
}
