using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBlock : MonoBehaviour
{
    //[HideInInspector]
    private int xPos;
    //[HideInInspector]
    private int zPos;

    public int XPos { get => xPos; set => xPos = value; }
    public int ZPos { get => zPos; set => zPos = value; }

    private GameObject selectionBlockObj;

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
