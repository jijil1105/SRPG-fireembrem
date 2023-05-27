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

	string filePath;
	SaveData saveData;

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

		filePath = Application.persistentDataPath + "/" + ".savedata.json";
		saveData = new SaveData();
		//Debug.Log(Save_HP + ":" + Save_Atk + ":" + Save_Def);
	}

	public void WriteSaveData(List<Charactor> charactors)
    {
		Debug.Log("Save Data");

		saveData.SceneName = "Battle_1";

		foreach(var chara in charactors)
        {
			saveData.name.Add(chara.charaName);
			saveData.maxHp.Add(chara.maxHP);
			saveData.atk.Add(chara.atk);
			saveData.def.Add(chara.def);
			saveData.atrr.Add(chara.attribute.ToString());
			saveData.movetype.Add(chara.moveType.ToString());
			saveData.skill.Add(chara.skill.ToString());
        }

		string json = LitJson.JsonMapper.ToJson(saveData);

		StreamWriter streamWriter = new StreamWriter(filePath);

		streamWriter.Write(json); streamWriter.Flush();
		streamWriter.Close();
    }

    public SaveData Load()
    {
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
}

[SerializeField]
public class SaveData
{
	public string SceneName;

	public List<string> name = new List<string>();

	public List<int> maxHp = new List<int>();

	public List<int> atk = new List<int>();

	public List<int> def = new List<int>();

	public List<string> atrr = new List<string>();

	public List<string> movetype = new List<string>();

	public List<string> skill = new List<string>();
};