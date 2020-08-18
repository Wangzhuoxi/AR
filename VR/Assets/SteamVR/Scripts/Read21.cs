using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class Read21 : MonoBehaviour
{
    public string serverAddress;//服务器地址
    public int port;//服务器端口
    private TcpClient localClient;//当前tcp客户端
    private Thread receiveThread;//接收服务器消息线程
    private byte[] resultBuffer = new byte[1024];//服务器返回流字节
    private string resultStr;//服务器返回字符串

    void Start()
    {
        //连接至服务端
        InitClientSocket();
    }

    /// <summary>
    /// 销毁时操作
    /// </summary>
    private void OnDestroy()
    {
        if (localClient != null)
            localClient.Close();
        if (receiveThread != null)
            receiveThread.Abort();
    }

    /// <summary>
    /// 客户端实例化Socket连接
    /// </summary>
    private void InitClientSocket()
    {
        localClient = new TcpClient();
        try
        {
            //当前客户端连接的服务器地址与远程端口
            localClient.Connect(IPAddress.Parse(serverAddress), port);
            //开始接收服务器消息子线程
            receiveThread = new Thread(SocketReceiver);
            receiveThread.Start();
            Debug.Log("客户端-->服务端完成,开启接收消息线程");
        }
        catch (System.Exception ex)
        {
            Debug.Log("客户端连接服务器异常: " + ex.Message);
        }
        Debug.Log("连接到服务器 本地地址端口:" + localClient.Client.LocalEndPoint + "  远程服务器端口:" + localClient.Client.RemoteEndPoint);
    }

    /// <summary>
    /// 客户端发送消息到服务器
    /// </summary>
    private void SendMessageToServer()
    {
        try
        {
            string clientStr = "Hello Server, This is Client!";
            //获取当前客户端的流对象，然后将要发送的字符串转化为byte[]写入发送
            NetworkStream stream = localClient.GetStream();
            byte[] buffer = Encoding.UTF8.GetBytes(clientStr);
            stream.Write(buffer, 0, buffer.Length);
        }
        catch (System.Exception ex)
        {
            Debug.Log("发送消息时服务器产生异常: " + ex.Message);
        }
    }

    /// <summary>
    /// 客户端检测收到服务器信息子线程
    /// </summary>
    private void SocketReceiver()
    {
        if (localClient != null)
        {
            while (true)
            {
                if (localClient.Client.Connected == false)
                    break;
                //在循环中，
                localClient.Client.Receive(resultBuffer);
                resultStr = Encoding.UTF8.GetString(resultBuffer);
                Debug.Log("客户端收到服务器消息 : " + resultStr);

                string[] data = resultStr.Split('_');   //字符串分割,把一个整体字符串分割出具有意义的6个坐标值
                Debug.Log("客户端收到服务器消息 : " + data[0]);
                Debug.Log("客户端收到服务器消息 : " + data[1]);
                Debug.Log("客户端收到服务器消息 : " + data[2]);

                Debug.Log("客户端收到服务器消息 : " + data[3]);
                Debug.Log("客户端收到服务器消息 : " + data[4]);
                Debug.Log("客户端收到服务器消息 : " + data[5]);



            }
        }
    }
    void Update()
    {
        localClient.Client.Receive(resultBuffer);
        resultStr = Encoding.UTF8.GetString(resultBuffer);
        string[] data = resultStr.Split('_');
        transform.position = new Vector3(float.Parse(data[0]), float.Parse(data[1]), float.Parse(data[2]));
        gameObject.GetComponent<Transform>().localEulerAngles = new Vector3(float.Parse(data[3]), float.Parse(data[4]), float.Parse(data[5]));
    }
}