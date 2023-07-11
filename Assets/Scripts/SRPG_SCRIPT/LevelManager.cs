using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private DataManager data;
    private int k = 3;//Lv1~Lv2にレベルアップさせるのに必要な倒す敵の数
    
    // Start is called before the first frame update
    void Start()
    {
        data = DataManager._instance;

        if (!data)
            Debug.Log("data == null");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// キャラクターのレベルアップ処理
    /// </summary>
    /// <param name="charaData">レベルアップさせるキャラクター</param>
    public List<int> LevelUp(Charactor charaData)
    {
        //現在の経験値をレベルアップに必要な経験値分引いて差を求める
        var nowExp = charaData.nowExp - charaData.ExpPerLv;
        //キャラクターのレベル値を＋１
        charaData.Lv += 1;
        //次のレベルアップに必要な経験値を取得
        charaData.ExpPerLv = (int)GetExp_Per_Level(100f, 1.5f, charaData.Lv);
        //現在の経験値を求めた差分に更新
        charaData.nowExp = nowExp;
        //キャラのステータス強化
        return LevelUpStatus(charaData);
    }

    /// <summary>
    /// レベルアップ時のキャラのステータス強化処理
    /// </summary>
    /// <param name="charaData">強化するキャラデータ</param>
    public List<int> LevelUpStatus(Charactor charaData)
    {
        /*
        public int maxHP;//最大Hp
        public int atk;//物理攻撃力
        public int def;//物理防御力
        public int Int;//魔法攻撃力
        public int Res;//魔法防御力
        */

        List<int> Up_Status = new List<int>();
        for (int i = 0; i < 5; i++)
            Up_Status.Add(0);

        switch (charaData.moveType)//行動タイプによって成長率変更（仮）
        {
            default:

                int randHP = Random.Range(0, 100);
                int randAtk = Random.Range(0, 100);
                int randDef = Random.Range(0, 100);
                int randInt = Random.Range(0, 100);
                int randRes = Random.Range(0, 100);

                if (randHP >= 33)
                {
                    charaData.maxHP += 1;
                    Up_Status[0] = 1;
                    Debug.Log("レベルアップ : " + charaData.charaName + " HP : " + charaData.maxHP);
                }
                    

                if (randAtk >= 33)
                {
                    charaData.atk += 1;
                    Up_Status[1] = 1;
                    Debug.Log("レベルアップ : " + charaData.charaName + " atk : " + charaData.atk);
                }

                if (randDef >= 33)
                {
                    charaData.def += 1;
                    Up_Status[2] = 1;
                    Debug.Log("レベルアップ : " + charaData.charaName + " def : " + charaData.def);
                }

                if(randInt >= 33)
                {
                    charaData.Int += 1;
                    Up_Status[3] = 1;
                    Debug.Log("レベルアップ : " + charaData.charaName + " Int : " + charaData.Int);
                }

                if(randRes >= 33)
                {
                    charaData.Res += 1;
                    Up_Status[4] = 1;
                    Debug.Log("レベルアップ : " + charaData.charaName + " Res : " + charaData.Res);
                }
                    
                break;
        }

        return Up_Status;
    }

    /// <summary>
    /// 倒したキャラのレベルによって取得経験値を計算する（等比数列を使用して計算しています。） 
    /// </summary>
    /// <param name="a">初項：Lv1~Lv2レベルアップで必要な数（100で固定）</param>
    /// <param name="r">公比：倒した敵のレベル分掛ける乗数</param>
    /// <param name="n">項数：レベルの値</param>
    /// <returns>取得経験値</returns>
    public float GetExp(float a, float r, float n)
    {
        var num = (a * Mathf.Pow(r, n - 1)) / k;
        return num;
    }

    /// <summary>
    /// 次のレベルアップに必要な経験値量を計算
    /// </summary>
    /// <param name="a"></param>
    /// <param name="r"></param>
    /// <param name="n"></param>
    /// <returns></returns>
    public float GetExp_Per_Level(float a, float r, float n)
    {
        return (a * Mathf.Pow(r, n - 1));
    }
}
