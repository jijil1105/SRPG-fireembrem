using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public Transform blockParent;
    public GameObject ChildeObject;
    public GameObject blockPrefab_Grass;
    public GameObject blockPrefab_Water;

    public const int MAP_WIDTH = 9;
    public const int MAP_HEIGHT = 9;
    private const int GENERATE_RATIO_GRASS = 90;

    void Start()
    {

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
                GameObject childeobj;

                if (isGrass) { obj = Instantiate(blockPrefab_Grass, blockParent); }

                else { obj = Instantiate(blockPrefab_Water, blockParent); }

                obj.transform.position = pos;
                if(obj.GetComponent<MapBlock>())
                {
                    Debug.Log("showflg = true");
                }
                
                childeobj = Instantiate(ChildeObject, obj.transform);
                childeobj.SetActive(false);

                if(childeobj.GetComponent<MapBlock>())
                {
                    Debug.Log("showflg = false");
                }

                childeobj.GetComponent<MapBlock>().XPos = (int)pos.x;
                childeobj.GetComponent<MapBlock>().ZPos = (int)pos.z;

                obj.GetComponent<MapBlock>().XPos = (int)pos.x;
                obj.GetComponent<MapBlock>().ZPos = (int)pos.z;

                GameManager.instance.childobjs.Add(childeobj);
                GameManager.instance.blocks.Add(obj);
            }
        }
    }

    void Update()
    {
        
    }
}
