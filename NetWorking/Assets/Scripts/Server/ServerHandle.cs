using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
public class ServerHandle 
{
  public static void HandleWelcomeCallback(Packet packet){
      var id=packet.ReadInt();
      Debug.Log("client connected and authorized!");
      Server.clients[id].SendIntoGame();
  }
}
