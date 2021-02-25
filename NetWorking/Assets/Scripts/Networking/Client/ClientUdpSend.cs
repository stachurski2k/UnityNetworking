using System.Collections;
using System.Collections.Generic;
using UnityEngine;

partial class ClientSend 
{
    public static void SendUdpData(Packet packet){
        packet.WriteLength();
        Client.udp.SendData(packet);
    }
    public static void SendDebugMsg(string msg){
        using(Packet packet=new Packet((int)ClientPackets.DebugMsg)){
            packet.Write(msg);
            SendUdpData(packet);
        }
    }
    public static void SendExecuteFunc(int netId,int funId,bool safe, byte[] data){
        using(Packet packet=new Packet((int)ClientPackets.ExecuteFun)){
            packet.Write(netId);
            packet.Write(funId);
            if(data!=null)
                packet.Write(data);
            if(safe){
                SendTcpData(packet);
            }
            else{
                SendUdpData(packet);
            }
        }
    }
   
}
