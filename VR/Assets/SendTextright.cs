using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
public class SendTextright : MonoBehaviour
{

    Thread connectThread;//当前服务端监听子线程
    public string address;//当前地址
    public int port;//当前本地端口
    TcpClient romoteClient;//远程客户端
    private int i = 0;

    // Use this for initialization
    void Start()
    {
        connectThread = new Thread(InitServerSocket);
        connectThread.Start();
    }

    /// <summary>
    /// 实例化服务端Socket
    /// </summary>
    public void InitServerSocket()
    {
        int bufferSize = 8192;//缓冲区大小
        IPAddress ip = IPAddress.Parse(address);
        //新建TCP连接，并开启监听子线程
        TcpListener tcpListener = new TcpListener(ip, port);
        tcpListener.Start();
        Debug.Log("服务端-->客户端完成,开启tcp连接监听");
        //如果有远程客户端连接，此时得到其对象用于通讯
        romoteClient = tcpListener.AcceptTcpClient();
        Debug.Log("客户端连接开始 本地地址端口: " + romoteClient.Client.LocalEndPoint + "  远程客户端地址端口: " + romoteClient.Client.RemoteEndPoint);
        NetworkStream stream = romoteClient.GetStream();
        do
        {
            try
            {
                //获取与客户端连接数据
                byte[] buffer = new byte[bufferSize];
                int byteRead = stream.Read(buffer, 0, bufferSize);
                if (byteRead == 0)
                {
                    Debug.Log("客户端断开");
                    break;
                }
                string msg = Encoding.UTF8.GetString(buffer, 0, byteRead);
                Debug.Log("接收到客户端的数据: " + msg + "   数据长度: " + byteRead + "字节");

            }
            catch (System.Exception ex)
            {
                Debug.Log("客户端异常: " + ex.Message);
                //客户端出现异常或者断开的时候，关闭线程防止溢出
                tcpListener.Stop();
                break;
            }
        } while (true);
    }

    /// <summary>
    /// 服务器端根据当前连接的远程客户端发送消息
    /// </summary>
    public void SendMessageToClient(string message)
    {
        if (romoteClient != null)
        {
            romoteClient.Client.Send(Encoding.UTF8.GetBytes(message));
            Debug.Log(message);

            //romoteClient.Client.Send(Encoding.UTF8.GetBytes("Hello Client ,This is Server!"));
        }
    }

    /// <summary>
    /// 销毁时关闭监听线程及连接
    /// </summary>
    void OnDestroy()
    {
        if (romoteClient != null)
            romoteClient.Close();
        if (connectThread != null)
            connectThread.Abort();
    }


    //对被跟踪的对象建立索引，此处指手柄。
    private SteamVR_TrackedObject trackedObj;
    //通过跟踪对象的索引访问控制器的输入
    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }
    //当脚本被加载时，关联此手柄
    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    void Update()
    {
        //   string message = System.Math.Ceiling(gameObject.transform.position.x) + "_" + System.Math.Ceiling(gameObject.transform.position.y) + "_" + System.Math.Ceiling(gameObject.transform.position.z)
        //      + "_" + System.Math.Ceiling((-gameObject.GetComponent<Transform>().localEulerAngles.x)) + "_" + System.Math.Ceiling(gameObject.GetComponent<Transform>().localEulerAngles.y) + "_" + System.Math.Ceiling(-gameObject.GetComponent<Transform>().localEulerAngles.z);
        string message= i + "_" + i + "_" +i + "_"+ i + "_"+ i + "_"+ i;
        i += 1;
        //Trigger扳机
        if (Controller.GetHairTriggerDown())
          {
            message = null;
         message = "True";
    }
        SendMessageToClient(message);
     
        Debug.Log("SENDok!");
        message = null;
        
    }

}
