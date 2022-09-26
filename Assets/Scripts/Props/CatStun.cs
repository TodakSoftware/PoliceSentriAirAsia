using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CatStun : MonoBehaviourPunCallbacks, IPunObservable
{
    public Sprite idleSprite;
    public List<GameObject> overlappedPlayers;
    //public float explodeCountdown = 3f;
    //[SerializeField] private float explodeTimer;

    [Header("Debug Lag Things")]
    Vector3 latestPos;
    float currentTime = 0;
    double currentPacketTime = 0;
    double lastPacketTime = 0;
    Vector3 positionAtLastPacket = Vector3.zero;
    public bool alreadyIdle, alreadySpawnSmoke, facingRight;
    public GameObject efx;
    
    void Start()
    {
        //explodeTimer = explodeCountdown;

        //if(Time.fixedTime%.5<.2){
            //StartCoroutine(flickerRed());
            if(photonView.IsMine){
                photonView.RPC("flickerRed", RpcTarget.All);
            }
        //}

        if(facingRight){
            transform.localScale = new Vector3(-1f,1f,1f);
        }else if(transform.position.magnitude < 0f){
            transform.localScale = new Vector3(1f,1f,1f);
        }
    }

    [PunRPC]
    IEnumerator flickerRed(){
        GetComponent<SpriteRenderer>().color = Color.red;
        yield return new WaitForSeconds(0.1f);
        GetComponent<SpriteRenderer>().color = Color.white;
        yield return new WaitForSeconds(0.1f);
        GetComponent<SpriteRenderer>().color = Color.red;
        yield return new WaitForSeconds(0.1f);
        GetComponent<SpriteRenderer>().color = Color.white;
        yield return new WaitForSeconds(0.1f);
        GetComponent<SpriteRenderer>().color = Color.red;
        yield return new WaitForSeconds(0.1f);
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    // Update is called once per frame
    void Update()
    {
        if(photonView.IsMine){
        if(!alreadyIdle){
            if(GetComponent<Rigidbody2D>().velocity.x != 0){
                if(facingRight && GetComponent<Rigidbody2D>().velocity.x < 4f){
                    if(photonView.IsMine){
                        photonView.RPC("ChangeToIdle", RpcTarget.All);
                    }
                    alreadyIdle = true;
                    //efx.SetActive(true);
                }else if(!facingRight && GetComponent<Rigidbody2D>().velocity.x > -4f){
                    if(photonView.IsMine){
                        photonView.RPC("ChangeToIdle", RpcTarget.All);
                    }
                    alreadyIdle = true;
                    //efx.SetActive(true);
                }
            }else{
                if(GetComponent<Rigidbody2D>().velocity.y < 4f && GetComponent<Rigidbody2D>().velocity.y > -4f){
                    if(photonView.IsMine){
                        photonView.RPC("ChangeToIdle", RpcTarget.All);
                    }
                    alreadyIdle = true;
                    
                    //efx.SetActive(true);
                }
            }
        }
        
        //if(explodeTimer <= 0){
            //explodeTimer = 0;
            /*if(!alreadySpawnSmoke){
                var smoke = PhotonNetwork.Instantiate(ConnectionManager.instance.GetParticlesName("PuffSmoke"), transform.position + new Vector3(0, .2f, 0), Quaternion.identity);
                //efx.SetActive(true);
                if(photonView.IsMine){
                    photonView.RPC("Exploding", RpcTarget.AllBuffered);
                }
                //var smoke = Instantiate(smokePrefab, transform.position + new Vector3(0, .2f, 0), Quaternion.identity);
                alreadySpawnSmoke = true;
                //Destroy(gameObject, .2f);
                //if(photonView.IsMine)
                //    photonView.RPC("DestroyMe", RpcTarget.AllBuffered);

                if(photonView.IsMine){
                    if(gameObject != null)
                        photonView.RPC("DestroyMe", RpcTarget.AllBuffered);
                    else
                        print("Nullllll");
                }else{
                    if(gameObject != null)
                        StartCoroutine(DestroyMe());
                }

            }
            if(overlappedPlayers.Count > 0){
                foreach(var go in overlappedPlayers){
                    go.GetComponent<PlayerControllerPhoton>().photonView.RPC("StunnedByCat", RpcTarget.AllBuffered, 3f, transform.position);
                }
            }
            GetComponent<BoxCollider2D>().enabled = false;
            GetComponent<CircleCollider2D>().enabled = false;*/
            //if(photonView.IsMine)
            //photonView.RPC("DestroyMe", RpcTarget.AllBuffered);
        //}else{
        //    explodeTimer -= Time.deltaTime;
            //if(explodeTimer <= 1.5){
                //if(Time.fixedTime%.5<.2){
                //    //StartCoroutine(flickerRed());
                //    if(photonView.IsMine){
                //        photonView.RPC("flickerRed", RpcTarget.AllBuffered);
                //    }
                //}
            //}
        //}

        //if(!photonView.IsMine){
        //    double timeToReachGoal = currentPacketTime - lastPacketTime;
        //    currentTime += Time.deltaTime;
//
        //    transform.position = Vector3.Lerp(positionAtLastPacket, latestPos, (float)(currentTime / timeToReachGoal));
        //}
        }else{
            double timeToReachGoal = currentPacketTime - lastPacketTime;
            currentTime += Time.deltaTime;

            transform.position = Vector3.Lerp(positionAtLastPacket, latestPos, (float)(currentTime / timeToReachGoal));
        }
    }

    void DelayExplode(){
        if(!alreadySpawnSmoke){
            var smoke = PhotonNetwork.Instantiate(NetworkManager.GetPhotonPrefab("Particles", "PuffSmoke"), transform.position + new Vector3(0, .2f, 0), Quaternion.identity);
            
            if(photonView.IsMine){
                photonView.RPC("Exploding", RpcTarget.All);
            }
            
            alreadySpawnSmoke = true;
        }

        if(overlappedPlayers.Count > 0){
            foreach(var go in overlappedPlayers){
                if(go.CompareTag("Police") && !go.GetComponent<Police>().isBot){
                    go.GetComponent<PlayerAbilities>().photonView.RPC("StunnedByCat", RpcTarget.All, 3f, transform.position);
                    print("Police not bot");
                }else if(go.CompareTag("Robber") && !go.GetComponent<Robber>().isBot){
                    go.GetComponent<PlayerAbilities>().photonView.RPC("StunnedByCat", RpcTarget.All, 3f, transform.position);
                    print("Robber not bot");
                }else{
                    if(go.CompareTag("Police")){
                        go.GetComponent<AIPolice>().StunnedByCat(3f,transform.position);
                    }else if(go.CompareTag("Robber")){
                        go.GetComponent<AIRobber>().StunnedByCat(3f,transform.position);
                    }
                } 
            }

            GetComponent<BoxCollider2D>().enabled = false;
            GetComponent<CircleCollider2D>().enabled = false;
        }else{
            print("not enough");
        }

        if(gameObject != null)
            StartCoroutine(DestroyMe());
        
    }

    [PunRPC]
    public void Exploding(){
        efx.SetActive(true);
    }

    [PunRPC]
    public void ChangeToIdle(){
        GetComponent<SpriteRenderer>().sprite = idleSprite;
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

        Invoke("DelayExplode",.5f);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
        if(stream != null && photonView != null){
            if(stream.IsWriting){
                stream.SendNext(transform.position);
                stream.SendNext(GetComponent<Rigidbody2D>().velocity);
                stream.SendNext(GetComponent<Rigidbody2D>().angularVelocity);
            }else{
                latestPos = (Vector3)stream.ReceiveNext();
                GetComponent<Rigidbody2D>().velocity = (Vector2)stream.ReceiveNext();
                GetComponent<Rigidbody2D>().angularVelocity = (float)stream.ReceiveNext();

                currentTime = 0.0f;
                lastPacketTime = currentPacketTime;
                currentPacketTime = info.SentServerTime;
                positionAtLastPacket = transform.position;
            }
        }
    }

    void OnTriggerStay2D(Collider2D other) {
        if(other.CompareTag("Robber") || other.CompareTag("Police")){
            if(!overlappedPlayers.Contains(other.gameObject)){
                overlappedPlayers.Add(other.gameObject);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Robber") || other.CompareTag("Police")){
            overlappedPlayers.Remove(other.gameObject);
        }
    }

    IEnumerator DestroyMe(){
        yield return new WaitForSeconds(3f);
        if(gameObject != null){
            if(photonView.IsMine){
                PhotonNetwork.Destroy(GetComponent<PhotonView>());
            }
        }
    }
}
