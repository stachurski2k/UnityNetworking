using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Chat : MonoBehaviour
{
  [SerializeField] TMP_InputField msgInput;
  [SerializeField] TextMeshProUGUI msgPrefab;
  [SerializeField] Button sendButton;
  [SerializeField] Transform msgPlace;
  [SerializeField] bool useEnableDisable=true;
  private void OnEnable()
  {
      if(!useEnableDisable)return;
      for (int i = 0; i < msgPlace.childCount; i++)
      {
          Destroy(msgPlace.GetChild(i).gameObject);
      }
      FlowEvents.OnDebugMsgReceived+=PlaceMessage;
  }
  private void OnDisable()
  {
      if(!useEnableDisable)return;
    FlowEvents.OnDebugMsgReceived-=PlaceMessage;
  }
  private void Start()
  {
        for (int i = 0; i < msgPlace.childCount; i++)
        {
            Destroy(msgPlace.GetChild(i).gameObject);
        }
        FlowEvents.OnDebugMsgReceived+=PlaceMessage;
  }
  private void OnDestroy()
  {
    FlowEvents.OnDebugMsgReceived-=PlaceMessage;
  }
  public void Send(){
      string msg=msgInput.text;
      msgInput.text=string.Empty;
      if(NetworkManager.isServer()){
        PlaceMessage(0,msg);
          ServerSend.SendDebugMsg(0,msg);
      }
      if(NetworkManager.isClient()){
          ClientSend.SendDebugMsg(msg);
      }
  }
  public void PlaceMessage(int id, string msg){
      var text=Instantiate<TextMeshProUGUI>(msgPrefab,msgPlace);
      NetPlayer player=NetPlayer.players[id];
      text.text=$"{player.playerName} : {msg}";
  }
}
