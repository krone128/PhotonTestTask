using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Addons.Physics;
using Fusion.Sockets;
using HostBasics.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;


public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner _runner;
    
    public GameManager gameManager;
    
    private void OnGUI()
    {
        var buttonSize = new Vector2(600, 250);
        
        if (_runner == null)
        {
            if (GUI.Button(new Rect((Screen.width - buttonSize.x) / 2f,  (Screen.height - buttonSize.y) / 2f, buttonSize.x, buttonSize.y), "Host"))
            {
                StartGame(GameMode.Host);
            }

            if (GUI.Button(new Rect((Screen.width - buttonSize.x) / 2f, (Screen.height + buttonSize.y) / 2f, buttonSize.x, buttonSize.y), "Join"))
            {
                StartGame(GameMode.Client);
            }
        }
    }

    async void StartGame(GameMode mode)
    {
        Player.OnPlayerSpawned += OnPlayerSpawned;
        
        // Create the Fusion runner and let it know that we will be providing user input
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        var runnerSimulatePhysics3D = gameObject.AddComponent<RunnerSimulatePhysics3D>();
        runnerSimulatePhysics3D.ClientPhysicsSimulation = ClientPhysicsSimulation.SimulateAlways;

        // Create the NetworkSceneInfo from the current scene
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid) {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }
        
        gameManager.Init(_runner);
        
        // Start or join (depends on gamemode) a session with a specific name
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestRoom",
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
        
        if (mode == GameMode.Host)
        {
            gameManager.SetupWorld();
        }
    }

    private void OnPlayerSpawned(Player player)
    {
        if (player.Object.HasInputAuthority)
        {
            gameManager.playerTransform = player.transform;
            Camera.main.GetComponent<CameraFollow>().playerTransform = player.transform;
        }
    }

    [SerializeField]
    private NetworkPrefabRef _playerPrefab; // Character to spawn for a joining player

    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            // Create a unique position for the player
            Vector3 spawnPosition = new Vector3(GameConfig.GridSize.x / 2f + GameConfig.ChunkSize / 2f, 1, GameConfig.GridSize.y / 2f + GameConfig.ChunkSize / 2f);
            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
            // Keep track of the player avatars for easy access
            _spawnedCharacters.Add(player, networkPlayerObject);

            if (player.PlayerId > 1)
            {
                gameManager.PlayerJoined(player, networkPlayerObject.transform);
            }
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
            gameManager.PlayerLeft(player);
        }
    }

    private bool _mouseButton0;
    private bool _mouseButton1;

    private void Update()
    {
        _mouseButton0 = _mouseButton0 || Input.GetMouseButton(0);
        _mouseButton1 = _mouseButton1 || Input.GetMouseButton(1);
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        if (Input.GetKey(KeyCode.W))
            data.direction += Vector3.forward;

        if (Input.GetKey(KeyCode.S))
            data.direction += Vector3.back;

        if (Input.GetKey(KeyCode.A))
            data.direction += Vector3.left;

        if (Input.GetKey(KeyCode.D))
            data.direction += Vector3.right;
        
        data.buttons.Set(NetworkInputData.MOUSEBUTTON0, _mouseButton0);
        _mouseButton0 = false;
        data.buttons.Set(NetworkInputData.MOUSEBUTTON1, _mouseButton1);
        _mouseButton1 = false;
        
        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {

    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }
    
    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }
    
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }
}