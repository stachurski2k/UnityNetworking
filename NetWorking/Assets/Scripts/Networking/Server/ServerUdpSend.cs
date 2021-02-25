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
  public static void SendDebugMsg(int from,string msg){
      using(Packet packet =new Packet((int)ServerPackets.DebugMsg)){
        packet.Write(from);
        packet.Write(msg);
        SendUdpDataToAll(packet);
      }
  }
  public static void SendExecuteFunc(int netId,int funId,bool safe,byte[] data=null){
    using(Packet packet=new Packet((int)ServerPackets.ExecuteFun)){
      packet.Write(netId);
      packet.Write(funId);
      if(data!=null)
        packet.Write(data);
      if(safe){
        SendTcpDataToAll(packet);
      }
      else{
        SendUdpDataToAll(packet);
      }
    }
  }
}
