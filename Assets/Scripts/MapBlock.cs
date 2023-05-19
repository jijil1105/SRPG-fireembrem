using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBlock : MonoBehaviour
{
    [HideInInspector]
    private int xPos;//ブロックのX座標
    [HideInInspector]
    private int zPos;//ブロックのY座標

    public int XPos { get => xPos; set => xPos = value; }
    public int ZPos { get => zPos; set => zPos = value; }

    //-------------------------------------------------------------------------

    [Header("通行可能フラグ")]
    public bool passable;//通行可能フラグ

    //-------------------------------------------------------------------------

    private GameObject selectionBlockObj;//このブロックが選択された際に表示する強調表示ブロック

    [Header("強調表示マテリアル：選択時")]
    public Material selMat_Select; // 選択時

    [Header("強調表示マテリアル：到達可能")]
    public Material selMat_Reachable; // キャラクターが到達可能

    [Header("強調表示マテリアル：攻撃可能")]
    public Material selMat_Attackable; // キャラクターが攻撃可能

    public enum Highlight
    {
        Off,//
        Select,//
        Reachable,//
        Attackable//
    }

    //-------------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        // 強調表示オブジェクトを取得
        selectionBlockObj = transform.GetChild(0).gameObject;

        // 初期状態では強調表示をしない
        SetSelectionMode(Highlight.Off);
    }

    //-------------------------------------------------------------------------

    /// <summary>
	/// 選択状態表示オブジェクトの表示・非表示を設定する
	/// </summary>
	/// <param name="mode">強調表示モード</param>
    public void SetSelectionMode(Highlight mode)
    {
        switch(mode)
        {
            // 強調表示なし
            case Highlight.Off:
                selectionBlockObj.SetActive(false);
                break;
            // 選択時強調表示：白色
            case Highlight.Select:
                selectionBlockObj.GetComponent<Renderer>().material = selMat_Select;
                selectionBlockObj.SetActive(true);
                break;
            // 移動可能ブロックの強調表示：青色
            case Highlight.Reachable:
                selectionBlockObj.GetComponent<Renderer>().material = selMat_Reachable;
                selectionBlockObj.SetActive(true);
                break;
            // 攻撃可能ブロックの強調表示：赤色
            case Highlight.Attackable:
                selectionBlockObj.GetComponent<Renderer>().material = selMat_Attackable;
                selectionBlockObj.SetActive(true);
                break;
        }
    }
}
