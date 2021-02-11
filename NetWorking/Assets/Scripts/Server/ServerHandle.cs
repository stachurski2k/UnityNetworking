using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
public class ServerHandle 
{
  public static void HandleWelcomeCallback(Packet packet){
      var data=packet.ReadBytes();
      Debug.Log(Encoding.ASCII.GetString(data,0,data.Length));
  }
}
