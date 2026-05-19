using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Security.Cryptography;
using System.Collections;
using System.Globalization;

public class APIRequest : MonoBehaviour
{
    // Set these values in Unity's PlayerSettings or in script
    public string awsAccessKey = "AKIAWENSX2JPCUBIWPPM";
    public string awsSecretKey = "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";
    public string service = "execute-api";
    public string region = "ap-southeast-1";
    
    public string apiUrl = "https://loki.airasiabig.com/v1/external/accrual/activity";

    public string memberID = "9999990005183161";
    public string partnerCode = "TBDSTGACT1"; 
    public string transactionDate = "202308180000"; 
    public int points = 1;
    public string description = "Test Accruals"; 
    public string referenceNumber = "ABCDEFG1234"; 
    public string partnerMID = "1111000101"; 

    private void Start()
    {
        StartCoroutine(CallAPI());
    }

    private IEnumerator CallAPI()
    {
        // Construct the JSON request body
        string requestBody = "{\"memberID\":\""+ memberID +"\",\"partnerCode\":\""+ partnerCode +"\",\"transactionDate\":\""+ transactionDate +"\",\"points\":\""+ points +"\",\"description\":\""+ description +"\",\"referenceNumber\":\""+ referenceNumber +"\",\"partnerMID\":\""+ partnerMID +"\"}";

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
         byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // Set the content type header
        request.SetRequestHeader("Content-Type", "application/json");

        // Construct and add Authorization header
        string authorizationHeader = GenerateAuthorizationHeader(request.method, apiUrl, bodyRaw);
        request.SetRequestHeader("Authorization", authorizationHeader);

        // Send the API request
        yield return request.SendWebRequest();

        // Handle the response
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("API Request Error: " + request.error);
        }
        else
        {
            Debug.Log("API Response: " + request.downloadHandler.text);
            // Parse the response JSON here
        }
    }


        private string GenerateAuthorizationHeader(string httpMethod, string endpoint, byte[] body)
    {
        // Generate AWS Signature V4
        string algorithm = "AWS4-HMAC-SHA256";
        string amzDate = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ", CultureInfo.InvariantCulture);
        string dateStamp = DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        
        string canonicalUri = "/";
        string canonicalQueryString = "";
        string canonicalHeaders = $"host:{new Uri(endpoint).Host}\nx-amz-date:{amzDate}\n";
        string signedHeaders = "host;x-amz-date";

        string canonicalRequest = $"{httpMethod}\n{canonicalUri}\n{canonicalQueryString}\n{canonicalHeaders}\n{signedHeaders}\n{BitConverter.ToString(body).Replace("-", "").ToLower()}";
        byte[] canonicalRequestBytes = Encoding.UTF8.GetBytes(canonicalRequest);

        using (var sha256 = SHA256.Create())
        {
            byte[] canonicalRequestHashBytes = sha256.ComputeHash(canonicalRequestBytes);
            string canonicalRequestHash = BitConverter.ToString(canonicalRequestHashBytes).Replace("-", "").ToLower();

            string stringToSign = $"{algorithm}\n{amzDate}\n{dateStamp}/{region}/{service}/aws4_request\n{canonicalRequestHash}";

            byte[] signingKey = GetSignatureKey(awsSecretKey, dateStamp, region, service);
            using (var kha = new HMACSHA256(signingKey))
            {
                byte[] signatureBytes = kha.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
                string signature = BitConverter.ToString(signatureBytes).Replace("-", "").ToLower();
                return $"{algorithm} Credential={awsAccessKey}/{dateStamp}/{region}/{service}/aws4_request, SignedHeaders={signedHeaders}, Signature={signature}";
                
            }
        }
    }

    private byte[] GetSignatureKey(string key, string dateStamp, string regionName, string serviceName)
    {
        byte[] kDate = Encoding.UTF8.GetBytes($"AWS4{key}");
        byte[] kRegion = HmacSHA256(kDate, dateStamp);
        byte[] kService = HmacSHA256(kRegion, regionName);
        byte[] kSigning = HmacSHA256(kService, serviceName);
        return kSigning;
    }

    private byte[] HmacSHA256(byte[] key, string data)
    {
        using (HMACSHA256 hmac = new HMACSHA256(key))
        {
            return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        }
    }
}
