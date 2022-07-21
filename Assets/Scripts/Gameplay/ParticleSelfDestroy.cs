using System.Collections;
using Photon.Pun;
using UnityEngine;

public class ParticleSelfDestroy : MonoBehaviourPunCallbacks
{
    public float duration;
    public float timer;
    
    void Start()
    {
        //StartCoroutine(AutoDestroy());
        timer = duration;
    }

    void Update(){
        if(timer <= 0){
            timer = 0;
            if(gameObject != null){
                //if(GetComponent<ParticleSystem>().IsAlive()){
                    Destroy(gameObject);
               // }
            }
            
        }else{
            timer -= Time.deltaTime;
        }
    }
}
