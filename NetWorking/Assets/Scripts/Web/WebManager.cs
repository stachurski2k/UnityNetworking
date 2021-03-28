using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
public class WebManager : MonoBehaviour
{
    void Start()
    {
        IPHostEntry ipEntry=Dns.GetHostEntry(Dns.GetHostName());
        IPAddress[] addr = ipEntry.AddressList;

        for (int i = 0; i < addr.Length; i++)
        {
            Debug.Log(addr[i].ToString());
        }
    }

    
}
