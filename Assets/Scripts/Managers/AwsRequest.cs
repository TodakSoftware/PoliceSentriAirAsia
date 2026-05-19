using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Collections;

public class AwsRequest : MonoBehaviour
{
    // Replace these with your AWS credentials
    private const string accessKey = "AKIAWENSX2JPCUBIWPPM";
    private const string secretKey = "Jga700QbY7KDBnYSdCGdS7FvWpu+hcBUIzHAo5L3";
    private const string region = "ap-southeast-1"; // AWS region
    private const string service = "execute-api"; // AWS service name

    // Example request
    private const string httpMethod = "POST";
    private const string canonicalUri = "/v1/external/accrual/activity";
    private const string hostHeader = "loki.airasiabig.com";
    public string corsProxyUrl = "https://cors-anywhere.herokuapp.com/";
    bool corsAnywhereWorking;

    [Header("Send Parameters")]
    string _memberId = ""; // Replace with the actual memberId //9999990005183161
    [SerializeField] string _partnerCode = "TBDSTGACT1";
    int _points = 0;
    string _description = "";
    string _referenceNumber = ""; // autoGenerate
    [SerializeField] string _partnerMID = "1111000101";

    void Start()
    {
        StartCoroutine(VerifyCorsProxy());
    }

    public void SendDataToAirAsia(string _MemberID, int _Points, string _Description){
        if(corsAnywhereWorking){
            _memberId = _MemberID;
            _points = _Points;
            _description = _Description;
            _referenceNumber = GenerateRandomCode();

            print(_description + " Code : "+ _referenceNumber);

            SendIt();
        }else{
            NotificationManager.instance.PopupNotification("Unexpected Error!");
        }
    }

    private string GenerateRandomCode()
    {
        // Generate a random part of the code
        string randomPart = "";
        System.Random random = new System.Random();
        for (int i = 0; i < 7; i++)
        {
            randomPart += random.Next(10); // Append a random digit (0-9)
        }

        // Get the current year
        int currentYear = DateTime.Now.Year;

        // Create the final code by combining the prefix, year, and random part
        string generatedCode = $"TAG{currentYear}{randomPart}";

        return generatedCode;
    }

    private void SendIt()
    {
        // Include the input parameters in the request body as a JSON object
        var requestBody = new
        {
            memberId = _memberId, // Replace with the actual memberId
            partnerCode = _partnerCode,
            transactionDate = formatDateToCustomFormat(), // Use the same formatted date
            points = _points, // Replace with the actual number of points
            description = _description, // Replace with the actual description
            referenceNumber = _referenceNumber, // Replace with the actual reference number
            partnerMID = _partnerMID // Replace with the actual partner MID
        };

        // Convert the request body to a JSON string
        string payload = JsonUtility.ToJson(requestBody);

        // Construct the full URL of the AWS service (replace with your service URL)
        string serviceUrl = "https://" + hostHeader + canonicalUri;

        // Get the AWS Authorization Header
        string awsAuthorizationHeader = GetAwsAuthorizationHeader(httpMethod, canonicalUri, hostHeader, payload);

        // Use a CORS proxy service URL
        //string corsProxyUrl = "https://cors-anywhere.herokuapp.com/";

        // Construct the full URL of the AWS service with the CORS proxy
        string serviceUrlWithProxy = corsProxyUrl + serviceUrl;

        // Create UnityWebRequest
        var request = new UnityWebRequest(serviceUrlWithProxy, httpMethod);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(payload);

        // Set headers
        request.SetRequestHeader("Host", hostHeader);
        request.SetRequestHeader("X-Amz-Date", formatDateToCustomFormat());
        request.SetRequestHeader("Authorization", awsAuthorizationHeader);
        request.SetRequestHeader("Content-Type", "application/json");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // Send the request
        var operation = request.SendWebRequest();

        // Handle the response
        operation.completed += (op) =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log("Response Data: " + responseText);
                // Handle the successful response here
                NotificationManager.instance.PopupNotification("Airasia Points Claimed!");
            }
            else
            {
                Debug.LogError("Request Error: " + request.error);
                // Handle the error here
                NotificationManager.instance.PopupNotification("Failed to receive Airasia Points!");
            }
        };
    }

    // Function to create a hex-encoded HMAC-SHA256 hash
    private string HmacSha256(string key, string data)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
        {
            byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

    // Function to format the current date in "yyyyMMddTHHmmssZ" format
    private string formatDateToCustomFormat()
    {
        DateTime now = DateTime.UtcNow;
        return now.ToString("yyyyMMddTHHmmssZ");
    }

    // Function to get the AWS Authorization Header
    private string GetAwsAuthorizationHeader(string httpMethod, string canonicalUri, string host, string payload)
    {
        DateTime now = DateTime.UtcNow;
        string currentDate = now.ToString("yyyyMMdd");
        string amzDate = now.ToString("yyyyMMddTHHmmssZ");

        string signedHeaders = "host;x-amz-date";
        string canonicalHeaders = "host:" + host + "\nx-amz-date:" + amzDate;

        string hashedPayload = HmacSha256(payload, "");
        string canonicalRequest = $"{httpMethod}\n{canonicalUri}\n\n{canonicalHeaders}\n{signedHeaders}\n{hashedPayload}";

        string hashedCanonicalRequest = HmacSha256(canonicalRequest, "");
        string stringToSign = $"AWS4-HMAC-SHA256\n{amzDate}\n{currentDate}/{region}/{service}/aws4_request\n{hashedCanonicalRequest}";

        byte[] signingKey = Encoding.UTF8.GetBytes($"AWS4{secretKey}");
        foreach (string scope in new[] { currentDate, region, service, "aws4_request" })
        {
            signingKey = Encoding.UTF8.GetBytes(HmacSha256(Encoding.UTF8.GetString(signingKey), scope));
        }

        string signature = HmacSha256(Encoding.UTF8.GetString(signingKey), stringToSign);

        return $"AWS4-HMAC-SHA256 Credential={accessKey}/{currentDate}/{region}/{service}/aws4_request, SignedHeaders={signedHeaders}, Signature={signature}";
    }

    private IEnumerator VerifyCorsProxy()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(corsProxyUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("CORS proxy is operational and accessible.");
                corsAnywhereWorking = true;
            }
            else
            {
                Debug.LogError("CORS proxy is not accessible. Error: " + request.error);
                corsAnywhereWorking = false;
            }
        }
    }
}
