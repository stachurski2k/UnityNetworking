using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class MainMenu : MonoBehaviour
{
    public GameObject mainPage;
    public GameObject lobbyPage;
    public GameObject clientDataPage;
    public GameObject serverDataPage;
    [SerializeField] Transform playerLobbyParent;
    [SerializeField] TMP_InputField serverNameInputField;
    [SerializeField] TMP_InputField clientNameInputField;
    [SerializeField] TMP_InputField ipInputField;
    [SerializeField] Button returnBtn;
    [SerializeField] PlayerLobby playerLobbyPrefab;
    Dictionary<NetPlayer,PlayerLobby> lobbyPlayers=new Dictionary<NetPlayer, PlayerLobby>();
    private void Awake()
    {
        NetworkManager.OnSuccesStart+=()=>SetOnTop(lobbyPage);
        NetworkManager.OnPlayerCreated+=AddPlayerToLobby;
        NetworkManager.OnPlayerDestroyed+=RemovePlayerFromLobby;
    }
    void Start()
    {
        SetOnTop(mainPage);
    }
    private void OnDestroy()
    {
        NetworkManager.OnPlayerCreated-=AddPlayerToLobby;
        NetworkManager.OnPlayerDestroyed-=RemovePlayerFromLobby;
    }
    public void SetOnTop(GameObject obj){
        obj.SetActive(true);
        if(obj!=lobbyPage){
            lobbyPage.SetActive(false);
        }
        if(obj!=clientDataPage){
            clientDataPage.SetActive(false);
        }
        if(obj!=serverDataPage){
            serverDataPage.SetActive(false);
        }
        returnBtn.gameObject.SetActive(false);
        if(obj!=mainPage){
            mainPage.SetActive(false);
            returnBtn.gameObject.SetActive(true);
        }
    }
    void AddPlayerToLobby(NetPlayer player){
        PlayerLobby playerLobby= Instantiate<PlayerLobby>(playerLobbyPrefab,playerLobbyParent);
        playerLobby.Init(player.id);
        playerLobby.GetComponentInChildren<TextMeshProUGUI>().text=player.playerName;
        lobbyPlayers.Add(player,playerLobby);
    }
    void RemovePlayerFromLobby(NetPlayer player){
        var lobbyPlayer=lobbyPlayers[player];
        lobbyPlayers.Remove(player);
        Destroy(lobbyPlayer.gameObject);
    }
    void SetMain(){
        SetOnTop(mainPage);
    }
    #region  BTns
    public void StartServerBtn(){
        SetOnTop(serverDataPage);
    }
    public void StartServer(){
        if(NetworkManager.instance.SetName(serverNameInputField.text)){
            NetworkManager.instance.StartServer();
        }
    }
    public void StartClientBtn(){
        SetOnTop(clientDataPage);
    }
    public void ConnectClient(){
        if(NetworkManager.instance.SetName(clientNameInputField.text)){
            NetworkManager.instance.StartClient(ipInputField.text);
        }
    }
    public void ReturnBtn(){
        NetworkManager.instance.StopAll();
        SetOnTop(mainPage);
    }
    #endregion

}
