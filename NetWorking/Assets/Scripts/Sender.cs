using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using System.Threading;
public class Sender : MonoBehaviour
{
    public static ConcurrentQueue<TcpSendData> tcpData;
    public static ConcurrentQueue<UdpSendData> udpData;
    static UdpClient udpSender;
    //static Thread udpThread,tcpThread;
    static bool isTcpWorking,isUdpWorking;
    static ManualResetEvent tcpSendEvent=new ManualResetEvent(false),udpSendEvent=new ManualResetEvent(false);
    public static void AddTcpData(TcpClient client,byte[] packet){
        tcpData.Enqueue(new TcpSendData(client,packet));
        tcpSendEvent.Set();
    }
    public static void AddUdpData(IPEndPoint endPoint,byte[] packet){
        udpData.Enqueue(new UdpSendData(endPoint,packet));
        udpSendEvent.Set();
    }
    public static void StartTcp(){
        if(isTcpWorking){
            return;
        }
        isTcpWorking=true;
        tcpData=new ConcurrentQueue<TcpSendData>();
        ThreadManager.ExecuteOnNewThread(SendTcpData);
    }
    public static void StartUdp(UdpClient sender){
        if(isUdpWorking){
            return;
        }
        isUdpWorking=true;
        udpSender=sender;
        udpData=new ConcurrentQueue<UdpSendData>();
        ThreadManager.ExecuteOnNewThread(SendUdpData);
    }
    public static void Stop(){
        tcpSendEvent.Set();
        udpSendEvent.Set();
        // udpThread.Resume();
        // tcpThread.Resume();
    }
    static void SendTcpData(){
        var data=new TcpSendData(null,null);
        byte[] bytes;
        while(true){
            try
            {
                tcpSendEvent.WaitOne();
                var isData=tcpData.TryDequeue(out data);
                if(isData){
                    Debug.Log("sending!");
                    bytes=data.packet;
                    data.client.GetStream().Write(bytes,0,bytes.Length);
                }else{
                    tcpSendEvent.Reset();
                }
                if(!NetworkManager.isWorking()){
                    break;
                }
            }
            catch (System.Exception)
            {
                if(!NetworkManager.isWorking()){
                    isTcpWorking=false;
                    break;
                }
            }
            //tcpThread.Suspend();
        }
        print("ending");

    }
    static void SendUdpData(){
        var data=new UdpSendData(null,null);
        byte[] bytes;
        while(true){
            try
            {
                udpSendEvent.WaitOne();
                var isData=udpData.TryDequeue(out data);
                if(isData){
                    bytes=data.packet;
                    udpSender.Send(bytes,bytes.Length,data.endPoint);
                }else{
                    udpSendEvent.Reset();
                }
                 if(!NetworkManager.isWorking()){
                    break;
                }
            }
            catch (System.Exception)
            {
                if(!NetworkManager.isWorking()){
                    isUdpWorking=false;
                    break;
                }
            }
            //udpThread.Suspend();
        }
        print("ending");
    }
}
public struct TcpSendData{
    public TcpClient client;
    public byte[] packet;
    public TcpSendData(TcpClient client, byte[] packet){
        this.client=client;
        this.packet=packet;
    }
}
public struct UdpSendData{
    public IPEndPoint endPoint;
    public byte[] packet;
    public UdpSendData(IPEndPoint endPoint, byte[] packet){
        this.endPoint=endPoint;
        this.packet=packet;
    }
}
