using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
public static class Server
{
    public static Dictionary<int,ServerClient> clients=new Dictionary<int, ServerClient>();
    public static Dictionary<int,Action<int,Packet>> packetHandlers=new Dictionary<int, Action<int,Packet>>();
    public static int MaxPlayers=5;
    public static int Port;
    public static IPAddress IP;
    static TcpListener tcpListener;
    static UdpClient udpListener;
    public static bool isRunning{get; private set;}
    public static bool CanAcceptClients;
    public static event Action<bool> OnStartServer;
    public static event Action OnStopServer;
    public static void StartServer(int maxPlayers=5,int port=23000){
        IP=IPAddress.Any;
        Port=(port>0&&port<6535)?port:23000;
        MaxPlayers=(maxPlayers>1&&maxPlayers<100)?maxPlayers:5;
        bool isPortInUse=Network.IsPortInUse(Port);
        if(isPortInUse){
            InvokeOnStartServer(!isPortInUse);
            return;
        }else{
            isRunning=true;
            CanAcceptClients=true;
            InvokeOnStartServer(!isPortInUse);
        }
        tcpListener=new TcpListener(port);
        udpListener=new UdpClient(port);
        InitData();
        ThreadManager.ExecuteOnNewThread(StartTcpAccepting);
        ThreadManager.ExecuteOnNewThread(StartUdpListening);
    }
    public static void StopServer(){
        if(isRunning){
            isRunning=false;
            tcpListener.Stop();
            udpListener.Close();
            Sender.Stop();
            foreach (var client in clients.Values)
            {
                client.Disconnect();
            }
            InvokeOnStopServer();
        }
    }
    static void InitData(){
        clients.Clear();
        packetHandlers.Clear();
        for (int i = 1; i < MaxPlayers; i++)
        {
            clients.Add(i,new ServerClient(i));
        }
        packetHandlers.Add((int)ClientPackets.WelcomeEcho,ServerHandle.HandleWelcomeCallback);
        packetHandlers.Add((int)ClientPackets.DebugMsg,ServerHandle.HandleDebugMsg);
        packetHandlers.Add((int)ClientPackets.RequestSpawn,ServerHandle.HandleRequestSpawn);
        packetHandlers.Add((int)ClientPackets.RequestDestroyID,ServerHandle.HandleRequestDestroyID);
        packetHandlers.Add((int)ClientPackets.ExecuteFun,ServerHandle.HandleExecuteFun);
    }
    static void StartTcpAccepting(){
        Sender.StartTcp();
        tcpListener.Start();
        while(isRunning){
            try{
                TcpClient client=tcpListener.AcceptTcpClient();
                if(!CanAcceptClients||AddClient(client)){
                    client.Client.Close();
                }
            }catch{
                break;
            }
        }
    }
    static void StartUdpListening(){
        Sender.StartUdp(udpListener);
        while(isRunning){
            try{
                IPEndPoint receiveEP=new IPEndPoint(IPAddress.Any,0);

                byte[] data=udpListener.Receive(ref receiveEP);
                if(data.Length<4){
                    continue;
                }
                using(Packet packet=new Packet(data)){
                    int clientId = packet.ReadInt();
                    if (clientId == 0)
                    {
                        continue;
                    }
                    if (clients[clientId].udp.endPoint == null)
                    {
                        clients[clientId].udp.Connect(receiveEP);
                        continue;
                    }
                    if (clients[clientId].udp.endPoint.ToString() == receiveEP.ToString())
                    {

                        clients[clientId].udp.HandleData(packet);
                    }
                }

            }catch(SocketException e){
                if(!isRunning)
                    break;
            }
        }
    }
    static bool AddClient(TcpClient client){
        for (int i = 1; i < MaxPlayers; i++)
        {
             if(clients[i].tcp.socket==null){
                clients[i].Connect(client);
                return false;
            }
        }
        return true;
    }
    #region  Invkoers
    static void InvokeOnStopServer(){
        if(OnStopServer!=null)
            OnStopServer();
    }
    static void InvokeOnStartServer(bool succes){
        if(OnStartServer!=null)
            OnStartServer(succes);
    }
    #endregion
}
