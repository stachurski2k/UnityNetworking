using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend 
{
    public static void SendTcpData(Packet packet){
        Client.tcp.SendData(packet);
    }
   public static void WelcomeEcho(int id){
       using(Packet packet=new Packet((int)ClientPackets.WelcomeEcho)){
            packet.Write(id);
            SendTcpData(packet);
       }
   }
}
