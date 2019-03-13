using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class SocketServer : MonoBehaviour
{
    #region --变量定义
    public string IPAdress = "127.0.0.1";
    public int port = 1234;
    public AddressFamily addressFamily = AddressFamily.InterNetwork;
    public SocketType socketType = SocketType.Stream;
    public ProtocolType protocolType = ProtocolType.Tcp;

    private byte[] data = new byte[1024];
    private Socket serverSocket;//服务器Socket
    private Socket client;//客户端Socket
    private Thread myThread;//启动监听线程
    private Thread receiveThread;//接收数据线程

    private List<Socket> clientSocketList = new List<Socket>();
    #endregion

    #region --系统函数
    private void Start()
    {
        InitSocket();
    }
    void OnDestroy()
    {

        try
        {
            //关闭线程
            if (myThread != null || receiveThread != null)
            {
                myThread.Interrupt();
                myThread.Abort();

                receiveThread.Interrupt();
                receiveThread.Abort();

            }
            //最后关闭socket
            if (serverSocket != null)
            {
                for (int i = 0; i < clientSocketList.Count; i++)
                {
                    clientSocketList[i].Close();
                }

                serverSocket.Shutdown(SocketShutdown.Both);
                serverSocket.Close();

            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }
        print("disconnect");
    }
    #endregion

    #region --自定义函数
    private void InitSocket()
    {
        try
        {
            serverSocket = new Socket(addressFamily, socketType, protocolType);
            IPEndPoint iPPoint = new IPEndPoint(IPAddress.Parse(IPAdress), port);
            serverSocket.Bind(iPPoint);
            serverSocket.Listen(0); //开启监听,等待客户端连接。参数backlog指定队列中最多可容纳等待接受的连接数， 0表示不限制。
            myThread = new Thread(ListenClientConnect);
            myThread.Start();
            Debug.Log("Server is Running...");
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
    private void ListenClientConnect()
    {
        while (true)
        {
            client = serverSocket.Accept();
            clientSocketList.Add(client);

            Debug.Log("客户端：" + client.RemoteEndPoint + "连接到服务器！");
            AllSendMs("From Server:" + client.RemoteEndPoint + "客户端已连接到服务器！");

            receiveThread = new Thread(ReceiveMsg);
            receiveThread.Start(client);

        }
    }

    void ReceiveMsg(object clientSocket)
    {
        client = clientSocket as Socket;
        while (true)
        {
            try
            {
                int lenght = 0;
                lenght = client.Receive(data);

                if (lenght == 0 || client.Poll(100, SelectMode.SelectRead))
                {
                    string s = "客户端：" + client.RemoteEndPoint + "断开了连接！";
                    Debug.Log(s);
                    AllSendMs(s);
                    clientSocketList.Remove(client);
                    break;
                }

                string str = Encoding.UTF8.GetString(data, 0, data.Length);
                Debug.Log(str);
                AllSendMs(str);
            }
            catch (System.Exception ex)
            {
                Debug.Log("从服务器获取数据错误" + ex.Message);
            }
        }
    }
    void AllSendMs(string ms)
    {
        for (int i = 0; i < clientSocketList.Count; i++)
        {
            clientSocketList[i].Send(Encoding.UTF8.GetBytes(ms));
        }
    }
    void AllSendMs(object obj)
    {
        for (int i = 0; i < clientSocketList.Count; i++)
        {
            //data = obj.SerializeToByteArray();
            clientSocketList[i].Send(data, data.Length, 0);
        }
    }

    public  string GetLocalIP()
    {
        try
        {
            string HostName = Dns.GetHostName(); //得到主机名
            IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
            for (int i = 0; i < IpEntry.AddressList.Length; i++)
            {
                //从IP地址列表中筛选出IPv4类型的IP地址
                //AddressFamily.InterNetwork表示此IP为IPv4,
                //AddressFamily.InterNetworkV6表示此地址为IPv6类型
                if (IpEntry.AddressList[i].AddressFamily == addressFamily)
                {
                    return IpEntry.AddressList[i].ToString();
                }
            }
            return "";
        }
        catch (Exception ex)
        {
            Debug.Log("获取本机IP出错:" + ex.Message);
            return "";
        }
    }
    #endregion
}
