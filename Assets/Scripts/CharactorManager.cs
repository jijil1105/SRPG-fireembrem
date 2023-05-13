using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

public class CharactorManager : MonoBehaviour
{
    public Transform charactorParent;
    public List<Charactor> Charactors = new List<Charactor>();

    //-------------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        charactorParent.GetComponentsInChildren(Charactors);
    }

    //-------------------------------------------------------------------------

    public Charactor GetCharactor(int X, int Z)
    {
        return Charactors.FirstOrDefault(cha => cha.XPos == X && cha.ZPos == Z);
    }

    public void DeleteCharaData(Charactor charadata)
    {
        Charactors.Remove(charadata);

        DOVirtual.DelayedCall(0.5f, () => { Destroy(charadata.gameObject); });
    }
}
