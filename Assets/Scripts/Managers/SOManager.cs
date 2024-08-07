using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SOManager : MonoBehaviour
{
    public static SOManager instance;
    public SO_Maps maps;
    public SO_GameSettings gameSettings;
    public SO_Prefabs prefabs;
    public SO_AnimatorVariant animVariantPolice;
    public SO_AnimatorVariant animVariantRobber;
    public SO_PetVariant animVariantPet;
    public SO_AnimatorVariant sharedVariant;
    public SO_ItemInfo itemInfo;

    void Awake(){
        if(instance == null){
            instance = this;
        }else{
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }
}
