using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
public class ClientHandle 
{
    public static void HandleWelcome(Packet packet){
        int id=packet.ReadInt();
        Client.id=id;
        Client.udp.Connect(((IPEndPoint)Client.tcp.socket.Client.LocalEndPoint).Port);
        ClientSend.WelcomeEcho(id);
    }
    public static void SpawnPlayer(Packet packet){
        int id=packet.ReadInt();
        string pName=packet.ReadString();
        NetworkManager.instance.ClientCreatePlayer(id,pName);
    }
    public static void DespawnPlayer(Packet packet){
        int id=packet.ReadInt();
        NetworkManager.instance.ClientDespawnPlayer(id);
    }
    public static void StartGame(Packet packet){
        GameManager.instance.ClientStartGame();
    }
    public static void HandleDebugMsg(Packet packet){
        int id=packet.ReadInt();
        string msg=packet.ReadString();
        FlowEvents.InvokeDebugMsgReceived(id,msg);
    }
    public static void Spawn(Packet packet){
        int prefabId=packet.ReadInt();
        int id=packet.ReadInt();
        int ownerId=packet.ReadInt();
        Vector3 pos=packet.ReadVector3();
        int behavioursLength=packet.ReadInt();
        int[] behavioursIds=new int[behavioursLength];
        for (int i = 0; i < behavioursLength; i++)
        {
            behavioursIds[i]=packet.ReadInt();
        }
        NetworkManager.instance.ClientSpawn(prefabId,id,ownerId,pos,behavioursIds);
    }
    public static void HandleExecuteFun(Packet packet){
        int id=packet.ReadInt();
        NetBehaviour.behaviours[id].FromServer(packet);
    }
    public static void DestroyID(Packet packet){
        int id=packet.ReadInt();
        NetworkManager.instance.ClientDestroyID(id);
    }
}
