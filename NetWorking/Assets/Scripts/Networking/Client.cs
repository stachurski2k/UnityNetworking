using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System;
public static class Client
{
    public static bool isConnected=false;
    public static Dictionary<int,Action<Packet>> packetHandlers=new Dictionary<int, Action<Packet>>();
    public static event Action OnServerDisconnected;
    public static event Action<bool> OnServerConnected;
    public static int id;
    public static void ConnectToServer(string ip,int port=23000){
        tcp=new TCP();
        InitData();
        tcp.Connect(ip,port);
    }
    static void InitData(){
        packetHandlers.Clear();
        packetHandlers.Add((int)ServerPackets.Welcome,ClientHandle.HandleWelcome);
        packetHandlers.Add((int)ServerPackets.SpawnPlayer,ClientHandle.SpawnPlayer);
        packetHandlers.Add((int)ServerPackets.DespawnPlayer,ClientHandle.DespawnPlayer);
    }
    public static TCP tcp;
    #region  Tcp
    public class TCP{
        public TcpClient socket;
        NetworkStream stream;
        Packet receivePacket;
        byte[] receiveBuff;
        public void Connect(string ip,int port=23000){
            socket=new TcpClient();
            receivePacket=new Packet();
            socket.SendBufferSize=NetworkManager.bufferSize;
            socket.ReceiveBufferSize=NetworkManager.bufferSize;
            ThreadManager.ExecuteOnNewThread(()=>{
                try
                {
                    socket.Connect(ip,port);
                }
                catch (SocketException)
                {
                }
            },StartListening);
        }
        void StartListening(){
            InvokeOnServerConnected(socket.Connected);
            if(!socket.Connected){return;}
            isConnected=true;
            ThreadManager.ExecuteOnNewThread(()=>{
                receiveBuff=new byte[NetworkManager.bufferSize];
                stream=socket.GetStream();
                while(isConnected){
                    try
                    {
                        int bytesReceived=stream.Read(receiveBuff,0,NetworkManager.bufferSize);
                        if(bytesReceived==0){
                            Client.Disconnect();
                            break;
                        }
                        byte[] data=new byte[bytesReceived];
                        Array.Copy(receiveBuff,data,bytesReceived);
                        receivePacket.Reset(HandleData(data));
                    }
                    catch (System.Exception)
                    {
                        break;
                        throw;
                    }
                }
            });
        }
        bool HandleData(byte[] bytes){
            int packetLength = 0;
            receivePacket.SetBytes(bytes);
            if (receivePacket.UnreadLength() >= 4)
            {
                packetLength = receivePacket.ReadInt();
                if (packetLength <= 0)
                {
                    return true;
                }
            }
            while (packetLength > 0 && packetLength <= receivePacket.UnreadLength())
            {
                byte[] packetBytes = receivePacket.ReadBytes(packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet packet = new Packet(packetBytes))
                    {
                        int packedId = packet.ReadInt();
                        Client.packetHandlers[packedId](packet);
                    }
                });
                packetLength = 0;
                if (receivePacket.UnreadLength() >= 4)
                {
                    packetLength = receivePacket.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }
            }
            if (packetLength <= 1)
            {
                return true;
            }
            return false;
        }
        public void SendData(Packet packet){
            try
            {
                Sender.AddTcpData(socket,packet.ToArray());
            }
            catch (System.Exception)
            {
                throw;
            }
        }
        public void Disconnect(){
            if(socket==null)return;
            //socket.Client.Disconnect(false);
            socket.Client.Close();
            socket=null;
            stream=null;
        }
    }
    #endregion
    public static void Disconnect(){
        if(!isConnected)return;
        isConnected=false;
        tcp.Disconnect();
        ThreadManager.ExecuteOnMainThread(()=>{
            InvokeOnServerDisconnected();
        });
    }
    #region Invokers
    static void InvokeOnServerConnected(bool succes){
        if(OnServerConnected!=null){
            OnServerConnected(succes);
        }
    }
    static void InvokeOnServerDisconnected(){
        if(OnServerDisconnected!=null){
            OnServerDisconnected();
        }
    }
    #endregion
}
