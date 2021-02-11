using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
public static class Server
{
    public static Dictionary<int,ServerClient> clients=new Dictionary<int, ServerClient>();
    public static Dictionary<int,Action<Packet>> packetHandlers=new Dictionary<int, Action<Packet>>();
    public static int MaxPlayers=2;
    public static int Port;
    public static IPAddress IP;
    static TcpListener tcpListener;
    static bool isRunning;
    public static void StartServer(int port=23000){
        IP=IPAddress.Any;
        Port=(port>0&&port<6535)?port:23000;
        tcpListener=new TcpListener(port);
        InitData();
        ThreadManager.ExecuteOnNewThread(StartAccepting);
    }
    public static void StopServer(){
        if(isRunning){
            isRunning=false;
            tcpListener.Stop();
            foreach (var client in clients.Values)
            {
                client.Disconnect();
            }
        }
    }
    static void InitData(){
        isRunning=true;
        for (int i = 0; i < MaxPlayers; i++)
        {
            clients.Add(i,new ServerClient(i));
        }
        //val of AAAA in asci is 1094795585
        packetHandlers.Add(1094795585,ServerHandle.HandleWelcomeCallback);
    }
    static void StartAccepting(){
        tcpListener.Start();
        while(NetworkManager.isWorking()){
            try{
                TcpClient client=tcpListener.AcceptTcpClient();
                AddClient(client);
            }catch{
                break;
            }
        }
    }
    static void AddClient(TcpClient client){
        for (int i = 0; i < MaxPlayers; i++)
        {
             if(clients[i].tcp.socket==null){
                clients[i].Connect(client);
                return ;
            }
        }
        client.Client.Close();
    }
}
