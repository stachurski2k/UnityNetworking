using System.Collections;
using System.Collections.Generic;
using UnityEngine;

partial class ServerSend 
{
  public static void SendTcpData(int toClient,Packet data){
      data.WriteLength();
      Server.clients[toClient].tcp.Send(data);
  }
  public static void SendTcpDataToAll(Packet data){
      data.WriteLength();
      foreach (var c in Server.clients.Values)
      {
        c.tcp.Send(data);
      }
  }
  public static void SendTcpDataToAll(int id,Packet data){
      data.WriteLength();
      foreach (var c in Server.clients.Values)
      {
        if(c.id!=id)
        c.tcp.Send(data);
      }
  }
  public static void SendWelcome(int toClient){
      using(Packet packet=new Packet((int)ServerPackets.Welcome)){
          packet.Write(toClient);
          SendTcpData(toClient,packet);
      }
  }
  public static void SpawnPlayer(int toClient,int playerId){
    using(Packet packet=new Packet((int)ServerPackets.SpawnPlayer)){
        packet.Write(playerId);
        packet.Write(NetPlayer.players[playerId].playerName);
        SendTcpData(toClient,packet);
    }
  }
  public static void DespawnPlayer(int playerId){
    using (Packet packet=new Packet((int)ServerPackets.DespawnPlayer)){
      packet.Write(playerId);
      SendTcpDataToAll(packet);
    }
  }
  public static void StartGame(){
    using(Packet packet=new Packet((int)ServerPackets.StartGame)){
      SendTcpDataToAll(packet);
    }
  }
  public static void Spawn(NetIdentity id){
    using(Packet packet=new Packet((int)ServerPackets.Spawn)){
      packet.Write(id.prefabID);
      packet.Write(id.id);
      packet.Write(id.ownerId);
      packet.Write(id.transform.position);
      packet.Write(id.behaviours.Length);
      for (int i = 0; i < id.behaviours.Length; i++)
      {
        packet.Write(id.behaviours[i].id);
      }
      SendTcpDataToAll(packet);
    }
  }
  public static void DestroyID(NetIdentity identity){
    using(Packet packet= new Packet((int)ServerPackets.DestroyID)){
      packet.Write(identity.id);
      SendTcpDataToAll(packet);
    }
  }
}
