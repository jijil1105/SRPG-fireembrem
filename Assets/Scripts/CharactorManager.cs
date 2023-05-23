using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

public class CharactorManager : MonoBehaviour
{
    public Transform charactorParent;// 全キャラクターオブジェクトの親オブジェクトTransform
    public List<Charactor> Charactors = new List<Charactor>();// 全キャラクターデータ

    //-------------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        // マップ上の全キャラクターデータを取得
        // (charactersParent以下の全Characterコンポーネントを検索しリストに格納)
        charactorParent.GetComponentsInChildren(Charactors);
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
	/// 指定したキャラクターを削除する
	/// </summary>
	/// <param name="charadata">対象キャラデータ</param>
    public void DeleteCharaData(Charactor charadata)
    {
        Charactors.Remove(charadata);

        // オブジェクト削除を攻撃完了後に処理させる為に遅延実行
        DOVirtual.DelayedCall(0.5f, () => { Destroy(charadata.gameObject); });

        GetComponent<GameManager>().CheckGameSet();
    }
}
