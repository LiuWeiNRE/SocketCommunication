using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class SendPhoto : MonoBehaviour
{
    public string serverIP = "127.0.0.1";
    public int TcpPort = 1234;

    private Socket socket = null;
    private IPEndPoint endPoint = null;

    void Start()
    {
        InitSocketEnv();
        SendMegEvent();
    }

    public void SendMegEvent()
    {
        SendPhotoMessage(Application.streamingAssetsPath + "/11.jpg");
        //StartCoroutine(GetScoreImage(new Rect(0, 0, 1920, 1080)));
    }

    void SendPhotoMessage(string fileName)
    {
        //byte[] buffer = ReadImg(fileName); //null

        FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Read);
        BinaryReader strread = new BinaryReader(fs);
        byte[] byt = new byte[fs.Length];
        Debug.Log(byt.Length);
        strread.Read(byt, 0, byt.Length - 1);

        //byte[] size = new byte[4];
        //size = BitConverter.GetBytes(byt.Length);

        socket.Send(byt);
        //socket.Send(size);

        fs.Close();
        socket.Close();
    }

    byte[] ReadImg(string fileName)
    {
        FileInfo fileInfo = new FileInfo(fileName);
        byte[] buffer = new byte[fileInfo.Length];
        using (FileStream fs = fileInfo.OpenRead())
        {
            fs.Read(buffer, 0, buffer.Length);
        }

        return buffer;
    }

    void InitSocketEnv()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        endPoint = new IPEndPoint(IPAddress.Parse(serverIP), TcpPort);
        socket.Connect(endPoint);
    }

    void OnDestroy()
    {
        socket.Close();
    }

    private IEnumerator GetScoreImage(Rect _rect)
    {
        if (_rect.width > Screen.width || _rect.height > Screen.height)
        {
            Debug.LogError(_rect + "超出屏幕大小，无法截图");
            yield break;
        }

        yield return new WaitForEndOfFrame();

        Texture2D tex = new Texture2D((int)_rect.width, (int)_rect.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(_rect, 0, 0, false);
        tex.Apply();

        byte[] _data = tex.GetRawTextureData();

        //Texture2D texture = new Texture2D(1920, 1080, TextureFormat.RGBA32, false);
        //texture.LoadRawTextureData(_data);
        //texture.Apply();

        //socket.Send(_data);
        //socket.Send(size);

        //socket.Close();
        SocketHelper.SendVarData(socket, _data);
    }
}
