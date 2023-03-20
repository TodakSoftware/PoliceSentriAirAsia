using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class GameUI : MonoBehaviourPunCallbacks
{
    public GameObject movementJoystick;
    public Toggle togglePet;
    public Button dashButton, itemButton, settingButton;
    public TextMeshProUGUI fpsText, redirectCountdownText;
    [Header("Lobby Related")]
    public GameObject lobbyButtonGroup;
    public Button lobbyLeaveGame, changeCharacterBtn, startCountGameBtn, cancelCountGameBtn;
    public bool hideRoomCode;
    string roomCode;
    public GameObject roomInfoGO;
    public TextMeshProUGUI roomCodeText;
    public GameObject copyCodeGO;

    [Header("Avatar Group")]
    public Transform avatarPoliceContent;
    public Transform avatarRobberContent;
    public GameObject avatarBtnPrefab;
    public List<Btn_Avatar> avatarBtnList = new List<Btn_Avatar>(); // added by PlayerController
    [Header("Timer Related")]
    public GameObject moneybagTimerGroup;
    public TextMeshProUGUI moneyTimerText;
    [Header("Notification")]
    public Transform feedsGroupContent;
    [Header("Setting Menu")]
    public Button settingLeaveBtn;

    void Start(){
        lobbyLeaveGame.onClick.AddListener(delegate{PhotonNetwork.LeaveRoom();});
        changeCharacterBtn.onClick.AddListener(delegate{UIManager.instance.PopupCharacterSelect();});
        //settingButton.onClick.AddListener(delegate{});
        settingLeaveBtn.onClick.AddListener(delegate{PhotonNetwork.LeaveRoom();});

        roomCode = PhotonNetwork.CurrentRoom.Name;
        roomCodeText.SetText(roomCode);
        HideCode();
    }

    // Room Code related
    public void HideCode(){
        if(!hideRoomCode){
            hideRoomCode = true;
            roomCodeText.text = "-----";
            copyCodeGO.SetActive(false);
        }else{
            hideRoomCode = false;
            roomCodeText.text = roomCode;
            copyCodeGO.SetActive(true);
        }
    }

    // Copy Code
    public void CopyCode(){
        GUIUtility.systemCopyBuffer = roomCode;
        NotificationManager.instance.PopupNotification("Room Code Copied!");
    }
}
