using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private DataManager data;
    
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

    public void LevelUpChecker(Charactor charaData, Charactor enemyData)
    {
        
    }
}
