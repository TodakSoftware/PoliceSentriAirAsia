using UnityEngine;
using System.Collections;
using System;

[System.Serializable]
public class PayloadData
{
    public string memberId;
    public string partnerCode;
    public string transactionDate;
    public string points;
    public string description;
    public string referenceNumber;
    public string partnerMID;
}

public class PayloadSender : MonoBehaviour
{
    public static PayloadSender instance;
    [TextArea] public string publicKey;
    public bool payloadSent = false;
    public Action OnPayloadSentWebview;

    void Awake(){
        if(instance == null){
            instance = this;
        }else{
            Destroy(gameObject);
        }
    }

    [System.Obsolete]
    public void SendPayloadParams(PayloadData _payLoadData)
    {
        // Serialize the payload object to JSON
        string payloadJson = JsonUtility.ToJson(_payLoadData);

        // Call the JavaScript function with the payload JSON
        Application.ExternalCall("sendPayloadToWebview", payloadJson);
        //Application.ExternalCall("sendPayloadToWebview", payloadJson, publicKey);
        //StartCoroutine(CheckPayloadSent());
    }

    private IEnumerator CheckPayloadSent()
    {
        float timeout = 3f; // Adjust timeout as needed
        float elapsed = 0f;

        while (!payloadSent && elapsed < timeout)
        {
            yield return null;
            elapsed += Time.deltaTime;
        }

        if (payloadSent)
        {
            Debug.Log("Payload sent successfully");
            // Handle successful send
            OnPayloadSentWebview?.Invoke();
        }
        else
        {
            Debug.Log("Payload send timed out");
            // Handle timeout
            OnPayloadSentWebview?.Invoke();
        }

        payloadSent = false; // Reset for next send
    }

    // Call this method from JavaScript when the payload is sent
    public void OnPayloadSent()
    {
        payloadSent = true;
        OnPayloadSentWebview?.Invoke();
    }
}