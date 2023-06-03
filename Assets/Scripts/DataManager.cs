using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using LitJson;

public class DataManager : MonoBehaviour
{
	// シングルトン管理用変数
	// (static設定によって全てのインスタンスでこの値を共有する)
	[HideInInspector]
	public static DataManager _instance;

	string filePath;//セーブデータを書き込むパス
	private SaveData saveData;//セーブするデータ

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

		//パス設定
		filePath = Application.persistentDataPath + "/" + ".savedata.json";
		//セーブデータ初期化
		saveData = new SaveData();
		//Debug.Log(Save_HP + ":" + Save_Atk + ":" + Save_Def);
	}

	public SaveData GetSaveData()
    {
		return saveData;
    }

	/// <summary>
    /// セーブするデータをJsonFileに書き込み
    /// </summary>
    /// <param name="charactors">セーブするキャラデータ</param>
	public void WriteSaveData(List<Charactor> charactors, string clear_scene_name)
    {
		Debug.Log("Save Data");

		//クリアしたシーンに応じて次にロードするシーン名を保存
		Save_ClearMap(clear_scene_name);

		//キャラデータをセーブデータ型のクラスに格納
		foreach (var chara in charactors)
        {
			saveData.name.Add(chara.charaName);
			saveData.maxHp.Add(chara.maxHP);
			saveData.atk.Add(chara.atk);
			saveData.def.Add(chara.def);
			saveData.Int.Add(chara.Int);
			saveData.res.Add(chara.Res);
			saveData.atrr.Add(chara.attribute);
			saveData.movetype.Add(chara.moveType);
			saveData.skill.Add(chara.skill);
			saveData.isMagicAttack.Add(chara.isMagicAttac);
			saveData.Lv.Add(chara.Lv);
			saveData.nowExp.Add(chara.nowExp);
			saveData.ExpPerLv.Add(chara.ExpPerLv);
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
		//JsonFileに書き込み
		string json = LitJson.JsonMapper.ToJson(saveData);

		StreamWriter streamWriter = new StreamWriter(filePath);

		streamWriter.Write(json); streamWriter.Flush();
		streamWriter.Close();
    }

	/// <summary>
    /// JsonFileに書き込んだセーブデータを読み込む
    /// </summary>
    /// <returns>読み込んだセーブデータ</returns>
    public SaveData Load()
    {
		//JsonFileを読み込み
		if (File.Exists(filePath))
		{
			StreamReader streamReader;

			streamReader = new StreamReader(filePath);

			string data = streamReader.ReadToEnd();

			streamReader.Close();

			saveData = LitJson.JsonMapper.ToObject<SaveData>(data);

			return saveData;
		}
		else
			return null;
    }

	/// <summary>
    /// 初期化データを既存データに上書き
    /// </summary>
	public void DeleteData()
    {
		Debug.Log("Delete Data");

		saveData = new SaveData();
		saveData.SceneName = "Delete Data";

		string json = LitJson.JsonMapper.ToJson(saveData);

		StreamWriter streamWriter = new StreamWriter(filePath);

		streamWriter.Write(json); streamWriter.Flush();
		streamWriter.Close();
	}

    public void Save_ClearMap (string name)
    {
		if(name == "Battle_1")
        {
			saveData.SceneName = "Battle_2";
        }
		else
        {
			saveData.SceneName = "Delete Data";

		}
    }
}

/// <summary>
/// セーブデータクラス
/// </summary>
[SerializeField]
public class SaveData
{
	public string SceneName;

	public List<string> name = new List<string>();

	public List<int> maxHp = new List<int>();

	public List<int> atk = new List<int>();

	public List<int> def = new List<int>();

	public List<int> Int = new List<int>();

	public List<int> res = new List<int>();

	public List<Charactor.Attribute> atrr = new List<Charactor.Attribute>();

	public List<Charactor.MoveType> movetype = new List<Charactor.MoveType>();

	public List<SkillDefine.Skill> skill = new List<SkillDefine.Skill>();

	public List<bool> isMagicAttack = new List<bool>();

	public List<int> Lv = new List<int>();

	public List<int> nowExp = new List<int>();

	public List<int> ExpPerLv = new List<int>();

	public List<Sprite> chara_sprite = new List<Sprite>();
};

/*
[Header("Charactor's Name")]
public string charaName;//キャラ名
[Header("maxHP")]
public int maxHP;//最大Hp
[Header("atk")]
public int atk;//物理攻撃力
[Header("def")]
public int def;//物理防御力
[Header("magic atk")]
public int Int;//魔法攻撃力
[Header("magic def")]
public int Res;//魔法防御力
[Header("Attribute")]
public Attribute attribute;// 属性
[Header("移動方法")]
public MoveType moveType;//移動タイプ
[Header("Skill")]
public SkillDefine.Skill skill;//スキル
[Header("魔法攻撃フラグ")]
public bool isMagicAttac;//魔法攻撃力

public int Lv;//レベル
public int nowExp;//現在の経験値
public int ExpPerLv;//次のレベルに必要な経験値
*/