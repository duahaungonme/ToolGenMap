using System.Collections;
using System.Collections.Generic;
using Pixelplacement;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Networking;

public class APIHandler : Singleton<APIHandler>
{
    [Button]
    public void Upload(string url, string body)
    {
        StartCoroutine(IUpload());

        IEnumerator IUpload()
        {
            UnityWebRequest request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = new System.Text.UTF8Encoding(true).GetBytes(body);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                NoticePanel.Instance.Init(request.error);
            }
            else
            {
                NoticePanel.Instance.Init(request.downloadHandler.text);
                Debug.Log(request.downloadHandler.text);
            }
        }
    }
}