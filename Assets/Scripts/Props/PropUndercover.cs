using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PropUndercover : MonoBehaviourPunCallbacks
{
    public SpriteRenderer playerSprite;
    public GameObject playerNameCanvas;
    public bool isActive;
    public List<Sprite> propList;

    public void Update(){
        if(isActive && photonView.IsMine){
            // Check if player is moving
            if(transform.parent.gameObject.GetComponent<PlayerController>().isMoving){
                GetComponent<Animator>().SetBool("Walk", true);
            }else{
                GetComponent<Animator>().SetBool("Walk", false);
            }

            // if immune
            if(transform.parent.gameObject.GetComponent<PlayerAbilities>().onCaughtImmune){
                var spriteTransparent = GetComponent<SpriteRenderer>().color;
                spriteTransparent.a = .5f;
                GetComponent<SpriteRenderer>().color = spriteTransparent;
            }else{
                var spriteTransparent = GetComponent<SpriteRenderer>().color;
                spriteTransparent.a = 1f;
                GetComponent<SpriteRenderer>().color = spriteTransparent;
            }
            // run animation also
            // if (isFall/isStunned/Dash) cancel
        }
    }
    
    [PunRPC]
    public void ChangeToProps(int nmbr){
        if(transform.parent.gameObject.GetComponent<PlayerAbilities>().onUndercover){
            transform.parent.gameObject.GetComponent<PlayerAbilities>().photonView.RPC("DisableUndercover", RpcTarget.All);
        }
        // isActive = true
        isActive = true;
        // Humanoid Sprite alpha 0
        var spriteTransparent = playerSprite.color;
        spriteTransparent.a = 0f;
        playerSprite.color = spriteTransparent;
        // Spawn Asap
        if(photonView.IsMine){ 
        var smoke = PhotonNetwork.Instantiate(NetworkManager.GetPhotonPrefab("Particles", "PuffSmoke"), (transform.position + new Vector3(0, 1f, 0)), Quaternion.identity);
        }
        // set prop to sprite renderer
        GetComponent<SpriteRenderer>().sprite = propList[nmbr];
        // Hide name
        playerNameCanvas.SetActive(false);
    }

    [PunRPC]
    public void PropsToNormal(){
        // isActive = false
        isActive = false;
        // spawn asap
        if(photonView.IsMine){
        var smoke = PhotonNetwork.Instantiate(NetworkManager.GetPhotonPrefab("Particles", "PuffSmoke"), (transform.position + new Vector3(0, 1f, 0)), Quaternion.identity);
        }
        // humanoid sprite alpha 1
        if(transform.parent.gameObject.GetComponent<PlayerAbilities>().onCaughtImmune){
            var spriteTransparent = playerSprite.color;
            spriteTransparent.a = .5f;
            playerSprite.color = spriteTransparent;
        }else{
            var spriteTransparent = playerSprite.color;
            spriteTransparent.a = 1f;
            playerSprite.color = spriteTransparent;
        }
        // set prop on sprite render to none/null
        GetComponent<SpriteRenderer>().sprite = null;
        // Show name
        if(!transform.parent.gameObject.GetComponent<PlayerAbilities>().charInvisible){
            playerNameCanvas.SetActive(true);
        }

        gameObject.SetActive(false);
    }
}
