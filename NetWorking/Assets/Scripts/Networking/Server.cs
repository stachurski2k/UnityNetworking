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
    public static bool isRunning{get; private set;}
    public static bool CanAcceptClients{get; private set;}
    public static event Action<bool> OnStartServer;
    public static event Action OnStopServer;
    static bool isTringToEstablish;
    public static void StartServer(int port=23000,int maxPlayers=5){
        if(isTringToEstablish)return;
        isTringToEstablish=true;
        IP=IPAddress.Any;
        Port=(port>0&&port<6535)?port:23000;
        MaxPlayers=(maxPlayers>1&&maxPlayers<100)?maxPlayers:5;
        bool isPortInUse=Network.IsPortInUse(Port);
        InvokeOnStartServer(!isPortInUse);
        if(isPortInUse){
            isTringToEstablish=false;
            return;
        }
        CanAcceptClients=true;
        isRunning=true;
        tcpListener=new TcpListener(port);
        InitData();
        ThreadManager.ExecuteOnNewThread(StartAccepting);
        isTringToEstablish=false;
    }
    public static void StopServer(){
        if(isRunning){
            isRunning=false;
            tcpListener.Stop();
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
    }
    static void StartAccepting(){
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
