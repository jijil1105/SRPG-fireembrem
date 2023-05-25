using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
	// シングルトン管理用変数
	// (static設定によって全てのインスタンスでこの値を共有する)
	[HideInInspector]
	public static DataManager _instance;

	// プレイヤー強化データ
	public int Save_HP; // 最大HP上昇量
	public int Save_Atk; // 攻撃力上昇量
	public int Save_Def; // 防御力上昇量

	// データのキー定義
	public const string Key_HP = "Key_HP";
	public const string Key_Atk = "Key_Atk";
	public const string Key_Def = "Key_Def";

	private void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
			DontDestroyOnLoad(gameObject);
		}
        else
        {
			Destroy(gameObject);
        }

		Save_HP = PlayerPrefs.GetInt(Key_HP);
		Save_Atk = PlayerPrefs.GetInt(Key_Atk);
		Save_Def = PlayerPrefs.GetInt(Key_Def);

		Debug.Log(Save_HP + ":" + Save_Atk + ":" + Save_Def);
	}

	public void WriteSaveData(Charactor charaData)
    {
		Save_HP = charaData.maxHP;
		Save_Atk = charaData.atk;
		Save_Def = charaData.def;

		PlayerPrefs.SetInt(Key_HP, Save_HP);
		PlayerPrefs.SetInt(Key_Atk, Save_Atk);
		PlayerPrefs.SetInt(Key_Def, Save_Def);
		PlayerPrefs.Save();
    }
}
