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
    public static EventHandler OnDisconnect;
    public ServerClient(int id){
        this.id =id;
        tcp=new TCP();
    }
    public class TCP{
        public TcpClient socket;
        NetworkStream stream;
        byte[] receivedBuff;
        public int id;
        public void Connect(TcpClient client){
            socket=client;
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
                    Debug.Log(bytesReceived);
                    if(bytesReceived==0){
                        Debug.Log("lol");
                        Server.clients[id].Disconnect();
                        break;
                    }
                    byte[] data=new byte[bytesReceived];
                    Array.Copy(receivedBuff,data,bytesReceived);
                    HandleData(data);
                }
                catch (System.Exception)
                {
                    break;
                    throw;
                }
            }
        }
        void HandleData(byte[] data){
            ThreadManager.ExecuteOnMainThread(()=>{
                using(Packet packet=new Packet(data)){
                    int id=packet.ReadInt();
                    
                    Server.packetHandlers[id](packet);
                }
            });
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
            socket.Client.Close();
            socket=null;
            stream=null;
        }
    }
    public void SendIntoGame(){
        NetworkManager.instance.ServerCreatePlayer(id);
        foreach (var player in NetPlayer.players.Values)
        {
            if(player.id!=0&&player.id!=id)
            ServerSend.SpawnPlayer(player.id,id);
        }
        foreach (var player in NetPlayer.players.Values)
        {
            ServerSend.SpawnPlayer(id,player.id);
        }
    }
    public void Connect(TcpClient client){
        tcp.id=id; 
        tcp.Connect(client);
    }
    public void Disconnect(){
        Debug.Log("lol disconnect");
        tcp.Disconnect();
        ThreadManager.ExecuteOnMainThread(()=>{
            OnDisconnect?.Invoke(this,null);
        });
    }
}
