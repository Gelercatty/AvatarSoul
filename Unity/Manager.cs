using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using myHTTP;
using System.Net.Http;
using System.Xml;
using UnityEngine.UI;
using UnityEngine.Networking;  // 引入UI命名空间用于操作Text组件

public class Manager : MonoBehaviour
{
    public string speechApi = "http://";
    public string llmApi = "http://43.153.116.88:23344/openai/?msg=";

    public AudioController audioController;
    public socketV2 motionSocket;
    private HTTP httpClient;

    public UImanager uiManager;

    public Mapper mapper;
    private void Start()
    {
        httpClient = gameObject.AddComponent<HTTP>();  // 确保HTTP组件已被添加到GameObject
    }

    private async void GetLLMResponse(string msg)
    {
        string url = llmApi + msg+"&userid=Demo2";
        string response = await httpClient.GetAsync(url);
        // string response = "<chat>Debuging the motion mapper...</chat><motion>QWQ</motion>";
        Debug.Log("LLM Response: " + response);
        ProcessLLMResponse(response);
    }


    private void ProcessAudioResponse(string audioData)
    {

    }
    private void ProcessLLMResponse(string xmlData)
    {
        string chatText = ExtractValueFromTag(xmlData, "chat");
        string motionText = ExtractValueFromTag(xmlData, "motion");

        if (!string.IsNullOrEmpty(chatText))
        {
            audioController.PlayAudioFromText(chatText); // 语音播报
            uiManager.AddText("Avatar:"+ chatText); // 显示聊天内容
        }

        if (!string.IsNullOrEmpty(motionText))
        {
            motionSocket.RequestMotion(motionText); // 请求播放动作
        }
    }

private string ExtractValueFromTag(string xml, string tagName)
{
    string startTag = $"<{tagName}>";
    string endTag = $"</{tagName}>";

    int startIndex = xml.IndexOf(startTag) + startTag.Length;
    int endIndex = xml.IndexOf(endTag);

    // 确保找到了开始和结束标签
    if (startIndex < startTag.Length || endIndex == -1)
    {
        Debug.LogError($"Tag {tagName} not found or malformed XML.");
        return string.Empty;
    }

    return xml.Substring(startIndex, endIndex - startIndex);
}
    // private void ProcessLLMResponse(string xmlData)
    // {
    //     // XmlDocument doc = new XmlDocument();
    //     // doc.LoadXml(xmlData);
    //     // string chatText = doc.SelectSingleNode("//chat")?.InnerText;
    //     // string motionText = doc.SelectSingleNode("//motion")?.InnerText;

    //     // if (!string.IsNullOrEmpty(chatText))
    //     // {
    //     //     Debug.Log("Chat: " + chatText);
    //     //     uiManager.AddText(chatText);
    //     // }

    //     // if (!string.IsNullOrEmpty(motionText))
    //     // {
    //     //     Debug.Log("Motion: " + motionText);
    //     //     motionSocket.RequestMotion(motionText);
    //     // }

    //     string wellFormedXml = "<root>" + xmlData + "</root>";

    // try
    // {
    //     XmlDocument doc = new XmlDocument();
    //     doc.LoadXml(wellFormedXml); // 使用修正后的XML字符串

    //     XmlNode chatText = doc.SelectSingleNode("//chat");
    //     XmlNode motionText = doc.SelectSingleNode("//motion");

    //     if (chatText != null)
    //     {
            
    //         Debug.Log("Chat: " + chatText);
    //         uiManager.AddText(chatText.InnerText);
    //     }

    //     if (motionText != null)
    //     {

    //         Debug.Log("Motion: " + motionText);
    //         motionSocket.RequestMotion(motionText.InnerText);
    //     }
    // }
    // catch (XmlException e)
    // {
    //     Debug.LogError("XML Error: " + e.Message);
    // }

        
    // }

    public void GotUserInput(string input)
    {
        GetLLMResponse(input);
    }

    public void ReapeatMotion()
    {
        motionSocket.mapper.RepeatMotion();
    }
}
