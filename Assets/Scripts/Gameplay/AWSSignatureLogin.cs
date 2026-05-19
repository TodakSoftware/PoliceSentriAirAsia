using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class AWSSignatureLogin : MonoBehaviour
{
    // AWS credentials
    private string accessKey = "AKIAWENSX2JPCUBIWPPM";
    private string secretKey = "Jga700QbY7KDBnYSdCGdS7FvWpu+hcBUIzHAo5L3";
    private string region = "ap-southeast-1"; // Change this to your region
    private string serviceName = "execute-api"; // e.g. "execute-api"

    // URL for the login API
    private string apiUrl = "https://loki.airasiabig.com/v1/external/accrual/activity";

    void Start()
    {
        StartCoroutine(SignAndSendRequest());
    }

    IEnumerator SignAndSendRequest()
    {
        string amzDate = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ");
        string dateStamp = DateTime.UtcNow.ToString("yyyyMMdd");

        string canonicalUri = "/";
        string canonicalQuerystring = "";
        string canonicalHeaders = "host:" + region + ".amazonaws.com\nx-amz-date:" + amzDate + "\n";
        string signedHeaders = "host;x-amz-date";
        string payloadHash = "UNSIGNED-PAYLOAD"; // Change if you have a payload

        string canonicalRequest = "GET" + "\n" + canonicalUri + "\n" + canonicalQuerystring + "\n"
                                + canonicalHeaders + "\n" + signedHeaders + "\n" + payloadHash;

        byte[] canonicalRequestBytes = Encoding.UTF8.GetBytes(canonicalRequest);
        byte[] canonicalRequestHashBytes;
        
        using (SHA256 hasher = SHA256.Create())
        {
            canonicalRequestHashBytes = hasher.ComputeHash(canonicalRequestBytes);
        }

        string canonicalRequestHash = BitConverter.ToString(canonicalRequestHashBytes).Replace("-", "").ToLower();

        string stringToSign = "AWS4-HMAC-SHA256" + "\n" + amzDate + "\n" + dateStamp + "/" + region + "/" + serviceName + "/aws4_request" + "\n" + canonicalRequestHash;

        byte[] signingKey = GetSignatureKey(secretKey, dateStamp, region, serviceName);

        byte[] signatureBytes;
        
        using (HMACSHA256 hmacSha256 = new HMACSHA256(signingKey))
        {
            signatureBytes = hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
        }

        string signature = BitConverter.ToString(signatureBytes).Replace("-", "").ToLower();

        string authorizationHeader = "AWS4-HMAC-SHA256 Credential=" + accessKey + "/" + dateStamp + "/" + region + "/" + serviceName + "/aws4_request, "
                                    + "SignedHeaders=" + signedHeaders + ", Signature=" + signature;

        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        request.SetRequestHeader("x-amz-date", amzDate);
        request.SetRequestHeader("Authorization", authorizationHeader);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Request successful: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Request failed: " + request.error);
        }
    }

    byte[] GetSignatureKey(string key, string dateStamp, string regionName, string serviceName)
{
    byte[] kSecret = Encoding.UTF8.GetBytes(("AWS4" + key).ToCharArray());

    using (var kDate = new HMACSHA256(kSecret))
    {
        byte[] kDateBytes = Encoding.UTF8.GetBytes(dateStamp);
        byte[] kDateHash = kDate.ComputeHash(kDateBytes);

        using (var kRegion = new HMACSHA256(kDateHash))
        {
            byte[] kRegionBytes = Encoding.UTF8.GetBytes(regionName);
            byte[] kRegionHash = kRegion.ComputeHash(kRegionBytes);

            using (var kService = new HMACSHA256(kRegionHash))
            {
                byte[] kServiceBytes = Encoding.UTF8.GetBytes(serviceName);
                byte[] kServiceHash = kService.ComputeHash(kServiceBytes);

                using (var kSigning = new HMACSHA256(kServiceHash))
                {
                    byte[] kSigningBytes = Encoding.UTF8.GetBytes("aws4_request");
                    return kSigning.ComputeHash(kSigningBytes);
                }
            }
        }
    }
}


}
