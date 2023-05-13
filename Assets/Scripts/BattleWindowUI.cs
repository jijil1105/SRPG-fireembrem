using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleWindowUI : MonoBehaviour
{
    public Text nameText;
    public Image hpGageImage;
    public Text hpText;
    public Text damageText;

    // Start is called before the first frame update
    void Start()
    {
        HideWindow();
    }

    public void ShowWindow(Charactor charadata, int damagevalue)
    {
        gameObject.SetActive(true);

        nameText.text = charadata.charaName;

        int nowHp = charadata.NowHp - damagevalue;

        nowHp = Mathf.Clamp(nowHp, 0, charadata.maxHP);

        float amount = (float)charadata.NowHp / charadata.maxHP;
        float endamount = (float)nowHp / charadata.maxHP;

        DOTween.To(() => amount, (n) => amount = n, endamount, 1.0f).OnUpdate(() =>
        {
            hpGageImage.fillAmount = amount;
        });

        hpText.text = nowHp + "/" + charadata.maxHP;

        damageText.text = damagevalue + "Damage";
    }

    public void HideWindow()
    {
        gameObject.SetActive(false);
    }
}