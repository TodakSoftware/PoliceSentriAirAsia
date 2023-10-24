using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WebRequest : MonoBehaviour
{
    public string baseUrl = ""; // Initialize with an empty string

    public string memberId = "9999990005183161";
    public int points = 10;
    public string description = "Sample Description";

    [System.Obsolete]
    public void SendDataToAirAsia(string _memberID, int _point, string _desc)
    {
        //baseUrl = Application.absoluteURL;
        memberId = _memberID;
        points = _point;
        description = _desc;

        // Create the URL with parameters
        string url = baseUrl + "?memberId=" + memberId + "" + points + "&description=" + description;

        // Start a coroutine to send the GET request
        StartCoroutine(SendGetRequest(url));
    }

    [System.Obsolete]
    IEnumerator SendGetRequest(string url)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            // Send the GET request
            yield return www.SendWebRequest();

            // Check for errors
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError("Error: " + www.error);
            }
            else
            {
                // Request was successful, and you can access the response data here
                string responseData = www.downloadHandler.text;
                //Debug.Log("Response: " + responseData);
                print("Successfully");
                // You can parse and handle the response data as needed
            }
        }
    }
}
