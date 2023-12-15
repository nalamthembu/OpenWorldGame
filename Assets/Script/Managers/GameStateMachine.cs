using System;
using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public class GameStateMachine : MonoBehaviour
{
    public GameStatePaused gameStatePaused = new();
    public GameStateRunning gameStateRunning = new();
    public GameState currentState = null;
    
    public static GameStateMachine Instance { get; private set; }

    public static event Action OnPaused;
    public static event Action OnResume;

    public bool IsGamePaused { get; private set; }

    private void Awake()
    {
        if (Instance is not null)
            Destroy(gameObject);

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        currentState = gameStateRunning;
    }

    private void Update()
    {
        if (currentState is not null)
        {
            currentState.OnUpdate(this);
        }
    }

    public void DoSwitchState(GameState state)
    {
        if (currentState is not null)
            currentState.OnExit(this);

        currentState = state;
        currentState.OnEnter(this);
    }

    public void SetGameIsPaused(bool isPaused)
    {
        if (isPaused)
        {
            DoSwitchState(gameStatePaused);
            OnPaused?.Invoke();
        }
        else
        {
            DoSwitchState(gameStateRunning);
            OnResume?.Invoke();
        }

        IsGamePaused = isPaused;
    }
}

public abstract class GameState
{
    public abstract void OnEnter(GameStateMachine stateMachine);
    public abstract void OnUpdate(GameStateMachine stateMachine);
    public abstract void OnExit(GameStateMachine stateMachine);
}

[System.Serializable]
public class GameStatePaused : GameState
{
    GameStateMachine machine;
    public List<PausedRigidbodies> RigidBodiesInScene { get; private set; }
    public List<Character> CharactersInScene { get; private set; }

    private void InitLists()
    {
        RigidBodiesInScene = new();
        CharactersInScene = new();

        GetAllRigidBodies();
        GetAllCharacters();
    }

    private void GetAllCharacters()
    {
        if (CharactersInScene is not null)
            CharactersInScene.Clear();

        Character[] chrs = Object.FindObjectsOfType<Character>();

        for (int i = 0; i < chrs.Length; i++)
        {
            CharactersInScene.Add(chrs[i]);
        }
    }

    private void GetAllRigidBodies()
    {
        if (RigidBodiesInScene is not null)
            RigidBodiesInScene.Clear();

        Rigidbody[] rbs = Object.FindObjectsOfType<Rigidbody>();

        for (int i = 0; i < rbs.Length; i++)
        {
            //Skip Kinematic Bodies (Don't alter them)
            if (rbs[i].isKinematic)
                continue;

            RigidBodiesInScene.Add(new(rbs[i], rbs[i].velocity));
        }
    }

    private void FreezeAllCharacters()
    {
        for(int i = 0; i < CharactersInScene.Count; i++)
        {
            if (CharactersInScene != null)
                CharactersInScene[i].Freeze();
        }
    }

    private void UnFreezeAllCharacters()
    {
        for (int i = 0; i < CharactersInScene.Count; i++)
        {
            if (CharactersInScene != null)
                CharactersInScene[i].UnFreeze();
        }
    }

    private void FreezeAllRigidbodies()
    {
        for (int i = 0; i < RigidBodiesInScene.Count; i++)
        {
            if (RigidBodiesInScene != null)
            RigidBodiesInScene[i].rigidbody.isKinematic = true;
        }
    }

    private void UnFreezeAllRigidbodies()
    {
        for (int i = 0; i < RigidBodiesInScene.Count; i++)
        {
            if (RigidBodiesInScene != null)
            {
                RigidBodiesInScene[i].rigidbody.isKinematic = false;
                RigidBodiesInScene[i].rigidbody.velocity = RigidBodiesInScene[i].prevVelocity;
            }
        }
    }

    public override void OnEnter(GameStateMachine stateMachine)
    {
        if (machine is null)
            machine = stateMachine;

        InitLists();

        FreezeAllRigidbodies();

        FreezeAllCharacters();

        GameManager.instance.SetCursorState(true, CursorLockMode.None);

        Debug.Log("GameState : Paused ");
    }

    public override void OnExit(GameStateMachine stateMachine)
    {
        UnFreezeAllRigidbodies();

        UnFreezeAllCharacters();
    }

    public override void OnUpdate(GameStateMachine stateMachine)
    {
        //TO-DO : Things when the game is paused.

    }
}

[System.Serializable]
public class GameStateRunning : GameState
{
    public override void OnExit(GameStateMachine stateMachine)
    {
        
    }

    public override void OnEnter(GameStateMachine stateMachine)
    {
        Debug.Log("GameState : Running ");
        GameManager.instance.SetCursorState(false, CursorLockMode.Locked);
    }

    public override void OnUpdate(GameStateMachine stateMachine)
    { 
        //TO-DO : WHILE GAME IS RUNNING.
    }
}

[System.Serializable]
public struct PausedRigidbodies
{
    public Rigidbody rigidbody;

    public Vector3 prevVelocity;

    public PausedRigidbodies(Rigidbody rigidbody, Vector3 prevVelocity)
    {
        this.rigidbody = rigidbody;
        this.prevVelocity = prevVelocity;
    }
}