// Handle UI Popup Related (Mostly modal)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject mainCanvas; // Ref for mainCanvas
    [HideInInspector] public GameObject cacheLoadingDuration, cacheLoadingScene, cacheReconnectGame, cacheCharacterSelect, cacheManualRoleSelect;
    public P_MainMenu p_MainMenu;
    public P_CharacterSelect p_CharacterSelect;
    public ManualRoleSelect manualRoleSelect;
    public Modal_ReconnectGame modalReconnectGame;
    public GameUI gameUI;
    public float timeoutTimer = 0;
    
    void Awake()
    {
        if(instance == null){
            instance = this;
        }else{
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(gameObject);

        if(mainCanvas == null){ // Find maincanvas on load if mainCanvas is empty.
            mainCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
        }
    }

    void Update(){
        RefreshMainCanvas();
    }

    public void RefreshMainCanvas(){
        if(mainCanvas == null){ // Always check if mainCanvas is missing. 
            mainCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
        }
    }

    public void RefreshControllerGroup(){
        if(gameUI == null){ // Always check if mainCanvas is missing. 
            gameUI = GameObject.FindGameObjectWithTag("GameUI").GetComponent<GameUI>();
        }
    }

/* ------------------------------------------------ SCENE LOADING START ---------------------------------------------------------*/
#region SCENE LOADING
    public void PopupLoadingScene(){ // Popup loading scene
        if(cacheLoadingScene == null){
            var loading = Instantiate(SOManager.instance.prefabs.modalLoadingScene);
            loading.transform.SetParent(mainCanvas.transform, false);
            cacheLoadingScene = loading;
        }else{
            // if we have cache the loading UI, resuse it by SetActive
            cacheLoadingScene.SetActive(true);
        }
    } // end PopupLoadingScene

    public IEnumerator CloseLoadingScene(float duration){ // Close the loading UI with certain duration
        if(cacheLoadingScene != null){
            yield return new WaitForSeconds(duration);
            cacheLoadingScene.SetActive(false);
        }
    } // end CloseLoadingScene

#endregion
/* ------------------------------------------------ SCENE LOADING END ---------------------------------------------------------*/

/* ------------------------------------------------  FIND GAME RELATED START ---------------------------------------------------------*/
#region FIND GAME RELATED

    public void PopupFindGame(){
        if(p_MainMenu == null){
            p_MainMenu.findGameGO.SetActive(true);
        }else{
            p_MainMenu.findGameGO.SetActive(true); // if we have cache the UI, resuse instead destroy
            p_MainMenu.timeoutDurationText.text = "0:00"; // Reset find game timeout timer
        }
    }

    public IEnumerator CloseFindGame(float duration){
        if(p_MainMenu != null){
            yield return new WaitForSeconds(duration);
            NetworkManager.instance.CancelFindGameOrLeaveRoom(); // tell networkmanager to close the finding
            p_MainMenu.findGameGO.SetActive(false);
        }
    }

    public void activeFindgameCancel(bool isOn){
        if(isOn){
            p_MainMenu.cancelFindGameBtn.interactable = true;
        }else{
            p_MainMenu.cancelFindGameBtn.interactable = false;
        }
    }

    public IEnumerator UpdateUI_FindgameTimeout(float duration){ // Called by NetworkManager. For timeout countdown display when find game
        while(timeoutTimer <= duration && !NetworkManager.instance.isInGame)
        {
            timeoutTimer += Time.deltaTime;

            int minutes = Mathf.FloorToInt(timeoutTimer / 60f);
            int seconds = Mathf.FloorToInt(timeoutTimer - minutes * 60f);
            string formattedTimer = string.Format("{0:0}:{1:00}", minutes, seconds);

            if(p_MainMenu != null){
                p_MainMenu.timeoutDurationText.text = formattedTimer;
            }

            //NetworkManager.instance.UpdateTotalFindGame();

            yield return null;
        }

        if(timeoutTimer >= duration && !NetworkManager.instance.isInGame){
            if(p_MainMenu != null){
                p_MainMenu.timeoutDurationText.text = "TIMEOUT!";
                NetworkManager.instance.CancelFindGameOrLeaveRoom();
            }
            StartCoroutine(CloseFindGame(.3f)); // Close find game UI 
        }
    } // end UpdateUI_FindgameTimeout

#endregion
/* ------------------------------------------------  FIND GAME RELATED END ---------------------------------------------------------*/

/* ------------------------------------------------  INGAME RECONNECT RELATED START ---------------------------------------------------------*/
#region INGAME RECONNECT RELATED
    public void PopupReconnectGame(){
        if(cacheReconnectGame == null){
            var reconnect = Instantiate(SOManager.instance.prefabs.modalReconnectGame);
            reconnect.transform.SetParent(mainCanvas.transform, false);
            cacheReconnectGame = reconnect;
            modalReconnectGame = reconnect.GetComponent<Modal_ReconnectGame>();
        }else{
            cacheReconnectGame.SetActive(true); // if we have cache the UI, resuse instead destroy
        }
    }

    public IEnumerator CloseReconnectGame(float duration){
        if(cacheReconnectGame != null){
            yield return new WaitForSeconds(duration);
            cacheReconnectGame.SetActive(false);
        }
    }

#endregion
/* ------------------------------------------------  INGAME RECONNECT RELATED END ---------------------------------------------------------*/


/* ------------------------------------------------  INGAME RELATED START ----------------------------------------------- */
    public void PopupManualSelectRole(){
        if(cacheManualRoleSelect == null){
            var manualRole = Instantiate(SOManager.instance.prefabs.manualRoleSelect);
            manualRole.transform.SetParent(mainCanvas.transform, false);
            cacheManualRoleSelect = manualRole;
            manualRoleSelect = manualRole.GetComponent<ManualRoleSelect>();
        }else{
            cacheManualRoleSelect.SetActive(true); // if we have cache the UI, resuse instead destroy
        }
    }

    public IEnumerator CloseManualSelectRole(float duration){
        if(cacheManualRoleSelect != null){
            yield return new WaitForSeconds(duration);
            cacheManualRoleSelect.SetActive(false);
        }
    }

    public void PopupCharacterSelect(){
        if(cacheCharacterSelect == null){
            var characterSelect = Instantiate(SOManager.instance.prefabs.p_CharacterSelect);
            characterSelect.transform.SetParent(mainCanvas.transform, false);
            cacheCharacterSelect = characterSelect;
            p_CharacterSelect = characterSelect.GetComponent<P_CharacterSelect>();
        }else{
            cacheCharacterSelect.SetActive(true); // if we have cache the UI, resuse instead destroy
        }
    }

    public IEnumerator CloseCharacterSelect(float duration){
        if(cacheCharacterSelect != null){
            yield return new WaitForSeconds(duration);
            cacheCharacterSelect.SetActive(false);
        }
    }

    public IEnumerator PopupWinUI(string team){
        yield return new WaitForSeconds(2.5f);
        
        GameObject popupUI = null;
        if(team == "Police"){
            popupUI = Instantiate(SOManager.instance.prefabs.p_PoliceWinUI);
        }else{
            popupUI = Instantiate(SOManager.instance.prefabs.p_RobberWinUI);
        }
        popupUI.transform.SetParent(mainCanvas.transform, false);
        yield return new WaitForSeconds(2f);
        popupUI.SetActive(false);
        yield return new WaitForSeconds(1f);
        GameManager.instance.PopupEndScreen();
    }

    public IEnumerator PopupLoseUI(string team){
        yield return new WaitForSeconds(2.5f);
        
        GameObject popupUI = null;
        if(team == "Police"){
            popupUI = Instantiate(SOManager.instance.prefabs.p_PoliceLoseUI);
        }else{
            popupUI = Instantiate(SOManager.instance.prefabs.p_RobberLoseUI);
        }
        popupUI.transform.SetParent(mainCanvas.transform, false);
        yield return new WaitForSeconds(2f);
        popupUI.SetActive(false);
        yield return new WaitForSeconds(1f);
        GameManager.instance.PopupEndScreen();
    }

    public void Notification(string textValue){
        var noti = Instantiate(SOManager.instance.prefabs.ui_GameplayFeeds);
        noti.GetComponent<UI_GameplayFeeds>().SetText(textValue);
        noti.transform.SetParent(UIManager.instance.gameUI.feedsGroupContent.transform, false);
    }

    public void NotificationPoliceCapture(string policeName, string robberName){
        var noti = Instantiate(SOManager.instance.prefabs.ui_GameplayFeeds);
        noti.GetComponent<UI_GameplayFeeds>().SetTextPoliceCapture(policeName, robberName);
        noti.transform.SetParent(UIManager.instance.gameUI.feedsGroupContent.transform, false);

        GameManager.instance.UpdateAvatarsUI();
    }

    public void NotificationPickupMoneybag(string robberName){
        var noti = Instantiate(SOManager.instance.prefabs.ui_GameplayFeeds);
        noti.GetComponent<UI_GameplayFeeds>().SetTextPickupMoneybag(robberName);
        noti.transform.SetParent(UIManager.instance.gameUI.feedsGroupContent.transform, false);

        GameManager.instance.UpdateAvatarsUI();
    }

    public void NotificationReleasedBy(string robberName, string teammateName){
        var noti = Instantiate(SOManager.instance.prefabs.ui_GameplayFeeds);
        noti.GetComponent<UI_GameplayFeeds>().SetTextReleasedBy(robberName, teammateName);
        noti.transform.SetParent(UIManager.instance.gameUI.feedsGroupContent.transform, false);

        GameManager.instance.UpdateAvatarsUI();
    }

    public void NotificationReleasedByLockpick(string robberName){
        var noti = Instantiate(SOManager.instance.prefabs.ui_GameplayFeeds);
        noti.GetComponent<UI_GameplayFeeds>().SetTextReleasedByLockpick(robberName);
        noti.transform.SetParent(UIManager.instance.gameUI.feedsGroupContent.transform, false);

        GameManager.instance.UpdateAvatarsUI();
    }
/* ------------------------------------------------  INGAME RELATED END ----------------------------------------------- */
}