using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBlock : MonoBehaviour
{
    [HideInInspector]
    private int xPos;//ブロックのX座標
    [HideInInspector]
    private int zPos;//ブロックのY座標

    [Header("通行可能フラグ")]
    public bool passable;

    public int XPos { get => xPos; set => xPos = value; }
    public int ZPos { get => zPos; set => zPos = value; }

    private GameObject selectionBlockObj;//このブロックが選択された際に表示する強調表示ブロック


    // Start is called before the first frame update
    void Start()
    {
        selectionBlockObj = transform.GetChild(0).gameObject;

        SetSelectionMode(false);
    }

    public void SetSelectionMode(bool mode)
    {
        if(mode) { selectionBlockObj.SetActive(true); }

        else { selectionBlockObj.SetActive(false); }
    }
}
