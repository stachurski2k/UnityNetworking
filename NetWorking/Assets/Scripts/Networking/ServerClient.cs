using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System;
public class ServerClient 
{
    public TCP tcp;
    public int id;
    public static event Action<ServerClient,string> ServerOnClientConnected;
    public static event Action<ServerClient> ServerOnClientDisconnected;
    public ServerClient(int id){
        this.id =id;
        tcp=new TCP();
    }
    public class TCP{
        public TcpClient socket;
        NetworkStream stream;
        Packet receivePacket;
        byte[] receivedBuff;
        public int id;
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
                        Debug.Log(packedId);
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
        public void Send(byte[] data){
            if(socket==null)return;
            try
            {
                Sender.AddTcpData(socket,data);
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
    public void SendIntoGame(string pName){
        InvokeOnClientConnected(this,pName);
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
        tcp.id=id; 
        tcp.Connect(client);
    }
    public void Disconnect(){
        if(tcp.socket!=null){
            ThreadManager.ExecuteOnMainThread(()=>{
                InvokeOnClientDisconnected(this);
            });
        }
        tcp.Disconnect();
        ServerSend.DespawnPlayer(id);
    }
    #region Invokers
    public void InvokeOnClientConnected(ServerClient client,string pName){
        if(ServerOnClientConnected!=null){
            ServerOnClientConnected( client,pName);
        }
    }
    public void InvokeOnClientDisconnected(ServerClient client){
        if(ServerOnClientDisconnected!=null){
            ServerOnClientDisconnected( client);
        }
    }
    
    #endregion
}
