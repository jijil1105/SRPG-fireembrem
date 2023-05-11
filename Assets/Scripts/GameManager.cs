using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else
        {
            Destroy(gameObject);
        }
    }

    public static GameManager instance;

    //-------------------------------------------------------------------------

    private MapManager mapManager;
    private CharactorManager charactorManager;

    private Charactor selectingChara;
    private List<MapBlock> reachableBlocks;

    private enum Phase
    {
        Myturn_Start,
        Myturn_Moving,
        Myturn_Command,
        Myturn_Targeting,
        Myturn_Result,
        Enemyturn_Start,
        Enemyturn_Result
    }

    private Phase nowPhase;

    //------------------------------------------------------------------------


    private void Start()
    {
        mapManager = GetComponent<MapManager>();
        charactorManager = GetComponent<CharactorManager>();

        reachableBlocks = new List<MapBlock>();

        nowPhase = Phase.Myturn_Start;
    }

    //-------------------------------------------------------------------------

    bool isCalledOnce = false;

    // Update is called once per frame
    void Update()
    {
        if(!isCalledOnce)
        {
            if (Input.GetMouseButton(0))
            {
                isCalledOnce = true;
                GetMapBlockByTapPos();
            }
        }

        if(!Input.GetMouseButton(0)) { isCalledOnce = false; }
    }

    private void GetMapBlockByTapPos()
    {
        GameObject targetObject = null;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        if(Physics.Raycast(ray, out hit))
        {
            targetObject = hit.collider.gameObject;
        }

        if(targetObject!=null)
        {
            SelectBlock(targetObject.GetComponent<MapBlock>());
        }
    }

    private void SelectBlock(MapBlock targetBlock)
    {
        switch(nowPhase)
        {
            case Phase.Myturn_Start:

                mapManager.AllSelectionModeClear();
                targetBlock.SetSelectionMode(true);

                Charactor charaData = charactorManager.GetCharactor(targetBlock.XPos, targetBlock.ZPos);

                if (charaData)
                {
                    selectingChara = charaData;
                    reachableBlocks = mapManager.SearchReachableBlocks(charaData.XPos, charaData.ZPos);

                    ChangePhase(Phase.Myturn_Moving);

                    Debug.Log("Select Charactor :" + charaData.gameObject.name + " : position :" + charaData.XPos + " : " + charaData.ZPos);
                }

               else
                {
                    selectingChara = null;

                    Debug.Log("Tapped on Block  Position : " + targetBlock.transform.position);
                }
                break;

            case Phase.Myturn_Moving:

                if (reachableBlocks.Contains(targetBlock))
                {
                    selectingChara.MovePosition(targetBlock.XPos, targetBlock.ZPos);

                    reachableBlocks.Clear();

                    mapManager.AllSelectionModeClear();

                    ChangePhase(Phase.Myturn_Command);
                }
                    
                Debug.Log("phase Moving");
                break;
        }
        
    }

    private void ChangePhase(Phase NowPhase)
    {
        nowPhase = NowPhase;
    }
}