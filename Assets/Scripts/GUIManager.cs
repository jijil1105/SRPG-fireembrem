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

    public Text ExpText;//経験値テキスト
    public Image ExpGageImage;//経験値ゲージ

    public Text LvText;//レベル表示テキスト

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
    public Image gameClearImage;//ゲームクリア時画像
    public Image gameOverImage;//ゲームオーバー時画像

    //-------------------------------------------------------------------------

    //行動キャンセルボタン
    public GameObject moveCancelButton;

    //行動決定ボタン
    public GameObject decideButtons;

    //-------------------------------------------------------------------------

    //シーン遷移のフェード画像
    public Image fadeImage;

    //-------------------------------------------------------------------------

    public GameObject GetExpWindow;//取得経験値を表示するウィンドウ
    public Image GetExpWindow_ExpBar;//経験値バー
    public Text GetExpWindow_charaname_text;//経験値を取得したキャラの名前

    //-------------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        // UI初期化
        HideStatusWindow(); // ステータスウィンドウを隠す
        HideCommandButtons(); // コマンドボタンを隠す
        ShowMoveCancelButton(false);//キャンセルボタンを隠す
        HideDecideButtons(); // 行動決定・キャンセルボタンを隠す
        HideGetExpWindow();
    }

    //-------------------------------------------------------------------------



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

        // 属性Image表示（キャラの属性によってアイコン変更）
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

        //キャラのレベルをステータスウィンドウにに表示
        LvText.text = "Lv : " + charaData.Lv;

        //キャラの次レベルまでに必要な経験値と現在の経験値の割合を経験値バーのfillAmountにセット
        float Expratio = (float)charaData.nowExp / charaData.ExpPerLv;
        ExpGageImage.fillAmount = Expratio;

        //経験値テキストに表示
        ExpText.text = charaData.nowExp + "/" + charaData.ExpPerLv;
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

    //-------------------------------------------------------------------------

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

    //-------------------------------------------------------------------------

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

    //-------------------------------------------------------------------------

    /// <summary>
    /// 画面をフェードインさせる
    /// </summary>
    /// <param name="duration"></param>
    public void FadeIn(float duration)
    {
         //フェードイン処理
         fadeImage.DOFade(1.0f, duration).SetEase(Ease.Linear);

         //フェードの当たり判定をオン
         fadeImage.raycastTarget = true;           
    }

    //-------------------------------------------------------------------------

    /// <summary>
    /// 決定ボタンを表示
    /// </summary>
    public void ShowDecideButtons()
    {
        decideButtons.SetActive(true);
    }

    //-------------------------------------------------------------------------

    /// <summary>
    /// 決定ボタン非表示
    /// </summary>
    public void HideDecideButtons()
    {
        decideButtons.SetActive(false);
    }

    //-------------------------------------------------------------------------

    public void ShowGetExpWindow(Charactor charadata)
    {
        GetExpWindow_charaname_text.text = charadata.charaName + ":" + charadata.Lv;
        GetExpWindow.SetActive(true);
    }

    //-------------------------------------------------------------------------

    public void HideGetExpWindow()
    {
        GetExpWindow.SetActive(false);
    }

    //-------------------------------------------------------------------------

    public void moveExpbar(float startvalue, float endvalue, float duration)
    {
        GetExpWindow_ExpBar.fillAmount = startvalue;
        GetExpWindow_ExpBar.DOFillAmount(endvalue, duration);
    }
}
