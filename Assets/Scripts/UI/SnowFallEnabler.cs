using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowFallEnabler : MonoBehaviour
{
    public bool enable;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("CheckingSnow", 2f);
    }

    void CheckingSnow(){
        if(PlayerPrefs.GetString("CristmasEvent") == "true"){
            enable = true;
        }else{
            enable = false;
        }

        if(enable){
            GetComponent<ParticleSystem>().Play();
        }else{
            GetComponent<ParticleSystem>().Stop();
        }
    }

}
