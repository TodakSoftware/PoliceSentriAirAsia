using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Flashbang : MonoBehaviourPunCallbacks, IPunObservable
{
    public List<GameObject> overlappedPlayers;
    public float explodeCountdown = 3f;
    private float explodeTimer;

    [Header("Debug Lag Things")]
    Vector3 latestPos;
    float currentTime = 0;
    double currentPacketTime = 0;
    double lastPacketTime = 0;
    Vector3 positionAtLastPacket = Vector3.zero;
    
    void Start()
    {
        explodeTimer = explodeCountdown;
    }

    // Update is called once per frame
    void Update()
    {
        if(explodeTimer <= 0){
            explodeTimer = 0;
            if(overlappedPlayers.Count > 0){
                foreach(var go in overlappedPlayers){
                    if(go.GetComponent<PlayerAbilities>().enabled || go.GetComponent<PlayerAbilities>() != null)
                    go.GetComponent<PlayerAbilities>().photonView.RPC("BlindByFlashbang", RpcTarget.All);
                }
            }
            if(photonView.IsMine)
                PhotonNetwork.Destroy(GetComponent<PhotonView>()); //photonView.RPC("DestroyMe", RpcTarget.All);
        }else{
            explodeTimer -= Time.deltaTime;
        }

        if(!photonView.IsMine){
            double timeToReachGoal = currentPacketTime - lastPacketTime;
            currentTime += Time.deltaTime;

            transform.position = Vector3.Lerp(positionAtLastPacket, latestPos, (float)(currentTime / timeToReachGoal));
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

    [PunRPC]
    public void DestroyMe(){
        Destroy(gameObject);
    }
}
