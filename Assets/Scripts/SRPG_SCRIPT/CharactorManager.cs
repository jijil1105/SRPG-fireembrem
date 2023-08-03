using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using Photon.Pun;

public class CharactorManager : MonoBehaviour
{
    public Transform charactorParent;// 全キャラクターオブジェクトの親オブジェクトTransform
    public List<Charactor> Charactors = new List<Charactor>();// 全キャラクターデータ

    [System.Serializable]
    public class CharaDatas
    {
        public GameObject charaObj;
        public string Character_Name;
        public string Character_ObjName;
    }
    [SerializeField]
    public CharaDatas[] charaObjs;

    public List<Vector3> initPos = new List<Vector3>();

    [SerializeField]
    private bool isMulti;

    //-------------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        switch(isMulti)
        {
            case true :

                if (DataManager._instance.GetSaveData().SceneName != "Delete Data")
                {
                    SaveData SaveData = DataManager._instance.GetSaveData();
                    if (SaveData != null)
                    {
                        int initX = -1;
                        int initZ = -4;
                        for (int i = 0; i < SaveData.name.Count(); i++)
                        {
                            var obj = GetCharaPhotonObj(SaveData.name[i]);

                            obj.GetComponent<Charactor>().charaName = SaveData.name[i];
                            obj.GetComponent<Charactor>().maxHP = SaveData.maxHp[i];
                            obj.GetComponent<Charactor>().atk = SaveData.atk[i];
                            obj.GetComponent<Charactor>().def = SaveData.def[i];
                            obj.GetComponent<Charactor>().Int = SaveData.Int[i];
                            obj.GetComponent<Charactor>().Res = SaveData.res[i];
                            obj.GetComponent<Charactor>().attribute = SaveData.atrr[i];
                            obj.GetComponent<Charactor>().moveType = SaveData.movetype[i];
                            obj.GetComponent<Charactor>().skill = SaveData.skill[i];
                            obj.GetComponent<Charactor>().isMagicAttac = SaveData.isMagicAttack[i];

                            obj.GetComponent<Charactor>().Lv = SaveData.Lv[i];
                            obj.GetComponent<Charactor>().nowExp = SaveData.nowExp[i];
                            obj.GetComponent<Charactor>().ExpPerLv = SaveData.ExpPerLv[i];
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

                            if (initPos.Count > 0)
                            {
                                obj.GetComponent<Charactor>().initPos_X = ((int)initPos[i].x);
                                obj.GetComponent<Charactor>().initPos_Z = ((int)initPos[i].z);
                            }
                            else
                            {
                                obj.GetComponent<Charactor>().initPos_X = initX;
                                obj.GetComponent<Charactor>().initPos_Z = initZ;
                                initX++;
                            }

                            DOVirtual.DelayedCall(1.0f, () =>
                            {
                                var Obj = PhotonNetwork
                                .Instantiate
                                (
                                    obj.name,
                                    new Vector3(obj.GetComponent<Charactor>().initPos_X, obj.GetComponent<Charactor>().initPos_Z),
                                    Quaternion.identity
                                );

                                Obj.transform.SetParent(charactorParent);
                            });

                            //obj.AddComponent<PhotonView>();

                            //var comp = obj.AddComponent<PhotonTransformView>();
                            //comp.m_SynchronizeScale = true;

                            //Instantiate(obj, charactorParent);
                        }
                    }
                }

                // マップ上の全キャラクターデータを取得
                // (charactersParent以下の全Characterコンポーネントを検索しリストに格納)
                charactorParent.GetComponentsInChildren(Charactors);

                Debug.Log("True");

                break;

            case false:

                if (DataManager._instance.GetSaveData().SceneName != "Delete Data")
                {
                    SaveData SaveData = DataManager._instance.GetSaveData();
                    if (SaveData != null)
                    {
                        int initX = -1;
                        int initZ = -4;
                        for (int i = 0; i < SaveData.name.Count(); i++)
                        {
                            var obj = GetCharaObj(SaveData.name[i]);

                            obj.GetComponent<Charactor>().name = SaveData.name[i];
                            obj.GetComponent<Charactor>().maxHP = SaveData.maxHp[i];
                            obj.GetComponent<Charactor>().atk = SaveData.atk[i];
                            obj.GetComponent<Charactor>().def = SaveData.def[i];
                            obj.GetComponent<Charactor>().Int = SaveData.Int[i];
                            obj.GetComponent<Charactor>().Res = SaveData.res[i];
                            obj.GetComponent<Charactor>().attribute = SaveData.atrr[i];
                            obj.GetComponent<Charactor>().moveType = SaveData.movetype[i];
                            obj.GetComponent<Charactor>().skill = SaveData.skill[i];
                            obj.GetComponent<Charactor>().isMagicAttac = SaveData.isMagicAttack[i];

                            obj.GetComponent<Charactor>().Lv = SaveData.Lv[i];
                            obj.GetComponent<Charactor>().nowExp = SaveData.nowExp[i];
                            obj.GetComponent<Charactor>().ExpPerLv = SaveData.ExpPerLv[i];
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

                            if (initPos.Count > 0)
                            {
                                obj.GetComponent<Charactor>().initPos_X = ((int)initPos[i].x);
                                obj.GetComponent<Charactor>().initPos_Z = ((int)initPos[i].z);
                            }
                            else
                            {
                                obj.GetComponent<Charactor>().initPos_X = initX;
                                obj.GetComponent<Charactor>().initPos_Z = initZ;
                                initX++;
                            }

                            Instantiate(obj, charactorParent);
                        }
                    }
                }

                // マップ上の全キャラクターデータを取得
                // (charactersParent以下の全Characterコンポーネントを検索しリストに格納)
                charactorParent.GetComponentsInChildren(Charactors);

                Debug.Log("false");

                break;
        }
    }

    //-------------------------------------------------------------------------

    /// <summary>
	/// 指定した位置に存在するキャラクターデータを検索して返す
	/// </summary>
	/// <param name="X">X位置</param>
	/// <param name="Z">Z位置</param>
	/// <returns>対象のキャラクターデータ</returns>
    public Charactor GetCharactor(int X, int Z)
    {
        return Charactors.FirstOrDefault(cha => cha.XPos == X && cha.ZPos == Z);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="chara_name"></param>
    /// <returns></returns>
    public GameObject GetCharaObj(string chara_name)
    {
        var obj = charaObjs.FirstOrDefault(chara => chara.Character_Name == chara_name);
        return obj.charaObj;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="chara_name"></param>
    /// <returns></returns>
    public GameObject GetCharaPhotonObj(string chara_name)
    {
        var obj = charaObjs.FirstOrDefault(chara => chara.Character_Name == chara_name);
        var photon_obj = (GameObject)Resources.Load(obj.Character_ObjName + "_Photon");
        return photon_obj;
    }
    /// <summary>
	/// 指定したキャラクターを削除する
	/// </summary>
	/// <param name="charadata">対象キャラデータ</param>
    public void DeleteCharaData(Charactor charadata)
    {
        Charactors.Remove(charadata);

        DataManager._instance.DeleteCharaData(charadata);
        
        // オブジェクト削除を攻撃完了後に処理させる為に遅延実行
        DOVirtual.DelayedCall(0.5f, () => { Destroy(charadata.gameObject); });

        var gm = GetComponent<GameManager>();

        var gm_muti = GetComponent<GameManager_Multi>();

        if(gm)
            gm.CheckGameSet();

        if (gm_muti)
            gm_muti.CheckGameSet();
    }

    public void SetCharaList()
    {
        // マップ上の全キャラクターデータを取得
        // (charactersParent以下の全Characterコンポーネントを検索しリストに格納)
        charactorParent.GetComponentsInChildren(Charactors);
    }
}
