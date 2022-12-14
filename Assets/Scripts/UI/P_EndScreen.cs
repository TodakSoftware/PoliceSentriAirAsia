using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable; 

public class P_EndScreen : MonoBehaviourPunCallbacks
{
    public Button endScreenleaveBtn, playAgainBtn;
    public GameObject scoreListPrefab, mvpPrefab, listContent;

    void Start(){
        endScreenleaveBtn.onClick.AddListener(delegate{ StartCoroutine(PhotonNetworkManager.instance.InGameLeaveRoom());});
        playAgainBtn.onClick.AddListener(delegate{ PlayAgainButton();});
        ShowScoreList();
    }

    void PlayAgainButton(){
        //GameManager.instance.endGamePlayAgain = true;
        //GameManager.instance.AskHostToPlayAgain();
        PlayerPrefs.SetInt("PlayAgain", 1);
        PlayerPrefs.Save();
        
        HideButton();
    }

    void HideButton(){
        playAgainBtn.gameObject.SetActive(false);
        endScreenleaveBtn.gameObject.SetActive(false);
    }

    void ShowScoreList ()
    {
        //Get Total Player Both Side
        List<GameObject> _totalPolice = GameManager.GetAllPlayersPolice();
        List<GameObject> _totalRobber = GameManager.GetAllPlayersRobber();

        // Police Score Calculation Start -------------------------------------------------
        for (int i = 0; i < _totalPolice.Count; i++)
        {
            var ScoreTemp = Instantiate(scoreListPrefab);
            ScoreTemp.transform.SetParent(listContent.transform, false);

            if(!_totalPolice[i].GetComponent<Police>().isBot){

                if(GameManager.GetCharacterIconHead("Police", _totalPolice[i].GetPhotonView().Controller.CustomProperties["CharacterCode"].ToString()) != null){
                    ScoreTemp.GetComponent<PlayerscoreList>().playerIcon.GetComponent<Image>().sprite = GameManager.GetCharacterIconHead("Police", _totalPolice[i].GetPhotonView().Controller.CustomProperties["CharacterCode"].ToString());
                }else{
                    ScoreTemp.GetComponent<PlayerscoreList>().playerIcon.GetComponent<Image>().sprite = GameManager.GetCharacterIconHead("Police", "P01");
                }
                
                ScoreTemp.GetComponent<PlayerscoreList>().playerName.GetComponent<TextMeshProUGUI>().text = _totalPolice[i].GetComponent<PlayerController>().playerNameText.text;
                
                if(_totalPolice[i].GetPhotonView().Controller.CustomProperties["PoliceCaughtCount"] != null){
                    ScoreTemp.GetComponent<PlayerscoreList>().scoreText.text = _totalPolice[i].GetPhotonView().Controller.CustomProperties["PoliceCaughtCount"].ToString();
                }else{
                    ScoreTemp.GetComponent<PlayerscoreList>().scoreText.text = "0";
                }

            }else{
                ScoreTemp.GetComponent<PlayerscoreList>().playerIcon.GetComponent<Image>().sprite = GameManager.GetCharacterIconHead("Police", "P01");
                ScoreTemp.GetComponent<PlayerscoreList>().playerName.GetComponent<TextMeshProUGUI>().text = _totalPolice[i].GetComponent<AIPolice>().playerNameText.text;
                ScoreTemp.GetComponent<PlayerscoreList>().scoreText.text = _totalPolice[i].GetComponent<AIPolice>().caughtCount.ToString();
            }
            

        //Loop Total Player & Instantiate Score List Prefab
        }
        // Police Score Calculation End -------------------------------------------------


        // Robber Score Calculation Start -------------------------------------------------
         for (int i = 0; i < _totalRobber.Count; i++)
        {
            var ScoreTemp = Instantiate(scoreListPrefab);
            ScoreTemp.transform.SetParent(listContent.transform, false);

            if(!_totalRobber[i].GetComponent<Robber>().isBot){

                if(GameManager.GetCharacterIconHead("Robber", _totalRobber[i].GetPhotonView().Controller.CustomProperties["CharacterCode"].ToString()) != null){
                    ScoreTemp.GetComponent<PlayerscoreList>().playerIcon.GetComponent<Image>().sprite = GameManager.GetCharacterIconHead("Robber", _totalRobber[i].GetPhotonView().Controller.CustomProperties["CharacterCode"].ToString());
                }else{
                    ScoreTemp.GetComponent<PlayerscoreList>().playerIcon.GetComponent<Image>().sprite = GameManager.GetCharacterIconHead("Robber", "R01");
                }
                
                ScoreTemp.GetComponent<PlayerscoreList>().playerName.GetComponent<TextMeshProUGUI>().text = _totalRobber[i].GetComponent<PlayerController>().playerNameText.text;
                
                if(_totalRobber[i].GetPhotonView().Controller.CustomProperties["RobberReleasedCount"] != null){
                    ScoreTemp.GetComponent<PlayerscoreList>().scoreText.text = _totalRobber[i].GetPhotonView().Controller.CustomProperties["RobberReleasedCount"].ToString();
                }else{
                    ScoreTemp.GetComponent<PlayerscoreList>().scoreText.text = "0";
                }

            }else{
                ScoreTemp.GetComponent<PlayerscoreList>().playerIcon.GetComponent<Image>().sprite = GameManager.GetCharacterIconHead("Robber", "R01");
                ScoreTemp.GetComponent<PlayerscoreList>().playerName.GetComponent<TextMeshProUGUI>().text = _totalRobber[i].GetComponent<AIRobber>().playerNameText.text;
                ScoreTemp.GetComponent<PlayerscoreList>().scoreText.text = _totalRobber[i].GetComponent<AIRobber>().releaseCount.ToString();
            }
            
            

        //Loop Total Player & Instantiate Score List Prefab
        }
        // Robber Score Calculation End -------------------------------------------------
    }
}
