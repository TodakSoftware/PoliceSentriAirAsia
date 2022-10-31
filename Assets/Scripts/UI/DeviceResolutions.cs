using UnityEngine;
using TMPro;

public class DeviceResolutions : MonoBehaviour
{
    void Start(){
        GetComponent<TextMeshProUGUI>().text = "Device: " + Screen.currentResolution.width + " x " + Screen.currentResolution.height + " | Current: " + Screen.width + " x " + Screen.height;
    }
}
