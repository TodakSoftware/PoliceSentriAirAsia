using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviourPunCallbacks
{
    public bool fillWithBots;
    public static GameManager instance;

    [Header("Spawnpoints")]
    public Transform waitingSpawnpoint;
    public Transform jailSpawnpoint;
    public List<Transform> policeSpawnpoints = new List<Transform>();
    public List<Transform> robberSpawnpoints = new List<Transform>();
    public List<Transform> moneybagSpawnpoints = new List<Transform>();
    public GameObject botEscapeGO;
    [HideInInspector] public List<Transform> botEscapeSpawnpoints = new List<Transform>();

    [Header("Moneybag Related")]
    [SerializeField] int timerStart; // <-- Will link to Game Mode settings (Clock Start)
    [SerializeField] int timerEnd; // <-- Will link to Game Mode settings (Clock End)
    [SerializeField] int currentTimer;
    public float noMoneybagTimer;
    List<GameObject> moneybagList = new List<GameObject>();
    public bool moneyBagOccupied, moneybagSpawned; // Just for debugging. TRUE : Destroy moneybag, FALSE : Spawn moneybag
    float pickupMoneybagDuration = 5f, penaltyDuration = 10f; // pickupMoneybagDuration : if dropped, amount of robber has to pickup. || penaltyDuration : Time Addition if no robber pickup. (Spawn new moneybag)
    Coroutine deductMoneybagTimerCoroutine;
    bool isDeducting;

    [Header("Game Mode")]
    [SerializeField] int gameModeIndex = 0; // <-- 0 = Cop n Robber
    C_GameMode gameModeData; // caching SO game mode data

    [Header("Ingame Info")]
    public bool gameStarted;
    public bool gameEnded;
    public int startGameCountdown = 20; // Start Countdown Duration
    [SerializeField] int currentStartGameCountdown; // Start Countdown *NOT Moneybag countdown
    Coroutine storedCurrentCountdown;
    [HideInInspector] public GameObject ownedPlayerGO; // The player who are currently owned the game manager (GameObject)
    public int currentIngamePolice, currentIngameRobber;

    [Header("EndGame Related")]
    public bool donePopupWinUI;
    public GameObject endScreenGO;
    public TextMeshProUGUI endScreenRedirectText;
    public int playAgainCount;
    bool askForHelpActivated;
    bool doneSpawnBots;
    [Header("Bot Related")]
    public List<string> policeNames = new List<string>();
    public List<string> robberNames = new List<string>();
    [Header("UI Popup Related")]
    public GameObject policeTutorialGO;
    public GameObject robberTutorialGO;
    private int spawnBotPoliceCount, spawnBotRobberCount;


    void Awake(){
        Application.targetFrameRate = -1;
        if(instance == null){
            instance = this;
        }else{
            Destroy(this.gameObject);
        }

        
    } // end Awake

    void Start(){
        Invoke("DelayMusic", 2f);

        if(PhotonNetwork.CurrentRoom.IsVisible){ // only if public
            // Bot add escape points
            if(botEscapeGO != null){
                foreach(var child in botEscapeGO.GetComponentsInChildren<Transform>()){
                    if(child.CompareTag("BotEscape")){
                        botEscapeSpawnpoints.Add(child);
                    }
                }
            }
        }
        

        UIManager.instance.RefreshMainCanvas(); // Make sure we have main canvas
        UIManager.instance.RefreshControllerGroup(); // Make sure we have reference the CanvasGroup

        UIManager.instance.PopupManualSelectRole(); // Popup Role Select UI
        UIManager.instance.PopupCharacterSelect(); // Popup Character Select
        StartCoroutine(UIManager.instance.CloseCharacterSelect(0)); // Close the char selection UI

        StartCoroutine(UIManager.instance.ShowAndCloseLoadingScene(1f));
        
        
        // Get Game Mode data
        foreach(var gm in SOManager.instance.gameSettings.gameMode){
            if(gm.gameModeIndex == gameModeIndex){
                gameModeData = gm;
            }
        } // end foreach loop

        timerStart = gameModeData.clockStartTime;
        timerEnd = gameModeData.clockEndTime;
        currentTimer = timerStart; // set current timer = timerStart

        // Link with UI
        int minutes = Mathf.FloorToInt(currentTimer / 60f);
        int seconds = Mathf.FloorToInt(currentTimer - minutes * 60f);
        string formattedTimer = string.Format("{0:0}:{1:00}", minutes, seconds);
        UIManager.instance.gameUI.moneyTimerText.text = formattedTimer;

        if((int)PhotonNetwork.CurrentRoom.CustomProperties["RoomPrivate"] == 0 || PhotonNetwork.OfflineMode){ // only if public
            currentStartGameCountdown = startGameCountdown;
            StartCoroutine(StartGameCountdown());
            UIManager.instance.gameUI.startCountGameBtn.gameObject.SetActive(false);
            UIManager.instance.gameUI.cancelCountGameBtn.gameObject.SetActive(false);
            UIManager.instance.gameUI.roomInfoGO.gameObject.SetActive(false);
        }else{
            if(PhotonNetwork.IsMasterClient){
                UIManager.instance.gameUI.startCountGameBtn.gameObject.SetActive(true);
                UIManager.instance.gameUI.startCountGameBtn.onClick.AddListener(() => {
                    AudioManager.instance.PlaySound("PS_UI_Button_Click");

                    // SINI
                    if(GetAllPlayersPolice().Count > 0 && GetAllPlayersRobber().Count > 0){
                        UIManager.instance.gameUI.startCountGameBtn.gameObject.SetActive(false);
                        UIManager.instance.gameUI.cancelCountGameBtn.gameObject.SetActive(true);

                        photonView.RPC("StartCountdownRPC", RpcTarget.All);
                    }else{
                        NotificationManager.instance.PopupNotification("Atleast both team have player(s)");
                    }
                    

                    // Disable 
                });
            }

            if(PhotonNetwork.IsMasterClient){
                UIManager.instance.gameUI.cancelCountGameBtn.onClick.AddListener(() => {
                    if(storedCurrentCountdown != null){
                        AudioManager.instance.PlaySound("PS_UI_Button_Click");
                        photonView.RPC("StopCountdownRPC", RpcTarget.All);
                        print("Cancelled");

                        UIManager.instance.gameUI.startCountGameBtn.gameObject.SetActive(true);
                        UIManager.instance.gameUI.cancelCountGameBtn.gameObject.SetActive(false);

                        if(!UIManager.instance.gameUI.lobbyLeaveGame.interactable){
                            UIManager.instance.gameUI.lobbyLeaveGame.interactable = true;
                        }

                        UIManager.instance.gameUI.redirectCountdownText.text = "";
                    }
                    // Disable 
                });
            }
            //UIManager.instance.gameUI.cancelCountGameBtn.gameObject.SetActive(false);
        }

        PhotonNetworkManager.instance.isInGame = true;

        UpdateTeamCount(); // Update Manual Select Role on 1st load

        SpawnAllMoneybag(); // Spawn moneybag   

        PhotonNetwork.IsMessageQueueRunning = true;
    } // end Start

    [PunRPC]
    public void StartCountdownRPC(){
        currentStartGameCountdown = 3;
        storedCurrentCountdown = StartCoroutine(StartGameCountdown());
    }

    [PunRPC]
    public void StopCountdownRPC(){
        StopCoroutine(storedCurrentCountdown);
        storedCurrentCountdown = null;

        UIManager.instance.gameUI.redirectCountdownText.text = "";
    }

    public void DelayMusic(){
         // Play Music
        AudioManager.instance.PlayMusic2("PS_BGM_LobbySong", false);
    }

    public void delayMusicStart(){
        AudioManager.instance.PlayMusic2("PS_BGM_MainTheme", true);
    }

    void Update(){
        // Moneybag Timer Related
        if(gameStarted && !gameEnded){
            if(moneyBagOccupied){
                if(!isDeducting){
                    isDeducting = true;
                    deductMoneybagTimerCoroutine = StartCoroutine(DeductMoneybagTimer()); // Start timer countdown
                }
            }else{
                if(isDeducting){
                    isDeducting = false;
                    StopCoroutine(deductMoneybagTimerCoroutine); // Stop timer countdown
                }
            }

           /*  if(Input.GetKeyDown(KeyCode.C)){ // Debug ask for robber help
                GameManager.instance.ChosenBotToRescue();
            } */
        } // end gameStarted

        //if(!gameEnded && currentStartGameCountdown <= 8 && !doneSpawnBots && GetAllPlayers().Count == (int)PhotonNetwork.CurrentRoom.CustomProperties["RealTotalPlayer"] && PhotonNetwork.CurrentRoom.CustomProperties["RealTotalPlayer"] != null){
        if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("RoomPrivate") && (int)PhotonNetwork.CurrentRoom.CustomProperties["RoomPrivate"] == 0){ // only if public
            if(!gameEnded && currentStartGameCountdown <= 8 && !doneSpawnBots){
                // Fill Bots
                if(fillWithBots){
                    if(PhotonNetwork.IsMasterClient){
                        //StartCoroutine(SpawnBots());
                        photonView.RPC("SpawnBots", RpcTarget.AllBuffered);
                    }
                }
            }
        }
    }

    
#region START GAME RELATED
    public void SpawnCharacterTeam(string team){ // Called by Join Robber / Police Button
        GameObject player = null;
        UIManager.instance.manualRoleSelect.StopCoroutine(UIManager.instance.manualRoleSelect.randomChooseCoroutine); // Stop Choose Random Coroutine
        switch(team){
            case "Police":
                player = PhotonNetwork.Instantiate(PhotonNetworkManager.GetPhotonPrefab("Characters", "CharacterPolice"), waitingSpawnpoint.position + new Vector3(Random.Range(0,3f), Random.Range(0,3f), 0f), Quaternion.identity);
                player.GetPhotonView().Owner.NickName = PlayerPrefs.GetString("Username");
                player.GetComponent<PlayerController>().characterCode = "P01"; // Spawn default police
                player.GetComponent<PlayerController>().photonView.RPC("CreateAvatar", RpcTarget.AllBuffered);
            break;

            case "Robber":
                player = PhotonNetwork.Instantiate(PhotonNetworkManager.GetPhotonPrefab("Characters", "CharacterRobber"), waitingSpawnpoint.position + new Vector3(Random.Range(0,3f), Random.Range(0,3f), 0f), Quaternion.identity);
                player.GetPhotonView().Owner.NickName = PlayerPrefs.GetString("Username");
                player.GetComponent<PlayerController>().characterCode = "R01"; // Spawn default robber
                player.GetComponent<PlayerController>().photonView.RPC("CreateAvatar", RpcTarget.AllBuffered);
            break;

            default:
                print("Unknown Team");
            break;
        } // end switch team

        UIManager.instance.cacheManualRoleSelect.SetActive(false); // Close Role Select

        UIManager.instance.cacheCharacterSelect.SetActive(true); // Active Character Select Screen
        UIManager.instance.p_CharacterSelect.InitializeAllCharacters(team); // Initialize all available characters
    } // end SpawnSelectedCharacter

    [PunRPC]
    public IEnumerator SpawnBots(){
        doneSpawnBots = true;
        yield return new WaitForSeconds(.5f);
        // Fill in police 1st
        if(GameManager.GetAllPlayersPolice().Count < (int)PhotonNetwork.CurrentRoom.CustomProperties["RoomPolicePerGame"]){
            int polDif = (int)PhotonNetwork.CurrentRoom.CustomProperties["RoomPolicePerGame"] - GameManager.GetAllPlayersPolice().Count;

            if(polDif > 0 && polDif <= 4){
                switch(polDif){
                    case 0:
                        spawnBotPoliceCount = 0;
                    break;

                    case 1:
                        spawnBotPoliceCount = 1;
                    break;

                    case 2:
                        spawnBotPoliceCount = 1;
                    break;

                    case 3:
                        spawnBotPoliceCount = 1;
                    break;

                    case 4:
                        spawnBotPoliceCount = 2;
                    break;

                    default:
                        spawnBotPoliceCount = 2;
                    break;
                }

                for(int i = 0; i < spawnBotPoliceCount; i++){
                    yield return new WaitForSeconds(Random.Range(0f, 1f));
                    GameObject player = PhotonNetwork.InstantiateRoomObject(PhotonNetworkManager.GetPhotonPrefab("Characters", "AIPolice"), waitingSpawnpoint.position + new Vector3(Random.Range(0,3f), Random.Range(0,3f), 0f), Quaternion.identity);
                    
                    if(policeNames.Count > 0){
                        var ran = Random.Range(0, policeNames.Count);
                        player.GetPhotonView().Owner.NickName = policeNames[ran];
                    }else{
                        player.GetComponent<AIPolice>().playerNameText.text = "Police" + photonView.OwnerActorNr;
                    }
                }
            }
            
            
        }

        // Fill in robber 1st
        if(GameManager.GetAllPlayersRobber().Count < (int)PhotonNetwork.CurrentRoom.CustomProperties["RoomRobberPerGame"]){
            int robDif = (int)PhotonNetwork.CurrentRoom.CustomProperties["RoomRobberPerGame"] - GameManager.GetAllPlayersRobber().Count;

            if(robDif > 0 && robDif <= 6){
                switch(robDif){
                    case 0:
                        spawnBotRobberCount = 0;
                    break;

                    case 1:
                        spawnBotRobberCount = 1;
                    break;

                    case 2:
                        spawnBotRobberCount = 1;
                    break;

                    case 3:
                        spawnBotRobberCount = 1;
                    break;

                    case 4:
                        spawnBotRobberCount = 1;
                    break;

                    case 5:
                        spawnBotRobberCount = 2;
                    break;

                    case 6:
                        spawnBotRobberCount = 2;
                    break;

                    default:
                        spawnBotRobberCount = 2;
                    break;
                }

                for(int i = 0; i < spawnBotRobberCount; i++){
                    yield return new WaitForSeconds(Random.Range(0f, 1f));
                    GameObject player = PhotonNetwork.InstantiateRoomObject(PhotonNetworkManager.GetPhotonPrefab("Characters", "AIRobber"), waitingSpawnpoint.position + new Vector3(Random.Range(0,3f), Random.Range(0,3f), 0f), Quaternion.identity);
                    
                    if(robberNames.Count > 0){
                        var ran = Random.Range(0, robberNames.Count);
                        player.GetPhotonView().Owner.NickName = robberNames[ran];
                    }else{
                        player.GetComponent<AIRobber>().playerNameText.text = "Robber" + photonView.OwnerActorNr;
                    }
                }
            } 
        }
        
    } // end SPawnBots

    public void ChooseRandom(){ // Choose Random Between Robber & Police
      // Ask GameManager to randomly choose

      int randomTeam = (Random.Range(0, 100)%2); // 0 = Police | 1 = Robber

      if(randomTeam == 0){ // Join Police
         if(currentIngamePolice < (int)PhotonNetwork.CurrentRoom.CustomProperties["RoomPolicePerGame"]){
            SpawnCharacterTeam("Police");
         }else{
            SpawnCharacterTeam("Robber");
         }
      }else{
         if(currentIngameRobber < (int)PhotonNetwork.CurrentRoom.CustomProperties["RoomRobberPerGame"]){
            SpawnCharacterTeam("Robber");
         }else{
            SpawnCharacterTeam("Police");
         }
      }
   }

    IEnumerator StartGameCountdown(){ // Start countdown before transferring all players to their positions
        while(currentStartGameCountdown != 0){
            if(currentStartGameCountdown > 0 && currentStartGameCountdown <= 3){
                AudioManager.instance.PlaySound("PS_UI_Countdown");

                if(UIManager.instance.gameUI.lobbyLeaveGame.interactable){
                    UIManager.instance.gameUI.lobbyLeaveGame.interactable = false;
                }
            }
            UIManager.instance.gameUI.redirectCountdownText.text = ""+currentStartGameCountdown;
            yield return new WaitForSeconds(1f);
            currentStartGameCountdown -= 1;

            if(currentStartGameCountdown <= 0){
                // Play Sound
                AudioManager.instance.PlaySound("PS_UI_StartGame");

                // Stop Music
                AudioManager.instance.Invoke("StopMusic", .3f);

                // Play Music
                Invoke("delayMusicStart", 1f);

                UIManager.instance.gameUI.redirectCountdownText.text = "START!";
                
                var policePos = 0;
                var robberPos = 0;
                foreach(var g in GetAllPlayers()){
                    if(g.CompareTag("Police")){
                        g.transform.position = policeSpawnpoints[policePos].position;
                        if(g.GetComponent<Police>().isBot){
                            //g.GetComponent<AIPolice>().InitBot();
                            g.GetComponent<AIPolice>().Invoke("InitBot",4f);
                        }else{
                            g.GetComponent<PlayerController>().DisplayTutorial("Police");
                            StartCoroutine(g.GetComponent<PlayerController>().PauseMovement(4f));
                        }
                        policePos++;
                    }else{ // else robber
                        g.transform.position = robberSpawnpoints[robberPos].position;
                        if(g.GetComponent<Robber>().isBot){
                            //g.GetComponent<AIRobber>().InitBot();
                            g.GetComponent<AIRobber>().Invoke("InitBot",4f);
                        }else{
                            g.GetComponent<PlayerController>().DisplayTutorial("Robber");
                            StartCoroutine(g.GetComponent<PlayerController>().PauseMovement(4f));
                        }
                        
                        g.GetComponent<Robber>().photonView.RPC("InvulnerableEffect", RpcTarget.All, 4f);
                        robberPos++;
                    }
                } // end foreach

                // Close Select Character UI
                StartCoroutine(UIManager.instance.CloseCharacterSelect(0));

                // Close Lobby Button Group
                UIManager.instance.gameUI.lobbyButtonGroup.SetActive(false);

                // CLose Room Code GO
                UIManager.instance.gameUI.roomInfoGO.SetActive(false);

                // Close Lobby Button Group
                UIManager.instance.gameUI.moneybagTimerGroup.SetActive(true);

                yield return new WaitForSeconds(2f); // wait for seconds to clear text
                UIManager.instance.gameUI.redirectCountdownText.text = "";

                CheckWinningCondition();

                //print("Redirect Everybody To Their Position");
                yield return new WaitForSeconds(1f); // delay gameStart = true
                gameStarted = true;
                
                if(PhotonNetwork.IsMasterClient){ 
                    InvokeRepeating("ChosenBotToRescue", 0f, 15f);

                    PhotonNetwork.CurrentRoom.IsOpen = false;
                }
            } // end if(currentStartGameCountdown <= 0)
        }
        
    } // end StartGameCountdown

    public void UpdateTeamCount(){
        int policeCount = 0;
        int robberCount = 0;
        foreach (GameObject p in GetAllPlayers())
        {
            if(p.GetComponent<PlayerController>() != null && p.GetComponent<PlayerController>().playerTeam == E_Team.POLICE){
                policeCount++;
            }else if(p.GetComponent<PlayerController>() != null && p.GetComponent<PlayerController>().playerTeam == E_Team.ROBBER){
                robberCount++;
            }
        }

        currentIngamePolice = policeCount;
        currentIngameRobber = robberCount;
        
        // Update Select Role UI
        UIManager.instance.manualRoleSelect.policeCountText.text = policeCount + "/" + PhotonNetwork.CurrentRoom.CustomProperties["RoomPolicePerGame"].ToString();
        UIManager.instance.manualRoleSelect.robberCountText.text = robberCount + "/" + PhotonNetwork.CurrentRoom.CustomProperties["RoomRobberPerGame"].ToString();

        if(policeCount >= (int)PhotonNetwork.CurrentRoom.CustomProperties["RoomPolicePerGame"]){
            UIManager.instance.manualRoleSelect.joinPoliceBtn.interactable = false;
        }else{
            UIManager.instance.manualRoleSelect.joinPoliceBtn.interactable = true;
        }

        if(robberCount >= (int)PhotonNetwork.CurrentRoom.CustomProperties["RoomRobberPerGame"]){
            UIManager.instance.manualRoleSelect.joinRobberBtn.interactable = false;
        }else{
            UIManager.instance.manualRoleSelect.joinRobberBtn.interactable = true;
        }

    } // end UpdateTeamCount()

    IEnumerator DeductMoneybagTimer(){ // PLAYER HOLD MONEYBAG ENDS
        while(currentTimer > timerEnd && !gameEnded){
            int minutes = Mathf.FloorToInt(currentTimer / 60f);
            int seconds = Mathf.FloorToInt(currentTimer - minutes * 60f);
            string formattedTimer = string.Format("{0:0}:{1:00}", minutes, seconds);
            UIManager.instance.gameUI.moneyTimerText.text = formattedTimer;

            currentTimer -= 1;
            yield return new WaitForSeconds(1);
        }

        if(currentTimer <= 0){
            PoliceWinning(false);
        }
    }

    [PunRPC]
    public void SpawnAllMoneybag(){
            if(PhotonNetwork.IsMasterClient){
                moneybagList.Clear();
                //print("SpawnAllMoney");
                foreach(var mb in moneybagSpawnpoints){
                    //var moneyBag = PhotonNetwork.InstantiateRoomObject(NetworkManager.GetPhotonPrefab("Props", "prop_moneybag01"), mb.position, Quaternion.identity);
                    var moneyBag = PhotonNetwork.InstantiateRoomObject(PhotonNetworkManager.GetPhotonPrefab("Props", "prop_moneybag01"), mb.position, Quaternion.identity);
                    moneybagList.Add(moneyBag);
                }
                moneybagSpawned = true;
            }
    } // end SpawnAllMoneybag()

    public void DestroyAllMoneybag(){
            if(moneybagList.Count > 0){
                foreach(var mb in moneybagList){
                    if(mb != null){
                        PhotonNetwork.Destroy(mb);
                    }
                }
            }
            moneybagSpawned = false;
    } // end DestroyAllMoneybag()

#endregion //end START GAME RELATED

#region MID GAME RELATED
    [PunRPC]
    public void UpdateAvatarsUI(){
        foreach(var btn in UIManager.instance.gameUI.avatarBtnList){
            /* foreach(Player player in GameManager.GetAllNetworkPlayers()){
                if(btn.actorNumber == player.ActorNumber){
                    btn.UpdateButton(player.CustomProperties["NetworkTeam"].ToString(), player.CustomProperties["CharacterCode"].ToString(), (bool)player.CustomProperties["PlayerCaught"], (bool)player.CustomProperties["PlayerHoldMoneybag"]);
                }
            } */

            foreach(GameObject player in GameManager.GetAllPlayers()){
                if(btn.ownerOfThisAvatarGO == player){
                    if(btn.ownerOfThisAvatarGO.CompareTag("Robber") && btn.ownerOfThisAvatarGO.GetComponent<Robber>().isBot){
                        btn.UpdateButton(btn.ownerOfThisAvatarGO.tag, "R01", btn.ownerOfThisAvatarGO.GetComponent<Robber>().isCaught, btn.ownerOfThisAvatarGO.GetComponent<Robber>().isHoldMoneybag);
                    }else if(btn.ownerOfThisAvatarGO.CompareTag("Police") && btn.ownerOfThisAvatarGO.GetComponent<Police>().isBot){ // else police
                        btn.UpdateButton(btn.ownerOfThisAvatarGO.tag, "P01", false, false);
                    }else{
                        if(btn.ownerOfThisAvatarGO.CompareTag("Robber") || btn.ownerOfThisAvatarGO.CompareTag("Police")){
                            if(btn.ownerOfThisAvatarGO.GetPhotonView().Owner.CustomProperties["CharacterCode"] != null && btn.ownerOfThisAvatarGO.GetPhotonView().Owner.CustomProperties["PlayerCaught"] != null && btn.ownerOfThisAvatarGO.GetPhotonView().Owner.CustomProperties["PlayerHoldMoneybag"] != null){
                                btn.UpdateButton(btn.ownerOfThisAvatarGO.tag, btn.ownerOfThisAvatarGO.GetPhotonView().Owner.CustomProperties["CharacterCode"].ToString(), (bool)btn.ownerOfThisAvatarGO.GetPhotonView().Owner.CustomProperties["PlayerCaught"], (bool)btn.ownerOfThisAvatarGO.GetPhotonView().Owner.CustomProperties["PlayerHoldMoneybag"]);
                            }else{
                                btn.UpdateButton(btn.ownerOfThisAvatarGO.tag, btn.ownerOfThisAvatarGO.GetComponent<PlayerController>().characterCode, false, false);
                            }
                            
                        }
                    }
                }
            }
        }

        // Check if not moneybag
        if(!moneyBagOccupied && PhotonNetwork.IsMasterClient && gameStarted && !gameEnded && !moneybagSpawned){
            photonView.RPC("SpawnAllMoneybag", RpcTarget.All);
        }
    } // end UpdateAvatarsUI()

    [PunRPC]
    public void CheckWinningCondition(){
        if(!gameEnded){
            UpdateAvatarsUI();
            // if total police ingame <= 0, ROBBER WINS
            if(GetAllPlayersPolice().Count <= 0){
                PoliceWinning(false);
            }
            // if total robber ingame <= 0, POLICE WINS
            if(GetAllPlayersRobber().Count <= 0){
                PoliceWinning(true);
            }

            // if robbers caught = total robber in game, POLICE WIN
            if(NumberOfCaughtRobber() >= GetAllPlayersRobber().Count){
                PoliceWinning(true);
            }
        } // if !gameEnded
    } // end CheckWinningCondition()

    public void PoliceWinning(bool policeWin){ // TRUE = Police WIN, FALSE = Robber WIN
        gameEnded = true;

        if(!donePopupWinUI){
            if(policeWin){
                if(ownedPlayerGO != null && ownedPlayerGO.GetComponent<PlayerController>().playerTeam == E_Team.POLICE){
                    StartCoroutine(UIManager.instance.PopupWinUI("Police"));
                }else if(ownedPlayerGO != null && ownedPlayerGO.GetComponent<PlayerController>().playerTeam == E_Team.ROBBER){
                    StartCoroutine(UIManager.instance.PopupLoseUI("Robber"));
                }

                UIManager.instance.gameUI.moneyTimerText.text = "<color=blue>Police Win!</color>";
            }else{
                if(ownedPlayerGO != null && ownedPlayerGO.GetComponent<PlayerController>().playerTeam == E_Team.POLICE){
                    StartCoroutine(UIManager.instance.PopupLoseUI("Police"));
                }else if(ownedPlayerGO != null && ownedPlayerGO.GetComponent<PlayerController>().playerTeam == E_Team.ROBBER){
                    StartCoroutine(UIManager.instance.PopupWinUI("Robber"));
                }

                UIManager.instance.gameUI.moneyTimerText.text = "<color=red>Robber Win!</color>";
            }
        } // end !donePopupWinUI

        donePopupWinUI = true;
    } // end PoliceWinning()

#endregion // end MID GAME RELATED

#region END GAME RELATED
    public void PopupEndScreen(){
        endScreenGO.SetActive(true);

        if(photonView.IsMine){
            AudioManager.instance.PlaySound("PS_UI_Endscreen_v1");
        }

        StartCoroutine(RedirectNewMap(5));
        AudioManager.instance.StopMusic();
        
        if(PhotonNetwork.IsMessageQueueRunning){
            PhotonNetwork.IsMessageQueueRunning = false;
        }
        
    } // end PopupEndScreen

    IEnumerator RedirectNewMap(float duration){
        float timer = duration;
        while(timer > 0){
            endScreenRedirectText.text = "Leave game in " + timer;
            /*if(PhotonNetwork.CurrentRoom.PlayerCount > 1){
                endScreenRedirectText.text = "Redirect to new map in " + timer;
            }else{
                endScreenRedirectText.text = "Leave game in " + timer;
            }*/
            AudioManager.instance.PlaySound("Placeholder_RedirectCountdown");
            
            timer -= 1;
            yield return new WaitForSeconds(1);
        }

        if(timer <= 0){
            timer = 0;
            PhotonNetwork.AutomaticallySyncScene = true;
            
            endScreenRedirectText.text = "Bye-bye";
            StartCoroutine(PhotonNetworkManager.instance.InGameLeaveRoom());
            /*if(PhotonNetwork.IsMasterClient){
                if(PhotonNetwork.CurrentRoom.PlayerCount > 1){
                    endScreenRedirectText.text = "Loading...";
                    photonView.RPC("Boom", RpcTarget.All);
                }else{
                    endScreenRedirectText.text = "Bye-bye";
                    StartCoroutine(NetworkManager.instance.InGameLeaveRoom());
                }
            }else{
                if(PhotonNetwork.CurrentRoom.PlayerCount > 1){
                    endScreenRedirectText.text = "Waiting for host...";
                }else{
                    endScreenRedirectText.text = "Bye-bye";
                    StartCoroutine(NetworkManager.instance.InGameLeaveRoom());
                }
            }*/
        }
    } // end redirect

    [PunRPC]
    public void Boom(){
        StartCoroutine(PhotonNetworkManager.instance.ChangeScene(PhotonNetworkManager.instance.GetRandomMap()));// Host load level
    }
    /*
    public void AskHostToPlayAgain(){
        if(!PhotonNetwork.IsMasterClient)
        photonView.RPC("AddPlayAgainList", RpcTarget.MasterClient);
    }

    [PunRPC]
    public void AddPlayAgainList(){
        playAgainCount += 1;
    }*/
#endregion // end END GAME RELATED

#region RARELY UNTOUCHED FUNCTIONS
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps){
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        UpdateTeamCount(); // Early game check for team count
        if(GetAllPlayers().Count > 0){
            UpdateAvatarsUI(); // Update avatars UI
        }
        
        if(gameStarted && !gameEnded){
            CheckWinningCondition();
        }
    } // end OnPlayerPropertiesUpdate

    public static List<GameObject> GetAllPlayers(){ // Return list of players GAMEOBJECT
        List<GameObject> allPlayers = new List<GameObject>();
        List<GameObject> polices = new List<GameObject>(GameObject.FindGameObjectsWithTag("Police"));
        List<GameObject> robbers = new List<GameObject>(GameObject.FindGameObjectsWithTag("Robber"));
        foreach(var p in polices){
            allPlayers.Add(p);
        }

        foreach(var r in robbers){
            allPlayers.Add(r);
        }

        return allPlayers;
    } // end GetAllPlayers
    
    public static List<GameObject> GetAllPlayersRobber(){ // Return list of players GAMEOBJECT
        List<GameObject> robbers = new List<GameObject>(GameObject.FindGameObjectsWithTag("Robber"));

        return robbers;
    } // end GetAllPlayersRobber

    public static List<GameObject> GetAllPlayersPolice(){ // Return list of players GAMEOBJECT
        List<GameObject> polices = new List<GameObject>(GameObject.FindGameObjectsWithTag("Police"));

        return polices;
    } // end GetAllPlayersRobber

    public static List<GameObject> GetAllMoneybag(){ // Return list of all moneybag
        List<GameObject> moneybag = new List<GameObject>(GameObject.FindGameObjectsWithTag("MoneyBag"));

        return moneybag;
    } // end GetAllPlayersRobber

    public void ChosenBotToRescue(){ // Select 1 bot who is !caught to rescue teammate
        //print("Ask for teammate help!");
        if(GameManager.instance.gameStarted && !GameManager.instance.gameEnded){
            List<GameObject> botRobber = new List<GameObject>();
            botRobber.Clear();

            // Filter all robber who is bot, then added to botRobber list
            if(GetAllPlayersRobber().Count > 0){
                foreach(var r in GetAllPlayersRobber()){
                    if(r.GetComponent<Robber>().isBot){
                        botRobber.Add(r);
                    }
                }
            }

            if(botRobber.Count > 0){
                var randomIndex = Random.Range(0, botRobber.Count);
                if(!botRobber[randomIndex].GetComponent<Robber>().isCaught && !botRobber[randomIndex].GetComponent<AIRobber>().isRescuing && NumberOfCaughtRobber() > 0){
                    botRobber[randomIndex].GetComponent<AIRobber>().RescueTeammate();
                }
            }
        }
        
    } // end GetAllPlayersRobber

    public static Sprite GetCharacterIconHead(string team, string code){ // Return character icon head sprite
       switch (team)
       {
            case "Police":
                foreach (var icon in SOManager.instance.animVariantPolice.animatorLists)
                {
                    if (icon.code == code)
                    {
                        return icon.iconHead;
                    }
                }
            break;

            case "Robber":
                foreach (var icon in SOManager.instance.animVariantRobber.animatorLists)
                {
                    if (icon.code == code)
                    {
                        return icon.iconHead;
                    }
                }
            break;

            default:
            print ("Wrong Team Name");
            break;
       }

       return null;

    } // end GetCharacterIconHead

    public static List<Player> GetAllNetworkPlayers(){ // Return list of NETWORK players
        List<Player> allPlayers = new List<Player>();
        List<Player> polices = new List<Player>();
        List<Player> robbers = new List<Player>();
        foreach(Player p in PhotonNetwork.PlayerList){
            if(p.CustomProperties["NetworkTeam"] != null){
                if(p.CustomProperties["NetworkTeam"].ToString() == "Police"){
                    polices.Add(p);
                }else if(p.CustomProperties["NetworkTeam"].ToString() == "Robber"){
                    robbers.Add(p);
                }
            }
        }

        foreach(Player police in polices){
            allPlayers.Add(police);
        }

        foreach(Player robber in robbers){
            allPlayers.Add(robber);
        }

        return allPlayers;
    } // end GetAllNetworkPlayers

    public static List<Player> GetAllNetworkRobbers(){ // Return list of NETWORK players (Robbers)
        List<Player> robbers = new List<Player>();
        foreach(Player p in PhotonNetwork.PlayerList){
            if(p.CustomProperties["NetworkTeam"] != null){
                if(p.CustomProperties["NetworkTeam"].ToString() == "Robber"){
                    robbers.Add(p);
                }
            }
        }

        return robbers;
    } // end GetAllNetworkRobbers
    

    public int NumberOfCaughtRobber(){
        int caughtCount = 0;
        
        foreach(GameObject p in GetAllPlayersRobber()){
            if(p.GetComponent<Robber>().isCaught){
                caughtCount += 1;
            }
        } // end foreach
        
        return caughtCount;
    }

    [PunRPC]
    public void SetMoneybagOccupied(bool isOccupied){
        if(isOccupied){
            moneyBagOccupied = true;
        }else{
            moneyBagOccupied = false;
        }
    }

#endregion // end RARELY UNTOUCHED FUNCTIONS

}
