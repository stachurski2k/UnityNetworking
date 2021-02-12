using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System;
public static class Client
{
    public static bool isConnected=false;
    public static event Action<bool> OnConnect;
    public static Dictionary<int,Action<Packet>> packetHandlers=new Dictionary<int, Action<Packet>>();
    public static int id;
    public static void ConnectToServer(string ip,int port=23000){
        tcp=new TCP();
        InitData();
        tcp.Connect(ip,port);
    }
    static void InitData(){
        packetHandlers.Add((int)ServerPackets.Welcome,ClientHandle.HandleWelcome);
        packetHandlers.Add((int)ServerPackets.SpawnPlayer,ClientHandle.SpawnPlayer);
    }
    public static TCP tcp;
    #region  Tcp
    public class TCP{
        public TcpClient socket;
        NetworkStream stream;
        byte[] receiveBuff;
        public void Connect(string ip,int port=23000){
            socket=new TcpClient();
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
            //To do let the app know if connected
            //OnConnect(socket.Connected);
            if(!socket.Connected){return;}
            isConnected=true;
            ThreadManager.ExecuteOnNewThread(()=>{
                receiveBuff=new byte[NetworkManager.bufferSize];
                stream=socket.GetStream();
                while(NetworkManager.isWorking()){
                    try
                    {
                        int bytesReceived=stream.Read(receiveBuff,0,NetworkManager.bufferSize);
                        Debug.Log("Data received!");
                        if(bytesReceived==0){
                            Client.Disconnect();
                            break;
                        }
                        byte[] data=new byte[bytesReceived];
                        Array.Copy(receiveBuff,data,bytesReceived);
                        HandleData(data);
                    }
                    catch (System.Exception)
                    {
                        break;
                        throw;
                    }
                }
            });
        }
        void HandleData(byte[] bytes){
            ThreadManager.ExecuteOnMainThread(()=>{
                using(Packet packet=new Packet(bytes)){
                    int id=packet.ReadInt();
                    packetHandlers[id](packet);
                }
            });
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
            socket.Client.Disconnect(false);
            socket.Client.Close();
            socket=null;
        }
    }
    #endregion
    public static void Disconnect(){
        isConnected=false;
        tcp.Disconnect();
        //To do add event on main Thread
    }
}
