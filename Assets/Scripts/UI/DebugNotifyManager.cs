using UnityEngine;
using TMPro;

public class DebugNotifyManager : MonoBehaviour
{
    public static DebugNotifyManager instance;
    public Transform notificationGroupCOntent;
    public GameObject debubNotifyPrefab;

    void Awake(){
        if(instance == null){
            instance = this;
        }
    }

    public void PopupDebugText(string teks){
        var dbg = Instantiate(debubNotifyPrefab);
        dbg.transform.SetParent(notificationGroupCOntent,false);
        dbg.GetComponent<TextMeshProUGUI>().text = teks;
        Destroy(dbg, 2.5f);
    }
}
