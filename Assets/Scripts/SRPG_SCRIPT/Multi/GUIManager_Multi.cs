using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;

public class GUIManager_Multi : MonoBehaviourPun
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

    //経験値バーを表示するWindow
    public GameObject GetExpWindow;//取得経験値を表示するウィンドウ
    public Image GetExpWindow_ExpBar;//経験値バー
    public Text GetExpWindow_charaname_text;//経験値を取得したキャラの名前

    //-------------------------------------------------------------------------

    //レベルアップ時のステータス上昇値を表示させるウィンドウ
    public GameObject LevelUpWindow;
    public Text levelupwindow_name_text;
    public Text levelupwindow_hp_text;
    public Text levelupwindow_atk_text;
    public Text levelupwindow_def_text;
    public Text levelupwindow_int_text;
    public Text levelupwindow_res_text;

    //-------------------------------------------------------------------------

    public GameObject waiting_window;
    public Image wait_untill_backimage;
    public Text wait_text;

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
        HideLevelUpWindow();
    }

    //-------------------------------------------------------------------------

    public void ShowWaitingWindow()
    {
        waiting_window.SetActive(true);
        wait_text.text = "Waiting Other Player Enter this Room";

        //UniRxかEventで文字送り実装予定
    }

    public void HideWaitingWindow()
    {
        waiting_window.SetActive(false);
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

        if (charaData.isMagicAttac)
            // 魔法攻撃力Text表示(intからstringに変換)
            atkText.text = charaData.Int.ToString();
        else
            // 物理攻撃力Text表示(intからstringに変換)
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="charaData"></param>
    public void ShowStatusWindow(Character_Multi charaData)
    {
        // オブジェクトアクティブ化
        statusWindow.SetActive(true);

        // 名前Text表示
        nameText.text = charaData.charaName;

        // 属性Image表示（キャラの属性によってアイコン変更）
        switch (charaData.attribute)
        {
            case Character_Multi.Attribute.Water:
                attributeIcon.sprite = attr_Water;
                break;
            case Character_Multi.Attribute.Fire:
                attributeIcon.sprite = attr_Fire;
                break;
            case Character_Multi.Attribute.Wind:
                attributeIcon.sprite = attr_Wind;
                break;
            case Character_Multi.Attribute.Soil:
                attributeIcon.sprite = attr_Soil;
                break;
        }

        // HPゲージ表示
        // 最大値に対する現在HPの割合をゲージImageのfillAmountにセットする
        float ratio = (float)charaData.nowHp / charaData.maxHP;
        hpGageImage.fillAmount = ratio;

        // HPText表示(現在値と最大値両方を表示)
        hpText.text = charaData.nowHp + "/" + charaData.maxHP;

        if (charaData.isMagicAttac)
            // 魔法攻撃力Text表示(intからstringに変換)
            atkText.text = charaData.Int.ToString();
        else
            // 物理攻撃力Text表示(intからstringに変換)
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

    // <summary>
    /// コマンドボタンを表示する
    /// </summary>
    /// <param name="charaData">SkillInfo表示に使用するキャラデータ</param>
    public void ShowCommandButtons(Character_Multi charaData)
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

    public void Sync_Show_Logo(bool isMyTurne)
    {
        photonView.RPC(nameof(SyncShowLogo), RpcTarget.All, isMyTurne);
    }

    [PunRPC]
    void SyncShowLogo(bool Turne)
    {
        if(Turne)
        {
            if (PhotonNetwork.MasterClient.UserId == PhotonNetwork.LocalPlayer.UserId)
                ShowLogoChangeTurn(false);
            else
                ShowLogoChangeTurn(true);
        }
        else
        {
            if (PhotonNetwork.MasterClient.UserId != PhotonNetwork.LocalPlayer.UserId)
                ShowLogoChangeTurn(false);
            else
                ShowLogoChangeTurn(true);
        }
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="charadata"></param>
    public void ShowGetExpWindow(Charactor charadata)
    {
        GetExpWindow_charaname_text.text = charadata.charaName + ":" + charadata.Lv;
        GetExpWindow.SetActive(true);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="charadata"></param>
    public void ShowGetExpWindow(Character_Multi charadata)
    {
        GetExpWindow_charaname_text.text = charadata.charaName + ":" + charadata.Lv;
        GetExpWindow.SetActive(true);
    }

    //-------------------------------------------------------------------------

    /// <summary>
    /// 
    /// </summary>
    public void HideGetExpWindow()
    {
        GetExpWindow.SetActive(false);
    }

    //-------------------------------------------------------------------------

    /// <summary>
    /// 
    /// </summary>
    /// <param name="startvalue"></param>
    /// <param name="endvalue"></param>
    /// <param name="duration"></param>
    public void moveExpbar(float startvalue, float endvalue, float duration)
    {
        GetExpWindow_ExpBar.fillAmount = startvalue;
        GetExpWindow_ExpBar.DOFillAmount(endvalue, duration);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="charaData"></param>
    /// <param name="list"></param>
    public void ShowLeveUpWindow(Charactor charaData, List<int> list)
    {
        LevelUpWindow.SetActive(true);
        levelupwindow_name_text.text = charaData.charaName + " : LV" + charaData.Lv;

        if (list[0] != 0)
            levelupwindow_hp_text.text = "Hp : " + charaData.maxHP + " <color=red> +" + list[0] + "</color>";
        else
            levelupwindow_hp_text.text = "Hp : " + charaData.maxHP;

        if (list[1] != 0)
            levelupwindow_atk_text.text = "Atk : " + charaData.atk + " <color=red> +" + list[1] + "</color>";
        else
            levelupwindow_atk_text.text = "Atk : " + charaData.atk;

        if (list[2] != 0)
            levelupwindow_def_text.text = "Def : " + charaData.def + " <color=red> +" + list[2] + "</color>";
        else
            levelupwindow_def_text.text = "Def : " + charaData.def;

        if (list[3] != 0)
            levelupwindow_int_text.text = "Int : " + charaData.Int + " <color=red> +" + list[3] + "</color>";
        else
            levelupwindow_int_text.text = "Int : " + charaData.Int;

        if (list[4] != 0)
            levelupwindow_res_text.text = "Res : " + charaData.Res + " <color=red> +" + list[4] + "</color>";
        else
            levelupwindow_res_text.text = "Res : " + charaData.Res;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="charaData"></param>
    /// <param name="list"></param>
    public void ShowLeveUpWindow(Character_Multi charaData, List<int> list)
    {
        LevelUpWindow.SetActive(true);
        levelupwindow_name_text.text = charaData.charaName + " : LV" + charaData.Lv;

        if (list[0] != 0)
            levelupwindow_hp_text.text = "Hp : " + charaData.maxHP + " <color=red> +" + list[0] + "</color>";
        else
            levelupwindow_hp_text.text = "Hp : " + charaData.maxHP;

        if (list[1] != 0)
            levelupwindow_atk_text.text = "Atk : " + charaData.atk + " <color=red> +" + list[1] + "</color>";
        else
            levelupwindow_atk_text.text = "Atk : " + charaData.atk;

        if (list[2] != 0)
            levelupwindow_def_text.text = "Def : " + charaData.def + " <color=red> +" + list[2] + "</color>";
        else
            levelupwindow_def_text.text = "Def : " + charaData.def;

        if (list[3] != 0)
            levelupwindow_int_text.text = "Int : " + charaData.Int + " <color=red> +" + list[3] + "</color>";
        else
            levelupwindow_int_text.text = "Int : " + charaData.Int;

        if (list[4] != 0)
            levelupwindow_res_text.text = "Res : " + charaData.Res + " <color=red> +" + list[4] + "</color>";
        else
            levelupwindow_res_text.text = "Res : " + charaData.Res;
    }

    /// <summary>
    /// 
    /// </summary>
    public void HideLevelUpWindow()
    {
        LevelUpWindow.SetActive(false);
    }
}
