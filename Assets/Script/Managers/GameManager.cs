using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Random = UnityEngine.Random;


public class GameManager : MonoBehaviour
{
    [Header("----------General----------")]
    [SerializeField] GameObject m_PlayerPrefab;
    [SerializeField] bool m_SpawnInRandomLocation = true;
    [SerializeField] Vector2 m_LevelSize = new(250, 250);
    [SerializeField] GameState m_GameState = GameState.NOT_RUNNING;
    [SerializeField] float m_TimeBeforeShowingFinalScore;
    [SerializeField] float m_TimeBeforeStartingGame = 5.0F;

    [Header("----------Player Aid----------")]
    [Tooltip("Pickup identification")]
    [SerializeField] string[] m_PickupPoolIDs;

    [Header("----------Debugging----------")]
    [SerializeField] bool m_VisualiseLevelBounds;
    [SerializeField] bool m_MakeLevelBoundsSolid;
    [SerializeField] bool m_DontSpawnHelp;

    private GameObject m_PlayerGameObject;
    List<BasePickup> m_PickupsInScene = new();
    List<Body> m_FrozenObjects = new();
    public static GameManager Instance;

    //Flags
    bool m_AllObjectsAreFrozen;
    bool m_GameIsPaused;
    bool m_StartedFromThisScene = true; //did we start the game in the scene from the game entry?

    //Timers
    float m_TimeSinceStartOfGame = 0;
    float m_StartOfGameTimer = 0;

    //Gameplay Elements
    int m_TotalKillsByPlayer = 0;

    //Events
    public static event Action OnGameIsStarting;
    public static event Action OnGameStarted;
    public static event Action OnGamePaused;
    public static event Action OnGameResume;
    public static event Action OnGameEnded;
    public static event Action<int, int, float> OnShowEndOfGameScreen;

    //Game Returns
    public float GameElapsedTime { get { return m_TimeSinceStartOfGame; } }
    public int TotalKillsByPlayer { get { return m_TotalKillsByPlayer; } }
    public bool GameIsPaused { get { return m_GameIsPaused; } }
    public float StartingTimer { get { return m_StartOfGameTimer; } }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
            Destroy(gameObject);

        SpawnPlayerInRandomPosition();
    }

    private void OnEnable()
    {
        PlayerHealthComponent.OnPlayerDeath += OnPlayerDeath;
        PauseMenuManager.OnPauseMenuClose += OnPauseMenuClose;
        LevelManager.OnLoadingComplete += OnLoadingScreenComplete;
    }

    private void OnDisable()
    {
        PlayerHealthComponent.OnPlayerDeath -= OnPlayerDeath;
        PauseMenuManager.OnPauseMenuClose -= OnPauseMenuClose;
        LevelManager.OnLoadingComplete -= OnLoadingScreenComplete;
    }

    private void OnLoadingScreenComplete()
    {
        m_StartedFromThisScene = false;
        InitialiseGame();
    }

    private void OnPauseMenuClose() => SetGameState(GameState.RUNNING);

    private void OnPlayerDeath()
    {
        //stop the game...
        m_GameState = GameState.OVER;

        //Slow Down...
        Time.timeScale = 0.25F;

        Debug.Log("Game is OVER!");

        OnGameEnded?.Invoke();
    }

    private void Start()
    {
        if (m_StartedFromThisScene)
            InitialiseGame();
    }

    private void InitialiseGame()
    {
        m_StartOfGameTimer = m_TimeBeforeStartingGame;

        SetGameState(GameState.STARTING);

        OnGameIsStarting?.Invoke();
    }

    private void Update()
    {
        switch (m_GameState)
        {
            case GameState.STARTING:

                m_StartOfGameTimer -= Time.deltaTime;

                if (m_StartOfGameTimer <= 0)
                {
                    OnGameStarted?.Invoke();

                    SetGameState(GameState.RUNNING);
                }

                break;

            case GameState.RUNNING:

                m_GameIsPaused = false;

                Cursor.visible = false;

                if (m_AllObjectsAreFrozen)
                    UnFreezeAllDynamicObjects();

                //Check if the player pressed pause...
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    m_GameIsPaused = true;
                    SetGameState(GameState.PAUSED);
                }

                m_TimeSinceStartOfGame += Time.deltaTime;

                break;

            case GameState.PAUSED:

                Cursor.visible = true;

                ProcessPausedGame();

                break;

            case GameState.OVER:

                //TO-DO : Compile results and display them after a few seconds...


                //Show the cursor...
                Cursor.visible = true;

                //TO-DO : Save results...

                return;
        }
    }

    private void ProcessPausedGame()
    {
        m_GameIsPaused = true;

        //Check if the player pressed RESUME...
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetGameState(GameState.RUNNING);

            m_GameIsPaused = false;

            return;
        }

        if (!m_AllObjectsAreFrozen)
        FreezeAllDynamicObjects();
    }

    private void UnFreezeAllDynamicObjects()
    {
        if (m_AllObjectsAreFrozen)
        {
            //Freeze all particles
            ParticleSystem[] particleSystems = FindObjectsOfType<ParticleSystem>();

            foreach (ParticleSystem particle in particleSystems)
                particle.Play(true);
            
            //UN-freeze all tanks
            foreach (Body body in m_FrozenObjects)
            {
                body.UnFreeze();
            }

            m_AllObjectsAreFrozen = false;
        }
    }

    private void FreezeAllDynamicObjects()
    {
        if (m_AllObjectsAreFrozen)
        {
            Debug.LogWarning("All objects are already frozen!");
            return;
        }
        
        //Freeze all particles
        ParticleSystem[] particleSystems = FindObjectsOfType<ParticleSystem>();

        foreach(ParticleSystem particle in particleSystems)
            particle.Pause(true);
        

        Rigidbody[] rigidBodiesInScene = FindObjectsOfType<Rigidbody>();

        m_FrozenObjects.Clear();

        //freeze all tanks
        foreach (Rigidbody body in rigidBodiesInScene)
        {
            if (body.isKinematic)
                continue;

            m_FrozenObjects.Add(new(body, body.velocity));
        }

        m_AllObjectsAreFrozen = true;
    }

    private void SetGameState(GameState newState)
    { 
        m_GameState = newState;

        switch (m_GameState)
        {
            case GameState.PAUSED:
                OnGamePaused?.Invoke();
                break;

            case GameState.RUNNING:
                OnGameResume?.Invoke();
                break;
        }
    }

    public void SpawnPickupAtPosition(string pickUpPoolID, Vector3 position)
    {
        //TODO : SPAWN PICKUP in a specific place
    }

    private void SpawnPlayerInRandomPosition()
    {
        if (m_PlayerPrefab is null)
        {
            Debug.LogError("There is no player prefab assigned!");
            return;
        }

        if (m_SpawnInRandomLocation)
            m_PlayerGameObject = Instantiate(m_PlayerPrefab, GetRandomPositionInLevelBounds(), Quaternion.identity);
    }

    private Vector3 GetRandomPositionInLevelBounds()
    {
        int maxIteration = 1000;

        Vector3 randomPosition = Vector3.zero;

        for (int j = 0; j < maxIteration; j++)
        {
            Bounds levelBounds = new(Vector3.zero, m_LevelSize);

            randomPosition = new()
            {
                x = Random.Range(-levelBounds.extents.x, levelBounds.extents.x),
                y = 0,
                z = Random.Range(-levelBounds.extents.z, levelBounds.extents.z),
            };

            if (NavMesh.SamplePosition(randomPosition, out var hit, 1000.0F, NavMesh.AllAreas))
            {
                return hit.position;
            }

            Debug.Log("Ran out of iterations defaulting position to World Origin");
        }

        return randomPosition;
    }

    private void OnDrawGizmosSelected()
    {
        if (m_VisualiseLevelBounds)
        {
            Gizmos.color = Color.green;

            if (!m_MakeLevelBoundsSolid)
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(m_LevelSize.x, 1, m_LevelSize.y));
            else
                Gizmos.DrawCube(Vector3.zero, new Vector3(m_LevelSize.x, 1, m_LevelSize.y));
        }
    }
}

public struct Body
{
    private Vector3 m_PreviousVelocity;
    private Rigidbody m_RigidBody;

    public Body(Rigidbody rigidbody, Vector3 previousVelocity)
    {
        m_PreviousVelocity = previousVelocity;
        m_RigidBody = rigidbody;
        Freeze();
    }

    public void Freeze()
    {
        m_RigidBody.isKinematic = true;
    }

    public void UnFreeze()
    {
        m_RigidBody.isKinematic = false;
        m_RigidBody.velocity = m_PreviousVelocity;
    }
}