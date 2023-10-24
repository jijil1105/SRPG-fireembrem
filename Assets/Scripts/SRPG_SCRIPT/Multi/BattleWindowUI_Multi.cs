using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Photon.Pun;

public class BattleWindowUI_Multi : MonoBehaviourPun
{
    // バトル結果表示ウィンドウUI
    public Text nameText;// 名前Text
    public Image hpGageImage;// HPゲージImage
    public Text hpText;// HPText
    public Text damageText;// ダメージ量Text

    void Start()
    {
        // 初期化時にウィンドウを隠す
        HideWindow();
    }

	/// <summary>
	/// バトル結果ウィンドウを表示する
	/// </summary>
	/// <param name="charadata">攻撃されたキャラクターのデータ</param>
	/// <param name="damagevalue">ダメージ量</param>
	public void ShowWindow(Character_Multi charadata, int damagevalue)
	{
		// オブジェクトアクティブ化
		gameObject.SetActive(true);

		// 名前Text表示
		nameText.text = charadata.charaName;

		// ダメージ計算後の残りHPを取得する
		// (ここでは対象キャラクターデータのHPは変化させない)
		int nowHP = charadata.nowHp - damagevalue;
		// HPが0～最大値の範囲に収まるよう補正
		nowHP = Mathf.Clamp(nowHP, 0, charadata.maxHP);

		// HPゲージ表示
		float amount = (float)charadata.nowHp / charadata.maxHP;// 表示中のFillAmount(初期値はHP減少前のもの)
		float endAmount = (float)nowHP / charadata.maxHP;// アニメーション後のFillAmount
														 // HPゲージを徐々に減少させるアニメーション
														 // (DOFillAmountメソッドを使ってもよい)
		DOTween.To(// 変数を時間をかけて変化させる
				() => amount, (n) => amount = n, // 変化させる変数を指定
				endAmount, // 変化先の数値
				1.0f) // アニメーション時間(秒)
			.OnUpdate(() =>// アニメーション中毎フレーム実行される処理を指定
			{
				// 最大値に対する現在HPの割合をゲージImageのfillAmountにセットする
				hpGageImage.fillAmount = amount;
			});

		// HPText表示(現在値と最大値両方を表示)
		hpText.text = nowHP + "/" + charadata.maxHP;

		// ダメージ量Text表示
		if (damagevalue >= 0)
			damageText.text = damagevalue + "ダメージ！";

		// HP回復時
		else
			damageText.text = -damagevalue + "回復！";
	}

	public void Show_Window(int photonViewID, int damagevalue)
    {
		photonView.RPC(nameof(ShowWindow), RpcTarget.All, photonViewID, damagevalue);
    }

	[PunRPC]
	void ShowWindow(int photonviewID, int damagevalue)
	{
		// オブジェクトアクティブ化
		gameObject.SetActive(true);

		Character_Multi charadata = PhotonView.Find(photonviewID).GetComponent<Character_Multi>();

		// 名前Text表示
		nameText.text = charadata.charaName;

		// ダメージ計算後の残りHPを取得する
		// (ここでは対象キャラクターデータのHPは変化させない)
		int nowHP = charadata.nowHp - damagevalue;
		// HPが0～最大値の範囲に収まるよう補正
		nowHP = Mathf.Clamp(nowHP, 0, charadata.maxHP);

		// HPゲージ表示
		float amount = (float)charadata.nowHp / charadata.maxHP;// 表示中のFillAmount(初期値はHP減少前のもの)
		float endAmount = (float)nowHP / charadata.maxHP;// アニメーション後のFillAmount
														 // HPゲージを徐々に減少させるアニメーション
														 // (DOFillAmountメソッドを使ってもよい)
		DOTween.To(// 変数を時間をかけて変化させる
				() => amount, (n) => amount = n, // 変化させる変数を指定
				endAmount, // 変化先の数値
				1.0f) // アニメーション時間(秒)
			.OnUpdate(() =>// アニメーション中毎フレーム実行される処理を指定
			{
				// 最大値に対する現在HPの割合をゲージImageのfillAmountにセットする
				hpGageImage.fillAmount = amount;
			});

		// HPText表示(現在値と最大値両方を表示)
		hpText.text = nowHP + "/" + charadata.maxHP;

		// ダメージ量Text表示
		if (damagevalue >= 0)
			damageText.text = damagevalue + "ダメージ！";

		// HP回復時
		else
			damageText.text = -damagevalue + "回復！";
	}

	public void Hide_Window()
    {
		photonView.RPC(nameof(HideWindow), RpcTarget.All);
    }

	// <summary>
	/// バトル結果ウィンドウを隠す
	/// </summary>
    [PunRPC]
	void HideWindow()
    {
		// オブジェクト非アクティブ化
		gameObject.SetActive(false);
    }
}
