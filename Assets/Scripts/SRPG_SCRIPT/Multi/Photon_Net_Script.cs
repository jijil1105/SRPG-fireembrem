using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using System;
using UnityEngine.UI;
using DG.Tweening;

public class Photon_Net_Script : MonoBehaviourPunCallbacks
{
    public string MaseterClientId;
    private string[] teamUserIDs;
    [SerializeField] AudioManager audioManager;
    [SerializeField] Image fadeimage;
    [SerializeField] Button multi_button;

    // Start is called before the first frame update
    void Start()
    {
        CharacterSerializer.Register();

        //セーブデータ読み込み
        SaveData data = DataManager._instance.Load();

        //セーブデータからキャラステータス反映（仮）、セーブデータに保存されているマップへ遷移
        if (data != null && data.SceneName != "Delete Data")
        {
            multi_button.gameObject.SetActive(true);
        }
        else
            multi_button.gameObject.SetActive(false);
    }

    public void Multi_Button()
    {
        AudioManager.instance.Play("SE_1");

        FadeOut(2);

        DOVirtual.DelayedCall(3, () =>
        {
            PhotonNetwork.ConnectUsingSettings();
        });
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinedRoom()
    {
        //セーブデータ読み込み
        SaveData data = DataManager._instance.Load();

        //セーブデータからキャラステータス反映（仮）、セーブデータに保存されているマップへ遷移
        if (data != null && data.SceneName != "Delete Data")
        {
            for (int i = 0; i < data.atk.Count; i++)
            {
                Debug.Log(
                    data.name[i] + ":" +
                    data.maxHp[i] + ":" +
                    data.atk[i] + ":" +
                    data.def[i] + ":" +
                    data.Int[i] + ":" +
                    data.res[i] + ":" +
                    data.atrr[i] + ":" +
                    data.movetype[i] + ":" +
                    data.skill[i] + ":" +
                    data.isMagicAttack[i] + ":" +
                    data.Lv[i] + ":" +
                    data.nowExp[i]
                    );
            }
            /*
		    public string charaName;//キャラ名
		    public int maxHP;//最大Hp
		    public int atk;//物理攻撃力
		    public int def;//物理防御力
		    public int Int;//魔法攻撃力
		    public int Res;//魔法防御力
		    public Attribute attribute;// 属性
		    public MoveType moveType;//移動タイプ
		    public SkillDefine.Skill skill;//スキル
		    public bool isMagicAttac;//魔法攻撃flg

		    public int Lv;//レベル
		    public int nowExp;//現在の経験値
		    public int ExpPerLv;//次のレベルに必要な経験値
            */

            MaseterClientId = PhotonNetwork.MasterClient.UserId;
            SetTeamUserIDsWithCurrentRoom();

            PhotonNetwork.LoadLevel("Battle_1_Multi");
        }
        else
            Debug.Log("Dont have Data");
    }

    public override void OnJoinedLobby()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        RoomOptions roomOptions = new RoomOptions();

        roomOptions.MaxPlayers = 2;
        PhotonNetwork.CreateRoom(null, roomOptions);
    }



    private void SetTeamUserIDsWithCurrentRoom()
    {
        teamUserIDs = new string[PhotonNetwork.PlayerListOthers.Length];
        for (int i = 0; i < PhotonNetwork.PlayerListOthers.Length; i++)
        {
            teamUserIDs[i] = PhotonNetwork.PlayerListOthers[i].UserId;
        }
    }

    private void FadeIn(float duration, Action on_completed = null)
    {
        StartCoroutine(FadeCoroutine(duration, on_completed, true));
    }

    private void FadeOut(float duration, Action on_completed = null)
    {
        StartCoroutine(FadeCoroutine(duration, on_completed));
    }

    private IEnumerator FadeCoroutine(float duration, Action on_compleated, bool is_reversing = false)
    {
        if (!is_reversing) fadeimage.enabled = true;

        var elapsed_time = 0.0f;
        var color = fadeimage.color;

        while (elapsed_time < duration)
        {
            var elapased_rate = Mathf.Min(elapsed_time / duration, 1.0f);
            color.a = is_reversing ? 1.0f - elapased_rate : elapased_rate;
            fadeimage.color = color;

            yield return null;
            elapsed_time += Time.deltaTime;
        }
    }
}



/*public class PTFNetworkManager : MonoBehaviourPunCallbacks
{
    private static PTFNetworkState state = PTFNetworkState.Idle;

    public string leaderUserID;
    private string[] teamUserIDs;

    public static PTFNetworkState State
    {
        get { return state; }
    }

    void Update()
    {
        switch (state)
        {
            case PTFNetworkState.FindAlly:
                if (PhotonNetwork.InRoom)
                {
                    if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers && PhotonNetwork.MasterClient.UserId != null)
                    {
                        //人数が揃ったらリーダーとチームメンバーを設定
                        leaderUserID = PhotonNetwork.MasterClient.UserId;
                        SetTeamUserIDsWithCurrentRoom();
                        //準備完了フラグを立てる
                        var properties = new ExitGames.Client.Photon.Hashtable();
                        properties.Add("foundAlly", true);
                        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
                        //ステート切り替え
                        state = PTFNetworkState.FoundAlly;
                    }
                }

                break;
            case PTFNetworkState.FoundAlly:
                //全員が設定を終えたら退室
                if (CheckAllPlayerFoundAlly())
                {
                    Debug.Log("Found Ally!");
                    PhotonNetwork.LeaveRoom();
                    PhotonNetwork.JoinLobby();
                    state = PTFNetworkState.FindOpponent;
                }
                break;
            case PTFNetworkState.FindOpponent:
                //リーダージャナイバアイ
                if (leaderUserID != PhotonNetwork.LocalPlayer.UserId)
                {
                    PhotonNetwork.FindFriends(new string[1] { leaderUserID });
                }

                if (PhotonNetwork.InRoom)
                {
                    //部屋に入ったら準備完了
                    Debug.Log("Ready! Waiting others or start!");
                    state = PTFNetworkState.Ready;
                }

                break;
            case PTFNetworkState.Ready:
                //人数が揃ったら開始するとか。
                break;
        }
    }

    public override void OnPlayerPropertiesUpdate(Player target, ExitGames.Client.Photon.Hashtable changedProps)
    {
        Debug.Log("target is " + target + "\n changed props are" + changedProps);
    }

    /// <summary>
    /// 現在の設定でマッチメイキングを開始
    /// </summary>
    public static void StartMatchMaking()
    {
        //ステートは味方を探す
        state = PTFNetworkState.FindAlly;
        ConnectToMaster();
    }

    private static void ConnectToMaster()
    {
        string gameMode = PlayerPrefs.GetString("GameMode", "Skirmish");

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("Connecting to master...");
        }
        else
        {
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();

        Debug.Log("Joining Lobby...");
    }

    /// <summary>
    /// ロビーに入ったらとりあえず見方を探す
    /// </summary>
    public override void OnJoinedLobby()
    {
        Debug.Log("Connected Master Server!");

        if (state == PTFNetworkState.FindAlly)
        {
            FindAlly();
        }
        else if (state == PTFNetworkState.FindOpponent)
        {
            FindOpponent();
        }
    }

    private void FindAlly()
    {
        string gameMode = PlayerPrefs.GetString("GameMode", "Skirmish");

        //ここから見方を探す
        //味方のプレイヤーの人数
        byte expectedMaxPlayers = 1;
        //想定プレイヤー
        //GameMode(gm)とFindType(ft)でソート
        ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "gm", "skirmish" }, { "ft", "ally" } };

        switch (gameMode)
        {
            case "Skirmish":
                expectedMaxPlayers = 1;
                expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "gm", "skirmish" }, { "ft", "ally" } };
                break;
            case "Solo":
                expectedMaxPlayers = 1;
                expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "gm", "solo" }, { "ft", "ally" } };
                break;
            case "Duo":
                expectedMaxPlayers = 2;
                expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "gm", "duo" }, { "ft", "ally" } };
                break;
        }

        PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, expectedMaxPlayers);
    }

    /// <summary>
    ///相手を探す
    /// リーダーのみ利用。
    /// </summary>
    private void FindOpponent()
    {
        Debug.Log("leaderUserID : " + leaderUserID);
        Debug.Log("myUserID : " + PhotonNetwork.LocalPlayer.UserId);
        if (leaderUserID != PhotonNetwork.LocalPlayer.UserId) return;
        Debug.Log("I am leader! I start finding opponent!!");
        string gameMode = PlayerPrefs.GetString("GameMode", "Skirmish");

        //ここから相手を探す
        //全体のプレイヤーの人数
        byte expectedMaxPlayers = 1;
        //想定プレイヤー
        //GameMode(gm)とFindType(ft)でソート
        ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "gm", "skirmish" }, { "ft", "opponent" } };

        switch (gameMode)
        {
            case "Skirmish":
                expectedMaxPlayers = 8;
                expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "gm", "skirmish" }, { "ft", "opponent" } };
                break;
            case "Solo":
                expectedMaxPlayers = 2;
                expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "gm", "solo" }, { "ft", "opponent" } };
                break;
            case "Duo":
                expectedMaxPlayers = 4;
                expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "gm", "duo" }, { "ft", "opponent" } };
                break;
        }

        //ランダムマッチメイキング
        PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, expectedMaxPlayers, MatchmakingMode.FillRoom, TypedLobby.Default, null, teamUserIDs);

        //ステートは相手を探す
        state = PTFNetworkState.FindOpponent;
    }

    /// <summary>
    /// ルーム入室失敗時、ステートにあった部屋を作成する
    /// </summary>
    /// <param name="returnCode"></param>
    /// <param name="message"></param>
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Faild to Join Room!");

        Debug.Log(message);

        string gameMode = PlayerPrefs.GetString("GameMode", "Skirmish");

        //ルームオプションの設定
        string findType = "";
        if (state == PTFNetworkState.FindAlly)
        {
            findType = "ally";
        }
        else if (state == PTFNetworkState.FindOpponent)
        {
            findType = "opponent";
        }
        byte expectedMaxPlayers = 1;
        ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "gm", "skirmish" } };

        switch (gameMode)
        {
            case "Skirmish":
                if (state == PTFNetworkState.FindAlly)
                {
                    expectedMaxPlayers = 1;
                }
                else if (state == PTFNetworkState.FindOpponent)
                {
                    expectedMaxPlayers = 8;
                }
                expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "gm", "skirmish" }, { "ft", findType } };
                break;
            case "Solo":
                if (state == PTFNetworkState.FindAlly)
                {
                    expectedMaxPlayers = 1;
                }
                else if (state == PTFNetworkState.FindOpponent)
                {
                    expectedMaxPlayers = 2;
                }
                expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "gm", "solo" }, { "ft", findType } };
                break;
            case "Duo":
                if (state == PTFNetworkState.FindAlly)
                {
                    expectedMaxPlayers = 2;
                }
                else if (state == PTFNetworkState.FindOpponent)
                {
                    expectedMaxPlayers = 4;
                }
                expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "gm", "duo" }, { "ft", findType } };
                break;
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "gm", "ft" };
        roomOptions.CustomRoomProperties = expectedCustomRoomProperties;
        roomOptions.MaxPlayers = expectedMaxPlayers;
        roomOptions.PublishUserId = true;

        Debug.Log(roomOptions.CustomRoomProperties);

        //ルームの作成
        PhotonNetwork.CreateRoom("", roomOptions, null, teamUserIDs);

        Debug.Log("Created Room!");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room!");
        Debug.Log(PhotonNetwork.CurrentRoom.CustomProperties);

        if (state == PTFNetworkState.FindAlly)
        {
            var properties = new ExitGames.Client.Photon.Hashtable();
            properties.Add("foundAlly", false);
            PhotonNetwork.SetPlayerCustomProperties(properties);
        }
    }

    private void SetTeamUserIDsWithCurrentRoom()
    {
        teamUserIDs = new string[PhotonNetwork.PlayerListOthers.Length];
        for (int i = 0; i < PhotonNetwork.PlayerListOthers.Length; i++)
        {
            teamUserIDs[i] = PhotonNetwork.PlayerListOthers[i].UserId;
        }
    }

    public override void OnFriendListUpdate(List<FriendInfo> friendList)
    {
        foreach (FriendInfo friendInfo in friendList)
        {
            if (friendInfo.UserId == leaderUserID && state == PTFNetworkState.FindOpponent)
            {
                PhotonNetwork.JoinRoom(friendInfo.Room);
            }
        }
    }

    private bool CheckAllPlayerFoundAlly()
    {
        Player[] playerList = PhotonNetwork.PlayerList;
        foreach (Player player in playerList)
        {
            if (player.CustomProperties["foundAlly"] == null)
            {
                return false;
            }

            if (!(bool)player.CustomProperties["foundAlly"])
            {
                return false;
            }
        }

        return true;
    }
}

public enum PTFNetworkState
{
    FoundAlly,
    Idle,
    FindAlly,
    FindOpponent,
    Ready
}*/