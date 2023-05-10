using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    public List<GameObject> blocks = new List<GameObject>();
    public List<GameObject> childobjs = new List<GameObject>();

    public int nowmode;//０：選択なし、１：キャラクター選択、２：キャラクター移動選択
    public bool nowPhase; // true:自ターン, false:相手ターン

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            GetMapBlockByTapPos();
        }
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
        GameObject obj = childobjs.FirstOrDefault(obj => obj.activeSelf == true);
        if(obj)
        {
            obj.SetActive(false);
        }
        
        childobjs.FirstOrDefault(obj => obj.transform.position == targetBlock.transform.position).SetActive(true);
        Debug.Log("Tapped on Block  Position : " + targetBlock.transform.position);

        Charactor charactor = CharactorManager.instance.GetCharactor(targetBlock.XPos, targetBlock.ZPos);
        if(charactor)
        {
            Debug.Log("Select Charactor");
        }
    }
}