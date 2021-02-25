using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
using System;
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
   public static IPAddress ResolveNameToIP(string name){
        try
        {
            IPAddress[] ips = Dns.GetHostAddresses(name);
            foreach (var ip in ips)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
        return null;
   }
}
