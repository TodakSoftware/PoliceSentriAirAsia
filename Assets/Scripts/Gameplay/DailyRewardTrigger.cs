using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DailyRewardTrigger : MonoBehaviour
{
    private void OnEnable() {
        Invoke("DelayInvoke", 1f);
    }

    [System.Obsolete]
    void DelayInvoke(){
        DailyRewardManager.instance.Start2();
    }
}
