using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ItemRandomSpawner : MonoBehaviourPunCallbacks
{
    public static ItemRandomSpawner instance;
    public List<Transform> itemSpawnpoints;
    public int spawnAmount = 4;
    [HideInInspector] public List<int> randomSpawnIndex;
    public float newSpawnDuration = 20f;
    public float newSpawnTimer;

    void Start(){
        instance = this;
        /*if(PhotonNetwork.IsMasterClient){
            Spawning();
        }*/
    }

    void Update(){
        if(GameManager.instance.gameStarted && !GameManager.instance.gameEnded && PhotonNetwork.IsMasterClient){
            if(newSpawnTimer <= 0){
                newSpawnTimer = 0;
                // Clear spawnpoint
                Spawning();
            }else{
                newSpawnTimer -= Time.deltaTime;
            }
        } // end if gameend = false
    }

    public void Spawning(){
        newSpawnTimer = newSpawnDuration;
        randomSpawnIndex.Clear();
            for(int i = 0; i < spawnAmount; i++){
                var spawnRdm = Random.Range(0, itemSpawnpoints.Count);
                if(!randomSpawnIndex.Contains(spawnRdm)){ // if doesnt have yet, spawn normally
                    randomSpawnIndex.Add(spawnRdm);
                    var props = PhotonNetwork.InstantiateRoomObject(PhotonNetworkManager.GetPhotonPrefab("Props", "ItemBox"), itemSpawnpoints[spawnRdm].position + new Vector3(Random.Range(-2,2f),Random.Range(-2,2f),0), Quaternion.identity);
                    
                    props.AddComponent<PropsAnimation>();
                    if(props.GetComponent<PropsAnimation>() != null){
                        props.GetComponent<PropsAnimation>().duration = newSpawnDuration - 1.5f;
                    }
                }else{ // else random again, n add new nmbr
                    var newSpawnRdm = Random.Range(0, itemSpawnpoints.Count);
                    if(!randomSpawnIndex.Contains(newSpawnRdm)){
                        randomSpawnIndex.Add(newSpawnRdm);
                        var props = PhotonNetwork.InstantiateRoomObject(PhotonNetworkManager.GetPhotonPrefab("Props", "ItemBox"), itemSpawnpoints[newSpawnRdm].position + new Vector3(Random.Range(-2,2f),Random.Range(-2,2f),0), Quaternion.identity);
                        
                        props.AddComponent<PropsAnimation>();
                        if(props.GetComponent<PropsAnimation>() != null){
                            props.GetComponent<PropsAnimation>().duration = newSpawnDuration - 1.5f;
                        }
                    }
                }
            }
    }


}
