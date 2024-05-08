using System;
using System.Collections.Generic;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

public class socketV2 : MonoBehaviour
{
    public SocketIOUnity socket;
    public InputField EventNameText;
    public InputField DataText;

    public string serverUrl = "http://127.0.0.1:5000";

    private DateTime requestTime;

    public Mapper mapper;
    private void Start() 
    {
        ConnectToServer();

    }

    private void OnApplicationQuit() 
    {
        DisconnectFromServer();    
    }


    void ConnectToServer()
    {
        Uri uri = new Uri(serverUrl);
        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Query = new Dictionary<string, string>
            {
                {"token", "UNITY"}
            },
            EIO = 4,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });
        socket.OnConnected += OnConnected;
        socket.OnDisconnected += OnDisconnected;
        socket.On("re_joints", OnJointsReceived);

        socket.Connect();
    }
    private void DisconnectFromServer()
    {
        if (socket != null)
        {
            socket.Disconnect();
        }
    }

    public void RequestMotion(string text)
    {
        // 发送请求动作事件
        JObject data = new JObject();
        data["text"] = text;
        Debug.Log(data);
        string json = data.ToString();
 
        socket.Emit("ask_motion", json);       
        requestTime = DateTime.UtcNow;

        Debug.Log("Requesting motion send at: " + requestTime.ToString("HH:mm:ss.fff"));
    }
    public void SendData()
    {
        // 发送自定义事件

        socket.Emit("test", "qwq");
    }
    private void OnConnected(object sender, EventArgs e)
    {
        Debug.Log("Connected to Socket.IO Server!");
        // 连接成功后的操作，例如发送一些初始化数据等
    }

    private void OnDisconnected(object sender, string e)
    {
        Debug.Log("Disconnected from Socket.IO Server: " + e);
    }

    private void OnJointsReceived(SocketIOResponse response)
    {
        DateTime responseTime = DateTime.UtcNow;
        // 当收到从服务器发来的数据时的处理
        Debug.Log("Received data from server: " + response.ToString());
        // 这里可以添加处理数据的逻辑，比如更新UI，控制游戏对象等
        TimeSpan duration = responseTime - requestTime; // 计算请求响应时间
        Debug.Log("Received data from server at: " + responseTime.ToString("HH:mm:ss.fff"));
        Debug.Log("Time taken for request-response: " + duration.TotalMilliseconds + " ms");
        Debug.Log("Received data from server: " + response.ToString());
        Debug.Log("ready to send message to Mapper.cs");
        mapper.OnMotionReceived(response.ToString());

    }
}
