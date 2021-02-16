using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
public class ServerHandle 
{
  public static void HandleWelcomeCallback(int fromClient,Packet packet){
      var id=packet.ReadInt();
      string name=packet.ReadString();
      Debug.Log("client connected and authorized!");
      Server.clients[id].SendIntoGame(name);
  }
}
