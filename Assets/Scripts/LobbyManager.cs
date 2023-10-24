using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class LobbyManager : NetworkBehaviour
{

    public Button startButton;
    public TMPro.TMP_Text statusLabel;
    void Start()
    {
        startButton.gameObject.SetActive(false);
        statusLabel.text = "Start Client/Host/Server";


        startButton.onClick.AddListener(OnStartButtonClicked);
        NetworkManager.OnClientStarted += OnClientStarted;
        NetworkManager.OnServerStarted += OnServerStarted;
    }

    private void OnServerStarted() {
        GotoLobby();
        //startButton.gameObject.SetActive(true);
        //statusLabel.text = "Press Start";
    }
    private void OnClientStarted() {
        if (!IsHost) {
            statusLabel.text = "Waiting for game to start";
        }
    }

    private void OnStartButtonClicked() {
        StartGame();
    }

    public void GotoLobby()
    {
        NetworkManager.SceneManager.LoadScene("Lobby", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void StartGame() {
        NetworkManager.SceneManager.LoadScene("Arena1Game", 
        UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
    
}
