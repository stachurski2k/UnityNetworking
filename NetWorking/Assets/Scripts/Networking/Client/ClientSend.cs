using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend 
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
}
