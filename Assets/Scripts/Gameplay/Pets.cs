using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Pets : MonoBehaviourPunCallbacks
{
    private Animator animator;
    public SO_PetVariant petVariant;
    public SpriteRenderer spriteRenderer;
    public RuntimeAnimatorController animatorController;
    public GameObject myMaster;
    public bool lookRight;
    private bool playRunAnim;

    new void OnEnable(){
        if(myMaster != null){
            animatorController = petVariant.animatorLists[myMaster.GetComponent<PlayerController>().petIndex].runTimeAnimController;
            animator.runtimeAnimatorController = animatorController;
        }
    }

    new void OnDisable(){
        //if(photonView.IsMine){
        //    photonView.RPC("StopAnim", RpcTarget.AllBuffered);
        //}
        StopAnim();
    }

    [PunRPC]
    public void AnimationSlideFix(){
        if(myMaster != null){
            if(myMaster.GetComponent<PlayerController>().isMoving && !playRunAnim){
                playRunAnim = true;
                animator.SetBool("Run", true);
            }
            
            if(!myMaster.GetComponent<PlayerController>().isMoving && playRunAnim){
                playRunAnim = false;
                animator.SetBool("Run", false);
            }
        }
    }

    public void StopAnim(){
        if(playRunAnim){
            playRunAnim = false;
        }
    }

    public void Awake() {
        animator = spriteRenderer.gameObject.GetComponent<Animator>();
    }

    void Start()
    {
        //animator.runtimeAnimatorController = animatorController;
        if(lookRight){
            transform.localScale = new Vector3(1f, transform.localScale.y, transform.localScale.z);
        }

        if(photonView.IsMine){
            photonView.RPC("AnimationSlideFix", RpcTarget.AllBuffered);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(myMaster != null){
            if(myMaster.GetComponent<PlayerController>().isMoving && !playRunAnim){
                playRunAnim = true;
                animator.SetBool("Run", true);
            }
            
            if(!myMaster.GetComponent<PlayerController>().isMoving || myMaster.GetComponent<PlayerController>().isFalling && playRunAnim){
                playRunAnim = false;
                animator.SetBool("Run", false);
            }
        }
    }

    [PunRPC]
    public void AssignController(int toChangeIndex){ 
        animator.runtimeAnimatorController = petVariant.animatorLists[toChangeIndex].runTimeAnimController;
    }

    [PunRPC]
    public void SwitchAnimController(int toChangeIndex){ 
        animatorController = petVariant.animatorLists[toChangeIndex].runTimeAnimController;
        animator.runtimeAnimatorController = animatorController;
    }
}
