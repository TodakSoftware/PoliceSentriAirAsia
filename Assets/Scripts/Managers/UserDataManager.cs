using System.Collections.Generic;
using UnityEngine;

public class UserDataManager : MonoBehaviour
{
    public static UserDataManager instance;
    public float currentKupang = 0;
    public List<string> policeList = new List<string>();
    public List<string> robberList = new List<string>();
    public List<string> petList = new List<string>();

    [Header("---- Daily Rewards -----")]
    public List<string> dailyRewardDayClaimed = new List<string>();

    public void Awake()
    {
        if(instance == null){
            instance = this;
        }else{
            Destroy(gameObject);
        }
        
        DontDestroyOnLoad(gameObject);
    }

    
}
