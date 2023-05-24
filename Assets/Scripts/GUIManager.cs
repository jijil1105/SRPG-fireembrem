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
    public GameObject commandButtons;// 全コマンドボタンの親オブジェクト
    public Button skillCommandButton;// 特技コマンドのButton
    public Text skillText;// 選択キャラクターの特技の説明Text

    //-------------------------------------------------------------------------

    // 各種ロゴ画像
    public Image playerTurnImage;// プレイヤーターン開始時画像
    public Image enemyTurnImage;// 敵ターン開始時画像
    public Image gameClearImage;
    public Image gameOverImage;

    //-------------------------------------------------------------------------

    //
    public GameObject moveCancelButton;

    //-------------------------------------------------------------------------

    //
    public Image fadeImage;

    //-------------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        // UI初期化
        HideStatusWindow(); // ステータスウィンドウを隠す
        HideCommandButtons(); // コマンドボタンを隠す
        ShowMoveCancelButton(false);//キャンセルボタンを隠す
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

        if (charaData.isDefBreak)//防御力０化状態以上だったら赤色で強調表示
            defText.text = "<color=red>0</color>";

        else// 防御力Text表示(intからstringに変換)
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

    //-------------------------------------------------------------------------

    /// <summary>
    /// コマンドボタンを表示する
    /// </summary>
    /// <param name="charaData">SkillInfo表示に使用するキャラデータ</param>
    public void ShowCommandButtons(Charactor charaData)
    {
        commandButtons.SetActive(true);

        SkillDefine.Skill skill = charaData.skill;
        string skillname = SkillDefine.dec_SkillName[skill];
        string skillInfo = SkillDefine.dec_SkillInfo[skill];

        skillText.text = "<size=40>" + skillname + "</size>\n" + skillInfo;

        // スキル使用不可状態なら特技ボタンを押せなくする
        if (charaData.isSkillLock)
            skillCommandButton.interactable = false;
        else
            skillCommandButton.interactable = true;
    }

    //-------------------------------------------------------------------------

    /// <summary>
	/// コマンドボタンを隠す
	/// </summary>
    public void HideCommandButtons()
    {
        commandButtons.SetActive(false);
    }

    //-------------------------------------------------------------------------

    /// <summary>
    /// プレイヤーのターンに切り替わった時のロゴ画像を表示する
    /// </summary>
    /// <param name="playerTurn">true : 自ターン開始時のロゴ表示、false : 相手ターン開始時のロゴ表示</param>
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

    //-------------------------------------------------------------------------

    /// <summary>
    /// キャンセルボタンの表示、非表示
    /// </summary>
    /// <param name="setFlg">trueでキャンセルボタン表示</param>
    public void ShowMoveCancelButton(bool setFlg)
    {
        moveCancelButton.SetActive(setFlg);
    }

    /// <summary>
    /// ゲームクリア時のロゴを表示する
    /// </summary>
    public void ShowLogo_GameClear()
    {
        gameClearImage.DOFade(
            1.0f,
            1.0f)
            .SetEase(Ease.OutCubic);

        gameClearImage.transform.DOScale(
            1.5f,
            1.0f)
            .SetEase(Ease.OutCubic)
            .SetLoops(2, LoopType.Yoyo);
    }

    /// <summary>
    /// ゲームオーバー時のロゴを表示する
    /// </summary>
    public void ShowLogo_gameOver()
    {
        gameOverImage.DOFade(
            1.0f,
            1.0f)
            .SetEase(Ease.OutCubic);
    }

    public void FadeIn(float duration)
    {
         //フェードイン処理
         fadeImage.DOFade(1.0f, duration).SetEase(Ease.Linear);

         //フェードの当たり判定をオン
         fadeImage.raycastTarget = true;           
    }

    
}
