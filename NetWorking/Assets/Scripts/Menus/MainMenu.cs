using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class MainMenu : MonoBehaviour
{
    public GameObject mainPage;
    public GameObject lobbyPage;
    public GameObject clientDataPage;
    [SerializeField] Transform playerLobbyParent;
    [SerializeField] TMP_InputField ipInputField;
    [SerializeField] TextMeshProUGUI textPrefab;
    private void Awake()
    {
        NetworkManager.OnPlayerCreated+=AddPlayerToLobby;
    }
    void Start()
    {
        SetOnTop(mainPage);
    }
    public void SetOnTop(GameObject obj){
        obj.SetActive(true);
        if(obj!=lobbyPage){
            lobbyPage.SetActive(false);
        }
        if(obj!=clientDataPage){
            clientDataPage.SetActive(false);
        }
        if(obj!=mainPage){
            mainPage.SetActive(false);
        }
    }
    void AddPlayerToLobby(NetPlayer player){
        TextMeshProUGUI text= Instantiate<TextMeshProUGUI>(textPrefab,playerLobbyParent);
        text.text=$"Player{player.id}";
    }
    public void StartServerBtn(){
        NetworkManager.instance.StartServer();
        SetOnTop(lobbyPage);
    }
    public void StartClientBtn(){
        SetOnTop(clientDataPage);
    }
    public void ConnectClient(){
        SetOnTop(lobbyPage);
        NetworkManager.instance.StartClient(ipInputField.text);
    }

}
