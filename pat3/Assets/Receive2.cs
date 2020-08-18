using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if NETFX_CORE
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#endif

public class Receive2 : MonoBehaviour {
    public GameObject ball;
#if NETFX_CORE
    private DataReader reader;
    private string message;
    private StreamSocket streamSocket;
    
#endif

    void Start ()
    {
#if NETFX_CORE
        StartClient();//开启
#endif
    }
#if NETFX_CORE
    private async void StartClient()
    {
        try {
            // 建立连接（这里是电脑的IP和端口）
            streamSocket = new Windows.Networking.Sockets.StreamSocket();
            var hostName = new Windows.Networking.HostName("172.20.10.2");
            await streamSocket.ConnectAsync(hostName, "8888");
            Debug.Log("========connected==========");
            
            // 读取数据流
            reader = new DataReader(streamSocket.InputStream);
     while (true){

            reader.InputStreamOptions = InputStreamOptions.Partial;
            await reader.LoadAsync(1024);
            message = reader.ReadString(reader.UnconsumedBufferLength);
            Debug.Log(message);
            if(message.Contains("True"))
    {
    ball.GetComponent<Rigidbody>().useGravity = false;
        ball.GetComponent<Rigidbody>().velocity = new Vector3(0.0f, 0.0f, 0.0f);
        float ballx = this.transform.position.x;
        float bally = this.transform.position.y + 0.5f;
        float ballz = this.transform.position.z;
        ball.transform.position = new Vector3(ballx, bally, ballz);
        ball.GetComponent<Rigidbody>().useGravity = true;
    message="0";
        continue;
           }

            string[] data = message.Split('_');
    transform.position = new Vector3(-int.Parse(data[0]), int.Parse(data[1]), int.Parse(data[2]));
          gameObject.GetComponent<Transform>().localEulerAngles = new Vector3(int.Parse(data[3]),int.Parse(data[4])+180, int.Parse(data[5]));
        Debug.Log("pai2.position:"+transform.position);
           }
        }
        catch (Exception ex) {
            Debug.Log(ex);
        }
        
    }
#endif

    void Update ()
    {
        
    }
}
