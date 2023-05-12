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
    private GUIManager guiManager;

    //-------------------------------------------------------------------------

    private Charactor selectingChara;
    private List<MapBlock> reachableBlocks;
    private List<MapBlock> attackableBlocks;

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
        guiManager = GetComponent<GUIManager>();

        reachableBlocks = new List<MapBlock>();
        attackableBlocks = new List<MapBlock>();

        nowPhase = Phase.Myturn_Start;
    }

    //-------------------------------------------------------------------------

    bool isCalledOnce = false;

    // Update is called once per frame
    void Update()
    {
        if (nowPhase == Phase.Enemyturn_Start)
            nowPhase = Phase.Myturn_Start;

        if(!isCalledOnce)
        {
            if (Input.GetMouseButton(0))
            {
                isCalledOnce = true;

                if (guiManager.battleWindowUI.gameObject.activeInHierarchy)
                {
                    // バトル結果表示ウィンドウを閉じる
                    guiManager.battleWindowUI.HideWindow();

                    // 進行モードを進める(デバッグ用)
                    ChangePhase(Phase.Myturn_Start);
                    return;
                }

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
                targetBlock.SetSelectionMode(MapBlock.Highlight.Select);

                Charactor charaData = charactorManager.GetCharactor(targetBlock.XPos, targetBlock.ZPos);

                if (charaData)
                {
                    selectingChara = charaData;

                    guiManager.ShowStatusWindow(charaData);

                    reachableBlocks = mapManager.SearchReachableBlocks(charaData.XPos, charaData.ZPos);

                    foreach (MapBlock mapblock in reachableBlocks)
                        mapblock.SetSelectionMode(MapBlock.Highlight.Reachable);

                    ChangePhase(Phase.Myturn_Moving);

                    Debug.Log("Select Charactor :" + charaData.gameObject.name + " : position :" + charaData.XPos + " : " + charaData.ZPos);
                }

               else
                {
                    ClearSelectingChara();

                    Debug.Log("Tapped on Block  Position : " + targetBlock.transform.position);
                }

                break;

            case Phase.Myturn_Moving:

                if (reachableBlocks.Contains(targetBlock))
                {
                    selectingChara.MovePosition(targetBlock.XPos, targetBlock.ZPos);

                    reachableBlocks.Clear();

                    mapManager.AllSelectionModeClear();

                    guiManager.ShowCommandButtons();

                    ChangePhase(Phase.Myturn_Command);
                }
                    
                Debug.Log("phase Command");
                break;

            case Phase.Myturn_Command:

                if (attackableBlocks.Contains(targetBlock))
                {
                    attackableBlocks.Clear();

                    mapManager.AllSelectionModeClear();

                    var targetChara = charactorManager.GetCharactor(targetBlock.XPos, targetBlock.ZPos);

                    if(targetChara != null)
                    {
                        CharaAttack(selectingChara, targetChara);

                        ChangePhase(Phase.Myturn_Result);

                        Debug.Log("phase Result");
                        return;
                    }

                    else
                    {
                        ChangePhase(Phase.Enemyturn_Start);

                        Debug.Log("phase EnemyTurnStart");
                    }
                }
                break;
        }
        
    }

    private void ClearSelectingChara()
    {
        selectingChara = null;

        guiManager.HideStatusWindow();
    }

    private void ChangePhase(Phase NowPhase)
    {
        nowPhase = NowPhase;
    }

    public void AttackCommand()
    {
        guiManager.HideCommandButtons();

        attackableBlocks = mapManager.SearchAttackableBlocks(selectingChara.XPos, selectingChara.ZPos);

        foreach(MapBlock block in attackableBlocks)
        {
            block.SetSelectionMode(MapBlock.Highlight.Attackable);
        }
    }

    public void StandbyCommand()
    {
        guiManager.HideCommandButtons();

        ChangePhase(Phase.Enemyturn_Start);

        Debug.Log("phase EnemyTurnStart");
    }

    private void CharaAttack(Charactor attackchara, Charactor defensechara)
    {
        int damagevalue;
        int atkpoint = attackchara.atk;
        int defpoint = defensechara.def;

        damagevalue = atkpoint - defpoint;

        if (damagevalue < 0)
            damagevalue = 0;

        guiManager.battleWindowUI.ShowWindow(defensechara, damagevalue);

        defensechara.NowHp -= damagevalue;

        defensechara.NowHp = Mathf.Clamp(defensechara.NowHp, 0, defensechara.maxHP);

        if (defensechara.NowHp == 0)
            charactorManager.DeleteCharaData(defensechara);

        Debug.Log("Atk:" + attackchara.charaName + " Def:" + defensechara.charaName);
    }
}