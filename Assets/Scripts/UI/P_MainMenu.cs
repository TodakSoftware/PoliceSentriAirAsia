using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class P_MainMenu : MonoBehaviour
{
    public GameObject modalUsernameGO;
    public TextMeshProUGUI usernameText;
    public string usernameValue;
    public TextMeshProUGUI timeoutDurationText;
    public Coroutine coroutinefindRoomTimeout;
    public GameObject findGameGO;
    public Button cancelFindGameBtn;
    public Button playButton, playWithBotButton;
    public TextMeshProUGUI playText;
    public TextMeshProUGUI statusText, waitingForPlayersText;


    void Start()
    {
        if(UIManager.instance.p_MainMenu == null){
            UIManager.instance.p_MainMenu = this;
            NetworkManager.instance.isInGame = false;
        }

        if(PlayerPrefs.HasKey("Username")){
            usernameValue = PlayerPrefs.GetString("Username");
            usernameText.text = usernameValue;
        }else{
            modalUsernameGO.SetActive(true);
        }

        playButton.onClick.AddListener(delegate{ NetworkManager.instance.PlayOnlineGame(); }); // JoinTheGame(0)   Link playBtn with network manager join game
        //playWithBotButton.onClick.AddListener(delegate{ NetworkManager.instance.SetOffline(); }); // Link playWithBotBtn with network manager join game
    }
}
