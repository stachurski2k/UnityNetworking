using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend 
{
  public static void SendTcpData(int toClient,Packet data){
      data.WriteLength();
      Server.clients[toClient].tcp.Send(data.ToArray());
  }
  public static void SendTcpDataToAll(Packet data){
      data.WriteLength();
      foreach (var c in Server.clients.Values)
      {
        c.tcp.Send(data.ToArray());
      }
  }
  public static void SendWelcome(int toClient){
      using(Packet packet=new Packet((int)ServerPackets.Welcome)){
          packet.Write(toClient);
          SendTcpData(toClient,packet);
      }
  }
}
