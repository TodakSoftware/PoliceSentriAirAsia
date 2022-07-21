using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class P_MainMenu : MonoBehaviour
{
    public GameObject modalUsernameGO;
    public TextMeshProUGUI usernameText;
    public string usernameValue;
    public TextMeshProUGUI timeoutDurationText;
    public Coroutine coroutinefindRoomTimeout;
    public GameObject findGameGO;
    public Button cancelFindGameBtn;
    public Button playButton;
    public TextMeshProUGUI playText;

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

        playButton.onClick.AddListener(delegate{ NetworkManager.instance.JoinTheGame(0); }); // Link playBtn with network manager join game
    }
}
