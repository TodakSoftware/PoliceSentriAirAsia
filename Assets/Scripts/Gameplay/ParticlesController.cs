using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesController : MonoBehaviour
{
    private ParticleSystem thisParticle;
    public bool autoDeactived, doneOneTime;
    public float duration;

    void OnEnable(){
        thisParticle = GetComponent<ParticleSystem>();
        if(!autoDeactived){
            if(doneOneTime){
                PlayParticle();
            }
            
        }else{
            Invoke("DeactiveParticles", duration);
        }
    }

    void Start(){
        PlayParticle();
        doneOneTime = true;
    }

    void OnDisable(){
        PlayStop();
    }

    public void PlayParticle(){
        thisParticle.Play();
    }

    public void PlayStop(){
        thisParticle.Stop();
    }

    public void DeactiveParticles(){
        gameObject.SetActive(false);
    }
}
