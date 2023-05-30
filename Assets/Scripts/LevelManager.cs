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
    public void LevelUp(Charactor charaData)
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
        LevelUpStatus(charaData);
    }

    /// <summary>
    /// レベルアップ時のキャラのステータス強化処理
    /// </summary>
    /// <param name="charaData">強化するキャラデータ</param>
    public void LevelUpStatus(Charactor charaData)
    {
        switch(charaData.moveType)//行動タイプによって成長率変更（仮）
        {
            default:
                int randHP = Random.Range(0, 100);
                int randAtk = Random.Range(0, 100);
                int randDef = Random.Range(0, 100);

                if (randHP >= 33)
                {
                    charaData.maxHP += 1;
                    Debug.Log("レベルアップ：" + charaData.charaName + charaData.maxHP);
                }
                    

                else if (randAtk >= 33)
                {
                    charaData.atk += 1;
                    Debug.Log("レベルアップ：" + charaData.charaName + charaData.atk);
                }

                else if (randDef >= 33)
                {
                    charaData.def += 1;
                    Debug.Log("レベルアップ：" + charaData.charaName + charaData.def);
                }
                    
                break;
        }
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
