using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
public class ServerClient 
{
    public TCP tcp;
    public UDP udp;
    public int id;
    public ServerClient(int id){
        this.id =id;
        tcp=new TCP(id);
        udp=new UDP(id);
    }
    #region  Tcp
    public class TCP{
        public TcpClient socket;
        NetworkStream stream;
        Packet receivePacket;
        byte[] receivedBuff;
        public int id;
        public TCP(int id){
            this.id=id; 
        }
        public void Connect(TcpClient client){
            socket=client;
            receivePacket=new Packet();
            socket.SendBufferSize=NetworkManager.bufferSize;
            socket.ReceiveBufferSize=NetworkManager.bufferSize;
            receivedBuff=new byte[NetworkManager.bufferSize];
            stream=socket.GetStream();

            ServerSend.SendWelcome(id);
            ThreadManager.ExecuteOnNewThread(StartListening);
        }
        void StartListening(){
            while (NetworkManager.isWorking())
            {
                try
                {
                    int bytesReceived=stream.Read(receivedBuff,0,NetworkManager.bufferSize);
                    if(bytesReceived==0){
                        Server.clients[id].Disconnect();
                        break;
                    }
                    byte[] data=new byte[bytesReceived];
                    Array.Copy(receivedBuff,data,bytesReceived);
                    receivePacket.Reset(HandleData(data));
                }
                catch (System.Exception)
                {
                    break;
                    throw;
                }
            }
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
                        Server.packetHandlers[packedId](id,packet);
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
        public void Send(Packet data){
            if(socket==null)return;
                Sender.AddTcpData(socket,data.ToArray());
           
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
        public IPEndPoint endPoint;
        public int id;
        public UDP(int id){
            this.id=id;
        }
        public void Connect(IPEndPoint endPoint){
            this.endPoint=endPoint;
        }
        public void SendData(Packet data){
            if(endPoint==null)return;
            Sender.AddUdpData(endPoint,data.ToArray());
        }
        public void HandleData(Packet packet){
            int packetLength = packet.ReadInt();
            byte[] data = packet.ReadBytes(packetLength);
            ThreadManager.ExecuteOnMainThread(()=>
            {
                using(Packet _packet=new Packet(data))
                {
                    int packetId = _packet.ReadInt();
                    Server.packetHandlers[packetId](id,_packet);
                }
            });
        }
        public void Disconnect(){
            endPoint=null;
        }
    }
    public void SendIntoGame(string pName){
        NetworkManager.instance.ServerCreatePlayer(id,pName);
        foreach (var _player in NetPlayer.players.Values)
        {
            if(_player.id!=0&&_player.id!=id)
            ServerSend.SpawnPlayer(_player.id,id);
        }
        foreach (var _player in NetPlayer.players.Values)
        {
            ServerSend.SpawnPlayer(id,_player.id);
        }
    }
    public void Connect(TcpClient client){
        tcp.Connect(client);
    }
    public void Disconnect(){
        if(tcp.socket!=null){
            ThreadManager.ExecuteOnMainThread(()=>{
                NetworkManager.instance.ServerDestroyPlayer(this);
            });
        }
        tcp.Disconnect();
        udp.Disconnect();
        ServerSend.DespawnPlayer(id);
    }
}
