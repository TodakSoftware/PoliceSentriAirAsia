using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Cauliflower : MonoBehaviourPunCallbacks, IPunObservable
{
    public string teamOwner;
    public float dur = 3f;
    private bool hitted;
    public GameObject ownerGO;
    public Player itemOwner;
    float timerToDestroy;

    [Header("Debug Lag Things")]
    Vector3 latestPos;
    float currentTime = 0;
    double currentPacketTime = 0;
    double lastPacketTime = 0;
    Vector3 positionAtLastPacket = Vector3.zero;

    void Start(){
        timerToDestroy = dur;
    }

    void Update(){
        if(!photonView.IsMine){
            double timeToReachGoal = currentPacketTime - lastPacketTime;
            currentTime += Time.deltaTime;

            transform.position = Vector3.Lerp(positionAtLastPacket, latestPos, (float)(currentTime / timeToReachGoal));
        }

        if(timerToDestroy <= 0){
            timerToDestroy = 0;
            if(photonView.IsMine){
                PhotonNetwork.Destroy(GetComponent<PhotonView>());
            }
        }else{
            timerToDestroy -= Time.deltaTime;
        }
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

    void OnTriggerEnter2D(Collider2D other) {
            if(photonView.IsMine){
                if(other.CompareTag("Robber") || other.CompareTag("Police") && !hitted && !other.GetComponent<PlayerController>().isFalling){
                    AudioManager.instance.PlaySound("PS_UI_BodyHit");

                    // Send Statistic
                    /*if(itemOwner != null && teamOwner == "Police" && other.CompareTag("Robber") && !hitted && ownerGO.GetComponent<PlayerStatisticPhoton>().sharpShooterHit == 0){
                        ownerGO.GetComponent<PlayerStatisticPhoton>().photonView.RPC("AddSharpshooterCount", RpcTarget.AllBuffered, 1);
                    }else if(itemOwner != null && teamOwner == "Robber" && other.CompareTag("Police") && !hitted && ownerGO.GetComponent<PlayerStatisticPhoton>().sharpShooterHit == 0){
                        ownerGO.GetComponent<PlayerStatisticPhoton>().photonView.RPC("AddSharpshooterCount", RpcTarget.AllBuffered, 1);
                    }*/


                    if(other.CompareTag("Robber") && !other.GetComponent<Robber>().isCaught && other.gameObject != ownerGO){
                        other.GetComponent<PlayerController>().photonView.RPC("PlayerFall", RpcTarget.All, dur);
                        if(photonView.IsMine){
                            PhotonNetwork.Destroy(GetComponent<PhotonView>()); // 
                        }
                        var explode = PhotonNetwork.Instantiate(PhotonNetworkManager.GetPhotonPrefab("Particles", "CauliflowerExplode"), transform.position, Quaternion.identity);
                    }else if(other.CompareTag("Police") && other.gameObject != ownerGO){
                        other.GetComponent<PlayerController>().photonView.RPC("PlayerFall", RpcTarget.All, dur);
                        if(photonView.IsMine){
                            PhotonNetwork.Destroy(GetComponent<PhotonView>());
                        }
                        var explode = PhotonNetwork.Instantiate(PhotonNetworkManager.GetPhotonPrefab("Particles", "CauliflowerExplode"), transform.position, Quaternion.identity);
                    }else if(other.CompareTag("Police") && other.gameObject == ownerGO){
                        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), other.GetComponent<Collider2D>());
                    }

                    hitted = true;
                }else{
                    var explode = PhotonNetwork.Instantiate(PhotonNetworkManager.GetPhotonPrefab("Particles", "CauliflowerExplode"), transform.position, Quaternion.identity);
                    if(photonView.IsMine){
                        PhotonNetwork.Destroy(GetComponent<PhotonView>());
                    }
                }
            }
    }

    [PunRPC]
    public void DestroyMe(){
        GetComponent<SpriteRenderer>().enabled = false;
        Destroy(GetComponent<Collider2D>());
        Destroy(gameObject);
    }
}
