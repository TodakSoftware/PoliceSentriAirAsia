using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class C_UserData{
    public float currentKupang = 0;
    public List<string> policeList = new List<string>();
    public List<string> robberList = new List<string>();
    public List<string> petList = new List<string>();

    [Header("---- Daily Rewards -----")]
    public string startDailyRewardDate;
    public int latestRewardClaimedDay; // button index day claimed
}