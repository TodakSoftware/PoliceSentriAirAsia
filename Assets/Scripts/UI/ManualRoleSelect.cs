using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManualRoleSelect : MonoBehaviour
{
   public TextMeshProUGUI robberCountText, policeCountText, chooseRandomText;
   public Button joinRobberBtn, joinPoliceBtn, randomJoinBtn;
   [HideInInspector] public GameObject characterSelectScreen;
   public Coroutine randomChooseCoroutine;

   void Start(){
      joinPoliceBtn.onClick.AddListener(delegate{GameManager.instance.SpawnCharacterTeam("Police");});
      joinRobberBtn.onClick.AddListener(delegate{GameManager.instance.SpawnCharacterTeam("Robber");});
      randomJoinBtn.onClick.AddListener(delegate{GameManager.instance.ChooseRandom();});
      characterSelectScreen = UIManager.instance.cacheCharacterSelect;
      randomChooseCoroutine = StartCoroutine(ChooseRandomCountdown());
   }

   IEnumerator ChooseRandomCountdown(){
      int duration = 5;

      while(duration > 0){
         chooseRandomText.text = "Choose For Me("+ duration+")";
         duration--;
         yield return new WaitForSeconds(1f);
      }

      if(duration <= 0){
         chooseRandomText.text = "Selecting...";
         GameManager.instance.ChooseRandom();
         
         this.gameObject.SetActive(false);
      } // end if(duration <= 0)
   }

   
}
