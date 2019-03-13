/*
 * 版本号：
 * Modify：修改日期
 * Modifier：刘伟
 * Modify Reason：修改原因
 * Modify Content：修改内容说明
*/
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class SocketClient : MonoBehaviour 
{
    #region -- 变量定义
    public string IPAdress;
    public int port;

    private byte[] data = new byte[1024];
    private Socket clientSocket;
    private Thread receiveT;
    #endregion

    #region -- 系统函数
    void Start()
    {
        ConnectToServer();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SendMes("我是客户端");
        }
    }
    void OnDestroy()
    {
        try
        {
            if (clientSocket != null)
            {
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();//关闭连接
            }

            if (receiveT != null)
            {
                receiveT.Interrupt();
                receiveT.Abort();
            }

        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
    #endregion

    #region -- 自定义函数
    void ConnectToServer()
    {
        try
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(IPAddress.Parse(IPAdress), port);
            Debug.Log("连接服务器成功");
            receiveT = new Thread(ReceiveMsg);
            receiveT.Start();

        }
        catch (System.Exception ex)
        {
            Debug.Log("连接服务器失败！");
            Debug.Log(ex.Message);
        }
    }
    private void ReceiveMsg()
    {
        while (true)
        {
            if (clientSocket.Connected == false)
            {
                Debug.Log("与服务器断开了连接");
                break;
            }

            int lenght = 0;
            lenght = clientSocket.Receive(data);

            string str = Encoding.UTF8.GetString(data, 0, data.Length);
            Debug.Log(str);
        }
    }
    private void SendMes(string ms)
    {
        byte[] data = new byte[1024];
        data = Encoding.UTF8.GetBytes(ms);
        clientSocket.Send(data);
    }

    private void SendMes(object obj)
    {
        byte[] data = new byte[1024];
        //data = obj.SerializeToByteArray();
        clientSocket.Send(data, data.Length, 0);
    }
    #endregion
}
