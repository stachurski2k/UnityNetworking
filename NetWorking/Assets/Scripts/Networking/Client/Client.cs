using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
public static class Client
{
    public static bool isConnected=false;
    public static Dictionary<int,Action<Packet>> packetHandlers=new Dictionary<int, Action<Packet>>();
    public static event Action OnServerDisconnected;
    public static event Action<bool> OnServerConnected;
    public static int id;
    public static TCP tcp;
    public static UDP udp;
    public static bool isConnecting{get; private set;}=false;
    static string desiredIp;
    public static void ConnectToServer(string ip,int port=23000){
        isConnecting=true;
        desiredIp=(string.IsNullOrEmpty(ip))?"127.0.0.1":ip;
        tcp=new TCP();
        udp=new UDP();
        InitData();
        tcp.Connect(desiredIp,port);
    }
    static void InitData(){
        packetHandlers.Clear();
        packetHandlers.Add((int)ServerPackets.Welcome,ClientHandle.HandleWelcome);
        packetHandlers.Add((int)ServerPackets.SpawnPlayer,ClientHandle.SpawnPlayer);
        packetHandlers.Add((int)ServerPackets.DespawnPlayer,ClientHandle.DespawnPlayer);
        packetHandlers.Add((int)ServerPackets.StartGame,ClientHandle.StartGame);
        packetHandlers.Add((int)ServerPackets.DebugMsg,ClientHandle.HandleDebugMsg);
        packetHandlers.Add((int)ServerPackets.Spawn,ClientHandle.Spawn);
        packetHandlers.Add((int)ServerPackets.DestroyID,ClientHandle.DestroyID);
        packetHandlers.Add((int)ServerPackets.ExecuteFun,ClientHandle.HandleExecuteFun);
    }
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
                    IPHostEntry ipEntry=Dns.GetHostEntry(Dns.GetHostName());
                    IPAddress[] addr = ipEntry.AddressList;

                    for (int i = 0; i < addr.Length; i++)
                    {
                        Debug.Log(addr[i].ToString());
                    }

                    var endPoint=Dns.GetHostByAddress(ip);
                    Debug.Log(endPoint.AddressList[0]);
                    bool isIP=IPAddress.TryParse(ip,out IPAddress _ip);
                    if(true)
                    {
                        socket.Connect(endPoint.AddressList[0],port);
                    }else{
                        socket.Connect(ip,port);
                    }
                }
                catch (SocketException)
                {
                }
            },StartListening);
        }
        void StartListening(){
            isConnecting=false;
            isConnected=socket.Connected;
            InvokeOnServerConnected(isConnected);
            if(!isConnected){return;}
            Sender.StartTcp();
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
            if(socket!=null)
            Sender.AddTcpData(socket,packet.ToArray());
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
    public class UDP{
        UdpClient socket;
        IPEndPoint endPoint;
        public UDP(){
            endPoint=new IPEndPoint(IPAddress.Parse(desiredIp),23000);
        }
        public void Connect(int port){
            socket=new UdpClient(port);
            Sender.StartUdp(socket);
            ThreadManager.ExecuteOnNewThread(StartListening);
            using(Packet packet = new Packet()){
                SendData(packet);
            }
        }
        void StartListening(){
            while(isConnected){
                try
                {
                    byte[] receiveBuff=socket.Receive(ref endPoint);
                    if(receiveBuff.Length<=4){
                        Client.Disconnect();
                    }
                    HandleData(receiveBuff);
                }
                catch (System.Exception)
                {
                    break;
                }
            }
        }
        void HandleData(byte[] data){
            ThreadManager.ExecuteOnMainThread(()=>
            {
                using(Packet packet=new Packet(data)){
                    int packetLength=packet.ReadInt();
                    data=packet.ReadBytes(packetLength);
                }
                using(Packet packet = new Packet(data)){
                    int packetId=packet.ReadInt();
                    packetHandlers[packetId](packet);
                }
            });
        }
        public void SendData(Packet data){
            if(socket==null)return;
            
            data.InsertInt(id);
            Sender.AddUdpData(endPoint,data.ToArray());
        }
        public void Disconnect(){
            endPoint=null;
        }
    }
    public static void Disconnect(){
        if(!isConnected)return;
        isConnected=false;
        tcp.Disconnect();
        tcp.Disconnect();
        Sender.Stop();
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
