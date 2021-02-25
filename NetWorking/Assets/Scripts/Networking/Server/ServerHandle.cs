using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
public class ServerHandle 
{
  public static void HandleWelcomeCallback(int fromClient,Packet packet){
      var id=packet.ReadInt();
      string name=packet.ReadString();
      Server.clients[id].SendIntoGame(name);
  }
  public static void HandleDebugMsg(int fromClient, Packet packet){
    string msg=packet.ReadString();
    FlowEvents.InvokeDebugMsgReceived(fromClient,msg);
    ServerSend.SendDebugMsg(fromClient,msg);
  }
  public static void HandleRequestSpawn(int fromClient,Packet packet){
    int prefabID=packet.ReadInt();
    Vector3 pos=packet.ReadVector3();
    NetworkManager.instance.HandleRequestSpawn(fromClient,prefabID,pos);
  }
   public static void HandleRequestDestroyID(int fromClient,Packet packet){
    int id=packet.ReadInt();
    NetworkManager.instance.HandleRequestDestroyID(id);
  }
  public static void HandleExecuteFun(int fromClient,Packet packet){
    int netId=packet.ReadInt();
    NetBehaviour.behaviours[netId].FromClient(fromClient,packet);
  }
 
}
