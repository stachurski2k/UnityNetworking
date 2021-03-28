using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class NetPlayerList : MonoBehaviour
{
    
    [SerializeField] PlayerLobby playerLobbyPrefab;
    Dictionary<NetPlayer,PlayerLobby> lobbyPlayers=new Dictionary<NetPlayer, PlayerLobby>();
    void Start()
    {
        foreach (var player in NetPlayer.players.Values)
        {
            AddPlayerToLobby(player);
        }
        NetworkManager.OnPlayerCreated+=AddPlayerToLobby;
        NetworkManager.OnPlayerDestroyed+=RemovePlayerFromLobby;
    }
    private void OnDestroy()
    {
        NetworkManager.OnPlayerCreated-=AddPlayerToLobby;
        NetworkManager.OnPlayerDestroyed-=RemovePlayerFromLobby;
    }
       void AddPlayerToLobby(NetPlayer player){
        PlayerLobby playerLobby= Instantiate<PlayerLobby>(playerLobbyPrefab,transform);
        playerLobby.Init(player.id);
        playerLobby.GetComponentInChildren<TextMeshProUGUI>().text=player.playerName;
        lobbyPlayers.Add(player,playerLobby);
    }
    void RemovePlayerFromLobby(NetPlayer player){
        var lobbyPlayer=lobbyPlayers[player];
        lobbyPlayers.Remove(player);
        Destroy(lobbyPlayer.gameObject);
    }

}
