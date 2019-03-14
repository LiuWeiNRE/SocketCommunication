using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class RecivePhoto : MonoBehaviour
{
    public string serverIP = "127.0.0.1";
    public int TcpPort = 1234;
    private Socket m_socket = null;
    private IPEndPoint m_ipEp = null;

    private Thread m_thread = null;
    private bool isRunningThread = false;

    private Queue<byte[]> m_queue;
    public byte[] data;
    public RawImage rawImage;
    public bool isCreate = false;
    public byte[] Data
    {
        set
        {
            data = value;
            isCreate = true;
        }
    }

    // Use this for initialization
    void Start()
    {
        m_queue = new Queue<byte[]>();
        InitSocketEnv();
    }
    
    // Update is called once per frame
    void Update()
    {
        if (isCreate)
        {
            Texture2D texture = new Texture2D(1920, 1080, TextureFormat.RGBA32, false);
            texture.LoadRawTextureData(data);
            texture.Apply();
            rawImage.texture = texture;
            isCreate = false;
        }
        if (m_queue.Count > 0)
        {
            Debug.Log(m_queue.Count);

            byte[] temp = m_queue.Dequeue();

            FileStream fs = File.Create(Application.streamingAssetsPath + "/22.jpg");
            fs.Write(temp, 0, temp.Length);
            fs.Close();
        }


    }

    void ReciveMeg()
    {
        while (isRunningThread)
        {
            Socket socket = m_socket.Accept();

            //获取图片字节流长度
            //byte[] dataSize = new byte[4];
            //int rect = socket.Receive(dataSize, 0, 4, SocketFlags.None);
            //int size = BitConverter.ToInt32(dataSize, 0);

            //Debug.Log(size);

            byte[] buffer = new byte[2000000];
            socket.Receive(buffer, buffer.Length, SocketFlags.None);

            byte[] bufferSize = new byte[4];
            socket.Receive(bufferSize, 0, 4, SocketFlags.None);
            int size = BitConverter.ToInt32(bufferSize, 0);
            Debug.Log(size);

            m_queue.Enqueue(buffer);
            //Data = SocketHelper.ReceiveVarData(socket);
            isRunningThread = false;
            //socket.Close();
            
        }

        Debug.Log("stop");
    }

    void InitSocketEnv()
    {
        m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        m_ipEp = new IPEndPoint(IPAddress.Parse(serverIP), TcpPort);
        m_socket.Bind(m_ipEp);
        m_socket.Listen(5);

        isRunningThread = true;
        m_thread = new Thread(ReciveMeg);
        m_thread.Start();
    }

    private void OnDestroy()
    {
        isRunningThread = false;
        m_socket.Close();
    }
}
