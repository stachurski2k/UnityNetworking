using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientHandle 
{
    public static void HandleWelcome(Packet packet){
        int id=packet.ReadInt();
        Client.id=id;
        Debug.Log(id);
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
}
