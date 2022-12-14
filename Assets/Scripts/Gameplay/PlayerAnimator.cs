using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerAnimator : MonoBehaviourPunCallbacks
{
    [HideInInspector] public Animator animator;
    string currentState;
    public RuntimeAnimatorController animatorController; // used by PlayerController [ANIMATION RELATED]
    public RuntimeAnimatorController originalAnimatorController;

    void Awake() {
        animator = GetComponent<Animator>();
    } // end Awake

    [PunRPC]
    public void SwitchAnimController(string team, string charCode){ // Switching own team design, not undercover
        switch(team){
            case "Police":
                originalAnimatorController = animatorController;
                foreach(var p in SOManager.instance.animVariantPolice.animatorLists){
                    if(p.code == charCode){
                        animatorController = p.runTimeAnimController;
                        animator.runtimeAnimatorController = animatorController;
                        

                        // Set Player New Code
                        if(GameManager.instance.ownedPlayerGO != null){
                            GameManager.instance.ownedPlayerGO.GetComponent<PlayerController>().characterCode = p.code;
                        }

                        if(photonView.IsMine){ // Save Changes to network
                            Hashtable teamRole = new Hashtable();
                            teamRole.Add("CharacterCode", charCode);
                            PhotonNetwork.LocalPlayer.SetCustomProperties(teamRole);
                        }
                    }
                }
            break;

            case "Robber":
                originalAnimatorController = animatorController;
                foreach(var p in SOManager.instance.animVariantRobber.animatorLists){
                    if(p.code == charCode){
                        animatorController = p.runTimeAnimController;
                        animator.runtimeAnimatorController = animatorController;

                        // Set Player New Code
                        if(GameManager.instance.ownedPlayerGO != null){
                            GameManager.instance.ownedPlayerGO.GetComponent<PlayerController>().characterCode = p.code;
                        }

                        if(photonView.IsMine){ // Save Changes to network
                            Hashtable teamRole = new Hashtable();
                            teamRole.Add("CharacterCode", charCode);
                            PhotonNetwork.LocalPlayer.SetCustomProperties(teamRole);
                        }
                    }
                }
            break;

            case "Shared":
                // SHARED
                originalAnimatorController = animatorController;
                foreach(var p in SOManager.instance.sharedVariant.animatorLists){
                    if(p.code == charCode){
                        animatorController = p.runTimeAnimController;
                        animator.runtimeAnimatorController = animatorController;
                        //originalAnimatorController = animatorController;

                        // Set Player New Code
                        if(GameManager.instance.ownedPlayerGO != null){
                            GameManager.instance.ownedPlayerGO.GetComponent<PlayerController>().characterCode = p.code;
                        }

                        if(photonView.IsMine){ // Save Changes to network
                            Hashtable teamRole = new Hashtable();
                            teamRole.Add("CharacterCode", charCode);
                            PhotonNetwork.LocalPlayer.SetCustomProperties(teamRole);
                        }
                    }
                }
            break;

            default:
            break;
        }
        
        
    } // end SwitchAnimController

    [PunRPC]
    public void RevertAnimController(){
        animatorController = originalAnimatorController;
        animator.runtimeAnimatorController = animatorController;
    } // end RevertAnimController

    public void PlayAnimation(string animName){
        if(photonView.IsMine){
            switch(animName){
                case "Idle":
                    animator.SetBool("Run", false);
                    animator.SetBool("Dash", false);
                    animator.SetBool("Fall", false);
                break;

                case "Run":
                    animator.SetBool("Run", true);
                    animator.SetBool("Dash", false);
                    animator.SetBool("Fall", false);
                break;

                case "Dash":
                    animator.SetBool("Run", false);
                    animator.SetBool("Dash", true);
                    animator.SetBool("Fall", false);
                break;

                case "Fall":
                    animator.SetBool("Run", false);
                    animator.SetBool("Dash", false);
                    animator.SetBool("Fall", true);
                break;
            } // end switch
        }
    } // end PlayAnim

}
