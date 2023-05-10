using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CharactorManager : MonoBehaviour
{
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else
        {
            Destroy(gameObject);
        }
    }

    public static CharactorManager instance;
    public List<Charactor> Charactors = new List<Charactor>();

    // Start is called before the first frame update
    void Start()
    {
        GetComponentsInChildren(Charactors);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Charactor GetCharactor(int X, int Z)
    {
        return Charactors.FirstOrDefault(cha => cha.XPos == X && cha.ZPos == Z);
    }
}
