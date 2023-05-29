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

    public void LevelUp(Charactor charaData)
    {
        var nowExp = charaData.nowExp - charaData.ExpPerLv;
        charaData.Lv += 1;
        charaData.ExpPerLv = (int)GetExp_Per_Level(100f, 1.5f, charaData.Lv);
        charaData.nowExp = nowExp;
        LevelUpStatus(charaData);
    }

    public void LevelUpStatus(Charactor charaData)
    {
        switch(charaData.moveType)
        {
            default:
                int randHP = Random.Range(0, 100);
                int randAtk = Random.Range(0, 100);
                int randDef = Random.Range(0, 100);

                if (randHP >= 33)
                    charaData.maxHP += 1;

                else if (randAtk >= 33)
                    charaData.atk += 1;

                else if (randDef >= 33)
                    charaData.def += 1;

                break;
        }
    }

    /// <summary>
    /// 経験値計算 
    /// </summary>
    /// <param name="a">初項：Lv1~Lv2レベルアップで必要な数（100で固定）</param>
    /// <param name="r">公比：倒した敵のレベル分掛ける乗数</param>
    /// <param name="n">項数：レベルの値</param>
    /// <returns></returns>
    public float GetExp(float a, float r, float n)
    {
        return (a * Mathf.Pow(r, n - 1)) / k;
    }

    public float GetExp_Per_Level(float a, float r, float n)
    {
        return (a * Mathf.Pow(r, n - 1));
    }
}
