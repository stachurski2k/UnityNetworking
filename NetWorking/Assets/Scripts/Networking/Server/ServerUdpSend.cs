using UnityEngine;
using System;
partial class ServerSend 
{
  public static void SendUdpData(int id,Packet packet){
    packet.WriteLength();
    Server.clients[id].udp.SendData(packet);
  }
  public static void SendUdpDataToAll(Packet packet){
    packet.WriteLength();
    foreach (var client in Server.clients.Values)
    {
        client.udp.SendData(packet);
    }
  }
  public static void SendUdpDataToAll(Packet packet,int except){
    packet.WriteLength();
    foreach (var client in Server.clients.Values)
    {
      if(client.id!=except)
        client.udp.SendData(packet);
    }
  }
  public static void SendDebugMsg(int from,string msg){
      using(Packet packet =new Packet((int)ServerPackets.DebugMsg)){
        packet.Write(from);
        packet.Write(msg);
        SendUdpDataToAll(packet);
      }
  }
  public static void SendExecuteFunc(int netId,int funId,bool safe,Packet data=null,int except=-1){
    using(Packet packet=new Packet((int)ServerPackets.ExecuteFun)){
      packet.Write(netId);
      packet.Write(funId);
      if(data!=null)
        packet.Write(data.ToArray());
      if(safe){
        SendTcpDataToAll(except,packet);
      }
      else{
        SendUdpDataToAll(packet,except);
      }
    }
  }
}
