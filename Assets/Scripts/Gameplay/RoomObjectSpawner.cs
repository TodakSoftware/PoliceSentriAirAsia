using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[System.Serializable]
public struct roomObjectData{
    public string name;
    public E_PrefabType type;
    public Transform location;
}

public class RoomObjectSpawner : MonoBehaviourPunCallbacks
{
    [Header("Spawners")]
    public List<roomObjectData> roomObjects = new List<roomObjectData>();

    void Start(){
        if(roomObjects.Count > 0){
            foreach(var obj in roomObjects){
                var roomObj = PhotonNetwork.InstantiateRoomObject(obj.name, obj.location.position, Quaternion.identity);
            }
        } // end if(roomObjects.Count > 0)
    }
}
