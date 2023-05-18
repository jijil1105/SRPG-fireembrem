using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GUIManager : MonoBehaviour
{
    public GameObject statusWindow;
    public Text nameText;
    public Text hpText;
    public Text atkText;
    public Text defText;

    public Image attributeIcon;
    public Image hpGageImage;

    public Sprite attr_Water;
    public Sprite attr_Fire;
    public Sprite attr_Wind;
    public Sprite attr_Soil;

    //-------------------------------------------------------------------------

    public BattleWindowUI battleWindowUI;

    //-------------------------------------------------------------------------

    public GameObject commandButtons;

    //-------------------------------------------------------------------------

    public Image playerTurnImage;
    public Image enemyTurnImage;

    //-------------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        HideStatusWindow();
        HideCommandButtons();
    }

    //-------------------------------------------------------------------------

    public void ShowStatusWindow(Charactor charaData)
    {
        statusWindow.SetActive(true);

        nameText.text = charaData.charaName;

        switch(charaData.attribute)
        {
            case Charactor.Attribute.Water:
                attributeIcon.sprite = attr_Water;
                break;
            case Charactor.Attribute.Fire:
                attributeIcon.sprite = attr_Fire;
                break;
            case Charactor.Attribute.Wind:
                attributeIcon.sprite = attr_Wind;
                break;
            case Charactor.Attribute.Soil:
                attributeIcon.sprite = attr_Soil;
                break;
        }

        float ratio = (float)charaData.NowHp / charaData.maxHP;
        hpGageImage.fillAmount = ratio;

        hpText.text = charaData.NowHp + "/" + charaData.maxHP;

        atkText.text = charaData.atk.ToString();

        defText.text = charaData.def.ToString();
    }

    //-------------------------------------------------------------------------

    public void HideStatusWindow()
    {
        statusWindow.SetActive(false);
    }

    public void ShowCommandButtons()
    {
        commandButtons.SetActive(true);
    }

    public void HideCommandButtons()
    {
        commandButtons.SetActive(false);
    }

    public void ShowLogoChangeTurn(bool playerTurn)
    {
        if(playerTurn)
        {
            playerTurnImage.DOFade(1.0f, 1.0f).SetEase(Ease.OutCubic).SetLoops(2, LoopType.Yoyo);
        }
        else
        {
            enemyTurnImage.DOFade(1.0f, 1.0f).SetEase(Ease.OutCubic).SetLoops(2, LoopType.Yoyo);
        }
    }
}
