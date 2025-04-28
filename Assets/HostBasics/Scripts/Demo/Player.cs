using System;
using Fusion;
using TMPro;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public static event Action<Player> OnPlayerSpawned;
    public float Speed { get; set; } = 25;

    private NetworkCharacterController _cc;
    private Vector3 _forward = Vector3.forward;
    private ChangeDetector _changeDetector;

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterController>();
    }
    
    public override void Spawned()
    {
        OnPlayerSpawned?.Invoke(this);
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();
            _cc.Move(Speed * data.direction * Runner.DeltaTime);
        }
    }

    public void Update()
    {
        if (Object.HasInputAuthority && Input.GetKeyDown(KeyCode.R))
        {
            RPC_SendMessage("Hey Mate!");
        }
    }

    private TMP_Text _messages;

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_SendMessage(string message, RpcInfo info = default)
    {
       RPC_RelayMessage(message, info.Source);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_RelayMessage(string message, PlayerRef messageSource)
    {
        if (_messages == null)
            _messages = FindObjectOfType<TMP_Text>();

        if (messageSource == Runner.LocalPlayer)
        {
            message = $"You said: {message}\n";
        }
        else
        {
            message = $"Some other player said: {message}\n";
        }
        
        _messages.text += message;
    }
}