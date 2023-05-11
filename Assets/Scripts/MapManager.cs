using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public Transform blockParent;
    public GameObject blockPrefab_Grass;
    public GameObject blockPrefab_Water;

    public MapBlock[,] mapBlocks;

    public const int MAP_WIDTH = 9;
    public const int MAP_HEIGHT = 9;
    private const int GENERATE_RATIO_GRASS = 90;

    void Start()
    {
        mapBlocks = new MapBlock[MAP_WIDTH, MAP_HEIGHT];

        Vector3 defaultPos = new Vector3(0.0f, 0.0f, 0.0f);
        defaultPos.x = -(MAP_WIDTH / 2);
        defaultPos.z = -(MAP_HEIGHT / 2);

        for(int i = 0; i < MAP_WIDTH; i++)
        {
            for (int j = 0; j < MAP_HEIGHT; j++)
            {
                Vector3 pos = defaultPos;
                pos.x += i;
                pos.z += j;

                int rand = Random.Range(0, 100);
                bool isGrass = false;

                if (rand < GENERATE_RATIO_GRASS) { isGrass = true; }

                GameObject obj;

                if (isGrass) { obj = Instantiate(blockPrefab_Grass, blockParent); }

                else { obj = Instantiate(blockPrefab_Water, blockParent); }

                obj.transform.position = pos;

                var mapBlock = obj.GetComponent<MapBlock>();
                mapBlocks[i, j] = mapBlock;

                mapBlock.XPos = (int)pos.x;
                mapBlock.ZPos = (int)pos.z;
            }
        }
    }

    public void AllSelectionModeClear()
    {
        for (int i = 0; i < MAP_WIDTH; i++)
            for (int j = 0; j < MAP_HEIGHT; j++)
                mapBlocks[i, j].SetSelectionMode(false);
    }

    public List<MapBlock> SearchReachableBlocks(int xPos, int zPos)
    {
        var results = new List<MapBlock>();

        int baseX = -1, baseZ = -1;

        for(int i=0; i<MAP_WIDTH; i++)
        {
            for(int j=0; j<MAP_HEIGHT; j++)
            {
                if((mapBlocks[i,j].XPos == xPos)&&(mapBlocks[i,j].ZPos == zPos))
                {
                    baseX = i;
                    baseZ = j;
                    break;
                }
            }
            if (baseX != -1) { break; }
        }

        for (int i = baseX + 1; i < MAP_WIDTH; i++)
            if (AddReachableList(results, mapBlocks[i, baseZ]))
                break;

        for (int i = baseX - 1; i >= 0; i--)
            if (AddReachableList(results, mapBlocks[i, baseZ]))
                break;

        for (int i = baseZ + 1; i < MAP_HEIGHT; i++)
            if (AddReachableList(results, mapBlocks[baseX, i]))
                break;

        for (int i = baseZ - 1; i >= 0; i--)
            if (AddReachableList(results, mapBlocks[baseX, i]))
                break;

        results.Add(mapBlocks[baseX, baseZ]);

        return results;
    }

    private bool AddReachableList(List<MapBlock> reachableList, MapBlock targetBlock)
    {
        if (!targetBlock.passable)
            return true;

        var charaData = GetComponent<CharactorManager>().GetCharactor(targetBlock.XPos, targetBlock.ZPos);
        if (charaData != null)
            return false;

        reachableList.Add(targetBlock);
        return false;
    }
}
