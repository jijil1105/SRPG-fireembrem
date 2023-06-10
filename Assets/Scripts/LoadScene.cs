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
        //SceneManager.LoadScene("Battle_1");
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

            SceneManager.LoadScene(data.SceneName);
        }
        else
            Debug.Log("Dont have Data");
    }
}
