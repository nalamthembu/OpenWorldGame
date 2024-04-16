using System;
using UnityEngine;

public class PlayerController : BaseCharacterController
{
    //Stores instance of player input.
    PlayerInput m_PlayerInput;

    //Variables that store input values
    Vector2 m_PlayerMovement;
    Vector2 m_CameraXY;
    bool m_MovePressed;
    bool m_RunPressed;
    bool m_CameraMoved;
    bool m_IsCrouched;
    bool m_IsProne;
    bool m_JumpPressed;
    bool m_PickUpItemPressed;
    bool m_InventoryPressed;
    bool m_InventoryHeld;
    bool m_IsAiming;
    bool m_IsFiring;

    public bool PickUpItemPressed { get { return m_PickUpItemPressed; } }
    public bool MovementInputPressed { get { return m_MovePressed; } }
    public bool RunHeld { get { return m_RunPressed; } }
    public bool CameraMoved { get { return m_CameraMoved; } }
    public bool IsCrouched { get { return m_IsCrouched; } }
    public bool IsProne { get { return m_IsProne; } }
    public bool JumpPressed { get { return m_JumpPressed; } }
    public bool InventoryPressed { get { return m_InventoryPressed; } }
    public bool InventoryHeld { get { return m_InventoryHeld; } }
    public bool IsAiming { get { return m_IsAiming; } }
    public bool IsFiring { get { return m_IsFiring; } }

    public Vector2 CameraXY { get { return m_CameraXY; } }
    public Vector2 PlayerMovement { get { return m_PlayerMovement; } }

    public static PlayerController Instance;

    //Events
    public static event Action OnChangeCameraView;
    public static event Action OnInventoryTap;
    public static event Action OnInventoryHold;
    public static event Action OnInventoryClose;
    public static event Action OnStartAim;
    public static event Action OnStopAim;
    public static event Action OnReload;
    public static event Action OnQuickSwap;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        Cursor.visible = false;

        Cursor.lockState = CursorLockMode.Confined;

        m_PlayerInput = new();

        //Setup callbacks
        PlayerMovementCallBacks();
        CameraMovementCallBacks();
        PlayerInteractionCallBacks();
        WeaponControlCallBacks();
    }

    private void WeaponControlCallBacks()
    {
        //Quick Swap (LB, L1, 2)
        m_PlayerInput.CharacterControls.QuickWeaponSwap.performed += ctx =>
        {
            OnQuickSwap?.Invoke();
        };

        //Reload (Circle / R)
        m_PlayerInput.CharacterControls.Reload.performed += ctx =>
        {
            OnReload?.Invoke();
        };

        //AIMING
        m_PlayerInput.CharacterControls.Aim.performed += ctx =>
        {
            m_IsAiming = true;
            OnStartAim?.Invoke();
        };

        m_PlayerInput.CharacterControls.Aim.canceled += ctx =>
        {
            m_IsAiming = false;
            OnStopAim?.Invoke();
        };

        //Shooting (RT / R2, LMB)
        m_PlayerInput.CharacterControls.Fire.performed += ctx =>
        {
            m_IsFiring = true;
        };

        m_PlayerInput.CharacterControls.Fire.canceled += ctx =>
        {
            m_IsFiring = false;
        };

        //INVENTORY (Tab on PC)- D-Pad/Right on the console.

        //Tap
        m_PlayerInput.CharacterControls.Inventory.started += ctx =>
        {
            m_InventoryPressed = true;
            OnInventoryTap?.Invoke();
        };

        //Hold
        m_PlayerInput.CharacterControls.Inventory.performed += ctx =>
        {
            m_InventoryPressed = false;
            m_InventoryHeld = true;
            OnInventoryHold?.Invoke();
        };

        //Release
        m_PlayerInput.CharacterControls.Inventory.canceled += ctx =>
        {
            m_InventoryPressed = false;
            m_InventoryHeld = false;
            OnInventoryClose?.Invoke();
        };
        //End of inventory controls.
    }

    private void PlayerInteractionCallBacks()
    {
        //Pickup Item
        //E or Circle (PS) or X (Xbox)
        m_PlayerInput.CharacterControls.PickUpItem.performed += ctx =>
        {
            m_PickUpItemPressed = true;
        };
        m_PlayerInput.CharacterControls.PickUpItem.canceled += ctx =>
        {
            m_PickUpItemPressed = false;
        };
        //End of pickup Item.
    }

    private void Update()
    {
        ValidateBools();
    }

    private void ValidateBools()
    {
        if (m_IsProne || m_RunPressed)
            m_IsCrouched = false;
    }

    private void CameraMovementCallBacks()
    {
        //Mouse OR Right Stick (Camera Movement)
        m_PlayerInput.CameraControls.CameraMovement.performed += ctx =>
        {
            m_CameraXY = ctx.ReadValue<Vector2>();
            m_CameraMoved = true;
        };

        m_PlayerInput.CameraControls.CameraMovement.canceled += ctx =>
        {
            m_CameraXY *= 0; //reset movement
            m_CameraMoved = false;
        };

        m_PlayerInput.CameraControls.ChangeView.performed += ctx =>
        {
            OnChangeCameraView?.Invoke();
        };
        //END OF CAMERA MOVEMENT
    }

    private void PlayerMovementCallBacks()
    {
        //WASD OR Left Stick (Player Movement)
        m_PlayerInput.CharacterControls.Movement.performed += ctx =>
        {
            m_PlayerMovement = ctx.ReadValue<Vector2>();
            m_MovePressed = true;
        };

        m_PlayerInput.CharacterControls.Movement.canceled += ctx =>
        {
            m_PlayerMovement *= 0; //reset movement
            m_MovePressed = false;
        };

        //Left Shift OR (A on XBOX - X on PlayStation) (Player Sprint)
        m_PlayerInput.CharacterControls.Sprint.performed += ctx =>
        {
            ctx.ReadValueAsButton();
            m_RunPressed = true;
        };

        m_PlayerInput.CharacterControls.Sprint.canceled += ctx =>
        {
            ctx.ReadValueAsButton();
            m_RunPressed = false;
        };

        m_PlayerInput.CharacterControls.Crouch.performed += ctx =>
        {
            ctx.ReadValueAsButton();
            m_IsCrouched = !m_IsCrouched;
        };

        m_PlayerInput.CharacterControls.Prone.performed += ctx =>
        {
            ctx.ReadValueAsButton();

            if (m_IsProne)
                m_IsCrouched = true;

            m_IsProne = !m_IsProne;
        };

        m_PlayerInput.CharacterControls.Jump.started += ctx =>
        {
            ctx.ReadValueAsButton();
            m_JumpPressed = true;
        };

        m_PlayerInput.CharacterControls.Jump.performed += ctx =>
        {
            ctx.ReadValueAsButton();
            m_JumpPressed = true;
        };

        m_PlayerInput.CharacterControls.Jump.canceled += ctx =>
        {
            ctx.ReadValueAsButton();
            m_JumpPressed = false;
        };

        //END OF PLAYER MOVEMENT
    }

    private void OnEnable() 
    { 
        m_PlayerInput.CharacterControls.Enable();
        m_PlayerInput.CameraControls.Enable();
    }
    private void OnDisable()
    {
        m_PlayerInput.CharacterControls.Disable();
        m_PlayerInput.CameraControls.Disable();
    }
    
}