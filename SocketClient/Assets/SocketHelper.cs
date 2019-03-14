using System;
using System.Net.Sockets;
using UnityEngine;

public static class SocketHelper
{
    /// <summary>
    /// 接收变长的数据，要求其打头的4个字节代表有效数据的长度
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static byte[] ReceiveVarData(Socket s)
    {
        if (s == null)
            throw new ArgumentNullException("s");
        int total = 0;  //已接收的字节数
        int recv;
        //接收4个字节，得到“消息长度”
        byte[] datasize = new byte[4];
        recv = s.Receive(datasize, 0, 4, 0);
        int size = BitConverter.ToInt32(datasize, 0);
        //按消息长度接收数据
        int dataleft = size;
        byte[] data = new byte[size];
        while (total < size)
        {
            recv = s.Receive(data, total, dataleft, 0);
            if (recv == 0)
            {
                break;
            }
            total += recv;
            dataleft -= recv;
        }
        return data;
    }

    /// <summary>
    /// 发送变长的数据，将数据长度附加于数据开头
    /// </summary>
    /// <param name="s"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static int SendVarData(Socket s, byte[] data)
    {
        int total = 0;
        int size = data.Length;  //要发送的消息长度
        int dataleft = size;     //剩余的消息
        int sent;
        //将消息长度（int类型）的，转为字节数组
        byte[] datasize = BitConverter.GetBytes(size);
        //将消息长度发送出去
        sent = s.Send(datasize);
        //发送消息剩余的部分
        while (total < size)
        {
            sent = s.Send(data, total, dataleft, SocketFlags.None);
            total += sent;
            dataleft -= sent;
            Debug.Log(dataleft);
        }
        return total;
    }
}