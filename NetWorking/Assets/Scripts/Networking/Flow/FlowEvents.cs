using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class FlowEvents : MonoBehaviour
{
  public static event Action<int,string> OnDebugMsgReceived;
  public static void InvokeDebugMsgReceived(int player,string msg){
      if(OnDebugMsgReceived!=null){
          OnDebugMsgReceived(player,msg);
      }
  }
}
