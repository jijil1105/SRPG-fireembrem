using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GUIManager : MonoBehaviour
{
    // ステータスウィンドウUI
    public GameObject statusWindow; // ステータスウィンドウオブジェクト
    public Text nameText; // 名前Text
    public Image attributeIcon; // 属性アイコンImage
    public Image hpGageImage; // HPゲージImage
    public Text hpText; // HPText
    public Text atkText; // 攻撃力Text
    public Text defText; // 防御力Text
                         // 属性アイコン画像
    public Sprite attr_Water; // 水属性アイコン画像
    public Sprite attr_Fire;  // 火属性アイコン画像
    public Sprite attr_Wind;  // 風属性アイコン画像
    public Sprite attr_Soil;  // 土属性アイコン画像

    //-------------------------------------------------------------------------

    // バトル結果表示UI処理クラス
    public BattleWindowUI battleWindowUI;

    //-------------------------------------------------------------------------

    // キャラクターのコマンドボタン
    public GameObject commandButtons;

    //-------------------------------------------------------------------------

    // 各種ロゴ画像
    public Image playerTurnImage;// プレイヤーターン開始時画像
    public Image enemyTurnImage;// 敵ターン開始時画像

    //-------------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        // UI初期化
        HideStatusWindow(); // ステータスウィンドウを隠す
        HideCommandButtons(); // コマンドボタンを隠す
    }

    //-------------------------------------------------------------------------

    /// <summary>
	/// ステータスウィンドウを表示する
	/// </summary>
	/// <param name="charaData">表示キャラクターデータ</param>
    public void ShowStatusWindow(Charactor charaData)
    {
        // オブジェクトアクティブ化
        statusWindow.SetActive(true);

        // 名前Text表示
        nameText.text = charaData.charaName;

        // 属性Image表示
        switch (charaData.attribute)
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

        // HPゲージ表示
        // 最大値に対する現在HPの割合をゲージImageのfillAmountにセットする
        float ratio = (float)charaData.NowHp / charaData.maxHP;
        hpGageImage.fillAmount = ratio;

        // HPText表示(現在値と最大値両方を表示)
        hpText.text = charaData.NowHp + "/" + charaData.maxHP;
        // 攻撃力Text表示(intからstringに変換)
        atkText.text = charaData.atk.ToString();
        // 防御力Text表示(intからstringに変換)
        defText.text = charaData.def.ToString();
    }

    //-------------------------------------------------------------------------

    /// <summary>
	/// ステータスウィンドウを隠す
	/// </summary>
    public void HideStatusWindow()
    {
        statusWindow.SetActive(false);
    }

    /// <summary>
	/// コマンドボタンを表示する
	/// </summary>
    public void ShowCommandButtons()
    {
        commandButtons.SetActive(true);
    }

    /// <summary>
	/// コマンドボタンを隠す
	/// </summary>
    public void HideCommandButtons()
    {
        commandButtons.SetActive(false);
    }

    /// <summary>
    /// プレイヤーのターンに切り替わった時のロゴ画像を表示する
    /// </summary>
    /// <param name="playerTurn"></param>true : 自ターン開始時のロゴ表示、false : 相手ターン開始時のロゴ表示
    public void ShowLogoChangeTurn(bool playerTurn)
    {
        // 徐々に表示→非表示を行うアニメーション(Tween)
        if (playerTurn)
        {
            playerTurnImage.DOFade(1.0f, 1.0f).SetEase(Ease.OutCubic).SetLoops(2, LoopType.Yoyo);
        }
        else
        {
            enemyTurnImage.DOFade(1.0f, 1.0f).SetEase(Ease.OutCubic).SetLoops(2, LoopType.Yoyo);
        }
    }
}
