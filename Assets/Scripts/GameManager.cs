using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class GameManager : BBB.SimpleMonoSingleton<GameManager>
{
    [Header("Player")]
    [SerializeField] PlayerReference m_player;
    Vector3 m_playerOrigin;
    Quaternion m_playerOriginOrientation;

    [Header("Gameplay")]
    [SerializeField] PayloadController m_payload;
    [SerializeField] PayloadSpline m_payloadPath;
    [SerializeField] EnemyDirector m_enemyDirector;
    [SerializeField] EnemySpawnController m_spawner;

    [SerializeField] UnityEvent m_gameStartEvent;
    [SerializeField] UnityEvent m_gameEndEvent;
    [SerializeField] UnityEvent m_gameWinEvent;
    [SerializeField] UnityEvent m_gameLoseEvent;
    [SerializeField] UnityEvent m_gameResetEvent;

    // Debug
    bool debug_showCursor = false;

    [SerializeField] bool debug_startOnLoad = false;

    public PlayerReference player { get { return m_player; } }
    public PayloadController payload { get { return m_payload; } }

    public EnemyDirector enemyDirector { get { return m_enemyDirector; } }

    protected override void Awake()
    {
        base.Awake();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Set enemies to constantly chase payload if doing nothing.
        EnemyDirector.instance.absoluteTarget = payload;

        m_spawner.enabled = false;
        m_payload.StopMoving();

        EnablePlayerControls(false);
        SetPlayerOrigin();

        InitialiseListeners();

        if(debug_startOnLoad)
        {
            StartGame();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            debug_showCursor = !debug_showCursor;
            if(debug_showCursor)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    #region GameManagement
    public void StartGame()
    {
        m_spawner.enabled = true;
        m_payload.StartMoving();

        EnablePlayerControls(true);

        m_gameStartEvent.Invoke();

        Cursor.lockState = CursorLockMode.Locked;
    }

    public void EndGame()
    {
        m_spawner.enabled = false;
        m_payload.StopMoving();
        m_enemyDirector.DespawnAllEnemies();

        m_gameEndEvent.Invoke();
    }

    public void ResetGame()
    {
        m_spawner.enabled = false;

        EnablePlayerControls(false);
        ResetPlayer();

        ResetPayload();

        m_enemyDirector.ResetDirector();
        m_spawner.ResetSpawner();

        m_gameResetEvent.Invoke();
    }

    public void WinGame()
    {
        m_gameWinEvent.Invoke();
        EndGame();
    }

    public void LoseGame()
    {
        m_gameLoseEvent.Invoke();
        EndGame();
    }
    #endregion // GameManagement

    #region PlayerManagement
    public void EnablePlayerControls(bool enabled)
    {
        m_player.input.inputsDisabled = !enabled;
    }

    void SetPlayerOrigin()
    {
        m_playerOrigin = m_player.controller.position;
        m_playerOriginOrientation = m_player.controller.transform.rotation;
    }

    void SetPlayerOrigin(Vector3 position, Quaternion rotation)
    {
        m_playerOrigin = position;
        m_playerOriginOrientation = rotation;
    }

    public void ResetPlayer()
    {
        m_player.controller.Warp(m_playerOrigin, m_playerOriginOrientation);
        m_player.controller.EndAction();
        m_player.animate.SetAnimToMovement();
        m_player.controller.StopVelocity();
        m_player.controller.Revive();
    }
    #endregion // PlayerManagement

    #region PayLoadManagement
    void InitialiseListeners()
    {
        m_payloadPath.finishEvent.AddListener(WinGame);
    }

    private void ResetPayload()
    {
        m_payload.ResetPayload();
        m_payload.StopMoving();
    }
    #endregion // PayLoadManagement
}
