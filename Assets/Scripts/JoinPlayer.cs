using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class JoinPlayer : MonoBehaviour
{
    private PlayerInput _playerInput;
    public static Action<int> onJoinSuccess;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        DontDestroyOnLoad(this);

        _playerInput = GetComponent<PlayerInput>();

        name = "Player " + _playerInput.user.id;
        _playerInput.actions["StartGame"].performed += StartGame;
        _playerInput.actions["LeaveGame"].performed += LeaveGame;

        if (!MatchManager.Instance.RegisterPlayer(this)) Destroy(this);

        onJoinSuccess?.Invoke(_playerInput.playerIndex);
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void OnDestroy()
    {
        MatchManager.Instance.UnregisterPlayer(this);

        _playerInput.actions["StartGame"].performed -= StartGame;
        _playerInput.actions["LeaveGame"].performed -= LeaveGame;
    }

    private void StartGame(InputAction.CallbackContext ctx)
    {
        MatchManager.Instance.TryStartGame(this);
    }

    private void LeaveGame(InputAction.CallbackContext ctx)
    {
        Debug.Log("Player opted to leave the game: " + this);
        Destroy(gameObject);
    }
}