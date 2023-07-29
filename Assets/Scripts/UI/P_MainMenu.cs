using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class P_MainMenu : MonoBehaviour
{
    public GameObject modalUsernameGO;
    public GameObject mainAreaGO;
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI profileAirasiaText;
    public string usernameValue;
    public TextMeshProUGUI timeoutDurationText;
    public Coroutine coroutinefindRoomTimeout;
    public GameObject findGameGO;
    public Button cancelFindGameBtn;
    public Button playButton, privateButton, playWithBotButton;
    public TextMeshProUGUI playText;
    public TextMeshProUGUI statusText, waitingForPlayersText;
    public GameObject offlineLoadingUI;


    void Start()
    {
        // Play Music
        //AudioManager.instance.PlayMusic2("PS_BGM_MainTheme", true);
        
        if(UIManager.instance.p_MainMenu == null){
            UIManager.instance.p_MainMenu = this;
            PhotonNetworkManager.instance.isInGame = false;
        }

        /* if(PlayerPrefs.HasKey("Username")){
            usernameValue = PlayerPrefs.GetString("Username");
            usernameText.text = usernameValue;
            profileAirasiaText.text = usernameValue;
        }else{
            modalUsernameGO.SetActive(true);
        } */

        if(ConfigReceiver.instance.configData.name != "" && !PhotonNetworkManager.instance.offlineMode){
            usernameValue = ConfigReceiver.instance.configData.name;
            usernameText.text = usernameValue;
            profileAirasiaText.text = usernameValue;
            PlayerPrefs.SetString("Username", usernameValue);
            PlayerPrefs.Save();
        }else{
            if(!PlayerPrefs.HasKey("Username") || PlayerPrefs.GetString("Username") == ""){
                modalUsernameGO.SetActive(true);
            }else{
                usernameValue = PlayerPrefs.GetString("Username");
                usernameText.text = usernameValue;
                profileAirasiaText.text = usernameValue;
            }
        }

        playButton.onClick.AddListener(delegate{  PhotonNetworkManager.instance.PlayOnlineGame(); }); // JoinTheGame(0)   Link playBtn with network manager join game
        cancelFindGameBtn.onClick.AddListener(delegate{  PhotonNetworkManager.instance.CancelFindGameOrLeaveRoom(); }); // JoinTheGame(0)   Link playBtn with network manager join game
        playWithBotButton.onClick.AddListener(delegate{ PhotonNetworkManager.instance.SetOffline();offlineLoadingUI.SetActive(true); AudioManager.instance.StopMusic(); }); // Link playWithBotBtn with network manager join game
    }
}
