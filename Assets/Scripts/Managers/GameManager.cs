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
    [HideInInspector] public GameObject botEscapeGO;
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
    [HideInInspector] public GameObject ownedPlayerGO; // The player who are currently owned the game manager (GameObject)
    public int currentIngamePolice, currentIngameRobber;

    [Header("EndGame Related")]
    public bool donePopupWinUI;
    public GameObject endScreenGO;
    public TextMeshProUGUI endScreenRedirectText;
    public bool endGamePlayAgain; // if true, will stay in the game
    public int playAgainCount;
    bool askForHelpActivated;


    void Awake(){
        if(instance == null){
            instance = this;
        }else{
            Destroy(this.gameObject);
        }
    } // end Awake

    void Start(){
        // Bot add escape points
        if(botEscapeGO != null){
            foreach(var child in botEscapeGO.GetComponentsInChildren<Transform>()){
                if(child.CompareTag("BotEscape")){
                    botEscapeSpawnpoints.Add(child);
                }
            }
        }

        UIManager.instance.RefreshMainCanvas(); // Make sure we have main canvas
        UIManager.instance.RefreshControllerGroup(); // Make sure we have reference the CanvasGroup

        UIManager.instance.PopupManualSelectRole(); // Popup Role Select UI
        UIManager.instance.PopupCharacterSelect(); // Popup Character Select
        StartCoroutine(UIManager.instance.CloseCharacterSelect(0)); // Close the char selection UI
        
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

        currentStartGameCountdown = startGameCountdown;
        StartCoroutine(StartGameCountdown());

        NetworkManager.instance.isInGame = true;

        UpdateTeamCount(); // Update Manual Select Role on 1st load

        SpawnAllMoneybag(); // Spawn moneybag

        // Disable PlayAgainButton on Masterclient
        if(PhotonNetwork.IsMasterClient){
            endScreenGO.GetComponent<P_EndScreen>().playAgainBtn.gameObject.SetActive(false);
        }
        
    } // end Start

    

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
        } // end gameStarted
        

        if(Input.GetKeyDown(KeyCode.M)){
            
        }

        if(Input.GetKeyDown(KeyCode.N)){
            
        }
    }

    
#region START GAME RELATED
    public void SpawnCharacterTeam(string team){ // Called by Join Robber / Police Button
        GameObject player = null;
        UIManager.instance.manualRoleSelect.StopCoroutine(UIManager.instance.manualRoleSelect.randomChooseCoroutine); // Stop Choose Random Coroutine
        switch(team){
            case "Police":
                player = PhotonNetwork.Instantiate(NetworkManager.GetPhotonPrefab("Characters", "CharacterPolice"), GameManager.instance.waitingSpawnpoint.position + new Vector3(Random.Range(0,3f), Random.Range(0,3f), 0f), Quaternion.identity);
                player.GetPhotonView().Owner.NickName = PlayerPrefs.GetString("Username");
                player.GetComponent<PlayerController>().characterCode = "P01"; // Spawn default police
                player.GetComponent<PlayerController>().photonView.RPC("CreateAvatar", RpcTarget.AllBuffered);
            break;

            case "Robber":
                player = PhotonNetwork.Instantiate(NetworkManager.GetPhotonPrefab("Characters", "CharacterRobber"), GameManager.instance.waitingSpawnpoint.position + new Vector3(Random.Range(0,3f), Random.Range(0,3f), 0f), Quaternion.identity);
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

        if(fillWithBots && PhotonNetwork.OfflineMode && PhotonNetwork.InRoom){
            if(PhotonNetwork.IsMasterClient){
                StartCoroutine(SpawnBots());
            }
        }
    } // end SpawnSelectedCharacter

    public IEnumerator SpawnBots(){
        yield return new WaitForSeconds(2);
        // Fill in police 1st
        if(GameManager.GetAllPlayersPolice().Count < (int)PhotonNetwork.CurrentRoom.CustomProperties["RoomPolicePerGame"]){
            int polDif = (int)PhotonNetwork.CurrentRoom.CustomProperties["RoomPolicePerGame"] - GameManager.GetAllPlayersPolice().Count;
            for(int i = 0; i < polDif; i++){
                yield return new WaitForSeconds(Random.Range(0f, 1f));
                GameObject player = PhotonNetwork.InstantiateRoomObject(NetworkManager.GetPhotonPrefab("Characters", "BotPolice"), GameManager.instance.waitingSpawnpoint.position + new Vector3(Random.Range(0,3f), Random.Range(0,3f), 0f), Quaternion.identity);
                player.GetPhotonView().Owner.NickName = "BOT POLICE";
                player.GetComponent<BotController>().enabled = true;

                var randomPoliceSkin = Random.Range(0, SOManager.instance.animVariantPolice.animatorLists.Count);
                player.GetComponent<PlayerController>().characterCode = SOManager.instance.animVariantPolice.animatorLists[randomPoliceSkin].code;

                player.GetComponent<PlayerController>().CreateAvatar();
                player.GetComponent<PlayerController>().SetupPlayerAnimator();
            }
            
        }

        // Fill in robber 1st
        if(GameManager.GetAllPlayersRobber().Count < (int)PhotonNetwork.CurrentRoom.CustomProperties["RoomRobberPerGame"]){
            int robDif = (int)PhotonNetwork.CurrentRoom.CustomProperties["RoomRobberPerGame"] - GameManager.GetAllPlayersRobber().Count;
            for(int i = 0; i < robDif; i++){
                yield return new WaitForSeconds(Random.Range(0f, 1f));
                GameObject player = PhotonNetwork.InstantiateRoomObject(NetworkManager.GetPhotonPrefab("Characters", "BotRobber"), GameManager.instance.waitingSpawnpoint.position + new Vector3(Random.Range(0,3f), Random.Range(0,3f), 0f), Quaternion.identity);
                player.GetPhotonView().Owner.NickName = "BOT ROBBER";
                player.GetComponent<BotController>().enabled = true;
                
                var randomRobberSkin = Random.Range(0, SOManager.instance.animVariantRobber.animatorLists.Count);
                player.GetComponent<PlayerController>().characterCode = SOManager.instance.animVariantRobber.animatorLists[randomRobberSkin].code;

                player.GetComponent<PlayerController>().CreateAvatar();
                player.GetComponent<PlayerController>().SetupPlayerAnimator();
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
            UIManager.instance.gameUI.redirectCountdownText.text = ""+currentStartGameCountdown;
            yield return new WaitForSeconds(1f);
            currentStartGameCountdown -= 1;
        }

        if(currentStartGameCountdown <= 0){
            UIManager.instance.gameUI.redirectCountdownText.text = "START!";
            gameStarted = true;

            print("Redirect Everybody To Their Position");
            //StartCoroutine(GameManager.instance.AskForHelp());
            var policePos = 0;
            var robberPos = 0;
            foreach(var g in GetAllPlayers()){
                if(g.GetComponent<PlayerController>().playerTeam == E_Team.POLICE){
                    g.transform.position = policeSpawnpoints[policePos].position;
                    policePos++;
                }else{ // else robber
                    g.transform.position = robberSpawnpoints[robberPos].position;
                    robberPos++;
                }
            } // end foreach

            // Close Select Character UI
            StartCoroutine(UIManager.instance.CloseCharacterSelect(0));

            // Close Lobby Button Group
            UIManager.instance.gameUI.lobbyButtonGroup.SetActive(false);

            // Close Lobby Button Group
            UIManager.instance.gameUI.moneybagTimerGroup.SetActive(true);

            yield return new WaitForSeconds(2f); // wait for seconds to clear text
            UIManager.instance.gameUI.redirectCountdownText.text = "";
        } // end if(currentStartGameCountdown <= 0)
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
                print("SpawnAllMoney");
                foreach(var mb in moneybagSpawnpoints){
                    //var moneyBag = PhotonNetwork.InstantiateRoomObject(NetworkManager.GetPhotonPrefab("Props", "prop_moneybag01"), mb.position, Quaternion.identity);
                    var moneyBag = PhotonNetwork.Instantiate(NetworkManager.GetPhotonPrefab("Props", "prop_moneybag01"), mb.position, Quaternion.identity);
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
    public void UpdateAvatarsUI(){
        foreach(var btn in UIManager.instance.gameUI.avatarBtnList){
            
            foreach(Player player in GetAllNetworkPlayers()){
                if(btn.goID == player.CustomProperties["PlayerViewID"].ToString()){
                    btn.UpdateButton(player.CustomProperties["NetworkTeam"].ToString(), player.CustomProperties["CharacterCode"].ToString(), (bool)player.CustomProperties["PlayerCaught"], (bool)player.CustomProperties["PlayerHoldMoneybag"]);
                }
            }
            
        }
    } // end UpdateAvatarsUI()

    public void CheckWinningCondition(){
        if(!gameEnded){
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
                if(ownedPlayerGO.GetComponent<PlayerController>().playerTeam == E_Team.POLICE){
                    StartCoroutine(UIManager.instance.PopupWinUI("Police"));
                }else if(ownedPlayerGO.GetComponent<PlayerController>().playerTeam == E_Team.ROBBER){
                    StartCoroutine(UIManager.instance.PopupLoseUI("Robber"));
                }

                UIManager.instance.gameUI.moneyTimerText.text = "<color=blue>Police Win!</color>";
            }else{
                if(ownedPlayerGO.GetComponent<PlayerController>().playerTeam == E_Team.POLICE){
                    StartCoroutine(UIManager.instance.PopupLoseUI("Police"));
                }else if(ownedPlayerGO.GetComponent<PlayerController>().playerTeam == E_Team.ROBBER){
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
        StartCoroutine(RedirectNewMap(5));
        // Default Host will create a new game
        if(PhotonNetwork.IsMasterClient){
            endGamePlayAgain = true;
        }
    } // end PopupEndScreen

    IEnumerator RedirectNewMap(float duration){
        float timer = duration;
        while(timer > 0){
            if(!endGamePlayAgain){
                endScreenRedirectText.text = "Leave game in " + timer;
            }else{
                endScreenRedirectText.text = "Next game in " + timer;
            }
            
            timer -= 1;
            yield return new WaitForSeconds(1);
        }

        if(timer <= 0){
            timer = 0;
            
            //NetworkManager.instance.CancelFindGameOrLeaveRoom(); // Leave Room
            if(endGamePlayAgain){
                if(PhotonNetwork.IsMasterClient){
                    endScreenRedirectText.text = "Creating New Game. " + (playAgainCount + 1) + " Players" ;
                    print("Host bring all people to new map");
                    NetworkManager.instance.HostWithCustomPlayer((playAgainCount + 1));
                    photonView.RPC("Boom", RpcTarget.All);
                }else{
                    endScreenRedirectText.text = "Waiting For Host...";
                }
            }else{
                endScreenRedirectText.text = "Bye-bye!";
                PhotonNetwork.LeaveRoom();
            }
        }
    } // end redirect

    [PunRPC]
    public void Boom(){
        StartCoroutine(NetworkManager.instance.ChangeScene(NetworkManager.instance.GetRandomMap()));// Host load level
    }

    public void AskHostToPlayAgain(){
        if(!PhotonNetwork.IsMasterClient)
        photonView.RPC("AddPlayAgainList", RpcTarget.MasterClient);
    }

    [PunRPC]
    public void AddPlayAgainList(){
        playAgainCount += 1;
    }
#endregion // end END GAME RELATED

#region RARELY UNTOUCHED FUNCTIONS
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps){
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        UpdateTeamCount(); // Early game check for team count
        UpdateAvatarsUI(); // Update avatars UI
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

    public void TellBotRobberToRescue(){
        foreach(var bot in GetAllPlayersRobber()){
            if(bot.GetComponent<BotController>() != null && !bot.GetComponent<Robber>().isCaught){
                bot.GetComponent<BotController>().BotRobberSaveTeammate();
                print("TellRobber to rescue");
            }
        }
    }

#endregion // end RARELY UNTOUCHED FUNCTIONS

}
