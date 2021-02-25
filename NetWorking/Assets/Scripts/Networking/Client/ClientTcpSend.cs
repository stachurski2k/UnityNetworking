using System.Collections;
using System.Collections.Generic;
using UnityEngine;

partial class ClientSend 
{
    public static void SendTcpData(Packet packet){
        packet.WriteLength();
        Client.tcp.SendData(packet);
    }
   public static void WelcomeEcho(int id){
       using(Packet packet=new Packet((int)ClientPackets.WelcomeEcho)){
            packet.Write(id);
            packet.Write(NetworkManager.instance.playerName);
            SendTcpData(packet);
       }
   }
   public static void RequestSpawn(int prefabID,Vector3 pos){
       using(Packet packet=new Packet((int)ClientPackets.RequestSpawn)){
           packet.Write(prefabID);
           packet.Write(pos);
           SendTcpData(packet);
       }
   }
   public static void RequestDestroyID(int id){
       using(Packet packet=new Packet((int)ClientPackets.RequestDestroyID)){
           packet.Write(id);
           SendTcpData(packet);
       }
   }
}
