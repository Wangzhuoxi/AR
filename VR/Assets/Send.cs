using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

public class Send : MonoBehaviour
{

    Thread connectThread;   //当前服务端监听子线程
    private string address="172.20.10.2";  //当前地址（电脑）
    private int port = 8888;        //当前本地端口（电脑）
    TcpClient romoteClient; //远程客户端
    string message;

    void Start()
    {
        connectThread = new Thread(InitServerSocket);
        connectThread.Start();
    }
    private SteamVR_TrackedObject trackedObj;
    void Awake()
    {

        Application.targetFrameRate = 75;
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }
    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }
    // 实例化服务端Socket
    public void InitServerSocket()
    {
        IPAddress ip = IPAddress.Parse(address);
        //新建TCP连接，并开启监听子线程
        TcpListener tcpListener = new TcpListener(ip, port);
        tcpListener.Start();
        Debug.Log("服务端-->客户端完成,开启tcp连接监听");
        //如果有远程客户端连接，此时得到其对象用于通讯
        romoteClient = tcpListener.AcceptTcpClient();
        Debug.Log("客户端连接开始 本地地址端口: " + romoteClient.Client.LocalEndPoint + "  远程客户端地址端口: " + romoteClient.Client.RemoteEndPoint);
    }

    // 服务器端根据当前连接的远程客户端发送消息
    public void SendMessageToClient(string message)
    {
        if (romoteClient != null)
        {
            romoteClient.Client.Send(Encoding.UTF8.GetBytes(message));
            Debug.Log("sENDok" + message);
        }
    }

    // 销毁时关闭监听线程及连接
    void OnDestroy()
    {
        if (romoteClient != null)
            romoteClient.Close();
        if (connectThread != null)
            connectThread.Abort();
    }

    void FixedUpdate()
    {
        string message = "0";
        message = System.Math.Ceiling(gameObject.transform.position.x) + "_" + System.Math.Ceiling(gameObject.transform.position.y) + "_" + System.Math.Ceiling(gameObject.transform.position.z)
             + "_" + System.Math.Ceiling((-gameObject.GetComponent<Transform>().localEulerAngles.x)) + "_" + System.Math.Ceiling(gameObject.GetComponent<Transform>().localEulerAngles.y) + "_" + System.Math.Ceiling(-gameObject.GetComponent<Transform>().localEulerAngles.z);


        if (Controller.GetHairTriggerDown())
        {
           
            message = "True";
          
        }
        SendMessageToClient(message);
        
    }

}