using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net;
public static class Network 
{
   public static  bool IsPortInUse(int port){
        IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
        IPEndPoint [] ipEndPoints = ipProperties.GetActiveTcpListeners();
 
        foreach(IPEndPoint endPoint in ipEndPoints)
        {
            if  (endPoint.Port == port)
            {
                return true;
            }
        }
        return false;
   }
}
