using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

public class AirAsiaProfile : MonoBehaviour
{
    public TextMeshProUGUI usernameInput;
    public Image avatarImage;

    void Start()
    {
        if(PhotonNetworkManager.instance.hasInternet){
            FetchAirasiaData();
        }
    }

    public void FetchAirasiaData(){
        if(PhotonNetworkManager.instance.hasInternet){
            string userText = usernameInput.text.ToLower();
            userText = userText.Replace(" ", "_");
            string imageURL = "https://storage.googleapis.com/life-stgairasia-com-tfstg-bini/games/police-sentri/index.html?nickname=" + userText + "&avatar=";
            //string imageURL = "https://upload.wikimedia.org/wikipedia/commons/thumb/8/8b/AirAsia_X_Logo.svg/800px-AirAsia_X_Logo.svg.png";

            // Start a coroutine to download the image from the URL
            StartCoroutine(DownloadImage(imageURL));
        }
    }

    IEnumerator DownloadImage(string url)
    {
        // Start a UnityWebRequest to download the image
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        // Check for errors during the download
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error downloading image: " + request.error);
            NotificationManager.instance.PopupNotification("Error Fetching Data");
            yield break;
        }

        // Get the downloaded texture from the request
        Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;

        // Create a Sprite object from the downloaded texture
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

        // Use the texture as desired (e.g. assign it to a UI element)
        avatarImage.sprite = sprite;
    }

}
