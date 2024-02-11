using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Rigidbody))]
public class Gun : Weapon
{
    [Header("----------General----------")]
    [SerializeField] Transform m_LeftHandIK;
    [SerializeField] Transform m_BulletSpawn;
    [SerializeField] ParticleSystem m_MuzzleFlash;
    [SerializeField] ParticleSystem m_BulletShellFX;

    AudioSource m_ShotFireSource;
    AudioSource m_ShotTailSource;
    AudioSource m_ShotIndoorTailSource;
    AudioSource m_SweetenerSource;

    protected int m_CurrClip, m_RemainingAmmo;
    private GunData m_GunData;
    private bool m_IsReloading;

    public Transform LeftHandIK { get { return m_LeftHandIK; } }
    public bool IsReloading { get { return m_IsReloading; } }

    //Waiting for the character to let go of the trigger (semi-autos only)
    private bool m_WaitingForTriggerReset;

    private float m_TimeForNextShot;

    protected override void Awake()
    {
        base.Awake();

        if (m_WeaponData != null)
            m_GunData = (GunData)m_WeaponData;

        InitialiseAudioSources();
    }

    protected override void Start()
    {
        if (Owner == null)
        {
            m_RemainingAmmo = Random.Range(0, 200);

            Reload();
        }
    }

    protected virtual void InitialiseAudioSources()
    {
        m_ShotFireSource = gameObject.AddComponent<AudioSource>();
        m_ShotFireSource.playOnAwake = false;
        m_ShotTailSource = gameObject.AddComponent<AudioSource>();
        m_ShotTailSource.playOnAwake = false;
        m_SweetenerSource = gameObject.AddComponent<AudioSource>();
        m_SweetenerSource.playOnAwake = false;
        m_ShotIndoorTailSource = gameObject.AddComponent<AudioSource>();
        m_ShotIndoorTailSource.playOnAwake = false;
    }

    public void Reload()
    {
        if (m_RemainingAmmo <= 0) //There's no ammo left.
            return;

        if (m_RemainingAmmo >= m_GunData.MaxClip)
        {
            m_CurrClip += m_GunData.MaxClip;

            m_RemainingAmmo -= m_CurrClip;
        }
        else
        {
            m_CurrClip = m_RemainingAmmo;
            m_RemainingAmmo -= m_CurrClip;
        }
    }

    [ContextMenu("[DEBUG]/(Cheat) Add 9999 Ammo")]
    public virtual void Debug_GiveLotsOfAmmo()
    {
        m_RemainingAmmo += 9999;
        Reload();
    }

    protected override void Update()
    {
        base.Update();

        if (m_CurrClip <= 0 && m_RemainingAmmo > 0)
            Reload();

        //CHECK IF EQUIPPED AND OWNER IS AIMING
        if (Owner != null && Owner is BaseCharacter ownerCharacter && IsEquipped && ownerCharacter.IsAiming)
        {
            //IF THIS WEAPON IS SEMI-AUTO & IF THE OWNER IS FIRING, FIRE.
            //ELSE IF THIS WEAPON IS AUTO & OWNER IS FIRING, FIRE AT FIRE RATE.

            if (ownerCharacter.IsFiring)
            {
                switch (m_GunData.FireType)
                {
                    case FireType.Semi_Auto:
                        if (!m_WaitingForTriggerReset)
                        {
                            Fire();
                            m_WaitingForTriggerReset = true;
                        }
                        break;


                    case FireType.Full_Auto:

                        if (m_TimeForNextShot <= 0)
                        {
                            Fire();

                            m_TimeForNextShot = m_GunData.FireRateInSeconds;
                        }

                        m_TimeForNextShot -= Time.deltaTime;

                        break;
                }
            }
            else
            {
                //Reset trigger if the character stops shooting
                m_WaitingForTriggerReset = false;
            }
        }
        else
        {
            //Reset trigger regardless of if the character stops shooting or aiming.
            m_WaitingForTriggerReset = false;
        }
    }

    [ContextMenu("[DEBUG]/Fire Shot")]
    public void Fire()
    {
        if (m_IsReloading)
            return;

        //TODO : IF AMMO <= 0 & NOT ISRELOADING THEN RELOAD,
        //TODO : SET ISRELOADING = TRUE, RETURN

        if (m_CurrClip > 0)
        {
            //FIRE A PROJECTILE
            if (m_GunData.BulletPrefab)
            {
                if (!m_BulletSpawn)
                {
                    Debug.LogError("There is no bullet spawn point assigned to this weapon : " + gameObject.name);
                    return;
                }

                Vector3 target = m_BulletSpawn.forward * m_GunData.range;

                if (Owner == PlayerCharacter.Instance)
                {
                    if (ThirdPersonCamera.Instance)
                    {
                        Vector3 TPS_CAM_POS = ThirdPersonCamera.Instance.CameraComponent.transform.position;

                        Vector3 TPS_CAM_FWD_DIR = ThirdPersonCamera.Instance.CameraComponent.transform.forward;

                        Vector3 LINE_CAST_ENDPOINT = TPS_CAM_POS + TPS_CAM_FWD_DIR * m_GunData.range;

                        if (Physics.Linecast(m_BulletSpawn.position, LINE_CAST_ENDPOINT, out var hit))
                            target = hit.point;
                        else
                            target = TPS_CAM_FWD_DIR * m_GunData.range;
                    }
                }

                Vector3 lookDir = (target - m_BulletSpawn.position).normalized;

                GameObject bulletGO = Instantiate(m_GunData.BulletPrefab, m_BulletSpawn.position, Quaternion.LookRotation(lookDir));

                Debug.DrawRay(m_BulletSpawn.position, lookDir, Color.red);

                if (bulletGO.TryGetComponent<Bullet>(out var bullet))
                {
                    bullet.InitialiseProjectile(Owner, m_WeaponData.damage, m_GunData.range);
                }

                if (m_MuzzleFlash != null)
                {
                    m_MuzzleFlash.Play();

                    if (m_BulletShellFX != null)
                    {
                        m_BulletShellFX.Play();
                    }
                    else
                    {
                        Debug.LogError("There is no bulletshell fx game object attached! : " + m_WeaponData.weaponName);
                    }
                }
                else
                {
                    Debug.LogError("There is no muzzle flash game object attached! : " + m_WeaponData.weaponName);
                }

                //SUBTRACT AMMO
                m_CurrClip--;

                //PLAY SOUND
                if (SoundManager.Instance)
                {
                    //Play some ear candy for the player.
                    if (Owner == PlayerCharacter.Instance)
                    {
                        SoundManager.Instance.PlayInGameSound("Generic_Sub_Sweeteneer", m_SweetenerSource, false, true, true, 1.5F);
                    }

                    //Play Fire Sound
                    SoundManager.Instance.PlayInGameSound(m_GunData.fireShotAudioID, m_ShotFireSource, false, true, false, 1.0F);

                    //PLAY INDOOR TAIL IF THERES SOMETHING within 100m ABOVE OUR HEADS
                    if (Physics.Linecast(transform.position, transform.position + transform.up * 100))
                    {
                        //Play outdoor tail at a really low attenuation
                        SoundManager.Instance.PlayInGameSound(m_GunData.fireShotOutDoorTailID, m_ShotTailSource, false, true, false, 0.25F);

                        //Play indoor tail
                        SoundManager.Instance.PlayInGameSound(m_GunData.fireShotIndoorTailID, m_ShotIndoorTailSource, false, true, false, 0.5F);
                    }
                    else
                    { 
                        //PLAY OUT DOOR TAIL IF THERES NOTHING ABOVE US (This could also mean we're in a really big room)
                        SoundManager.Instance.PlayInGameSound(m_GunData.fireShotOutDoorTailID, m_ShotTailSource, false, true, false);
                    }
                }

                //If the gun isn't picked up, there should be some visible kickback (assuming the gun misfired)
                if (m_RigidBody && Owner == null)
                {
                    m_RigidBody.AddForce(-m_BulletSpawn.forward * 2, ForceMode.Impulse);
                }
            }
        }

    }

    public void AddAmmo(int amount) => m_RemainingAmmo += amount;

    /// <summary>
    /// This method returns all the remaining ammo in this weapon, calling this method will deplete this weapon of all its ammo.
    /// </summary>
    /// <returns></returns>
    public int TakeAllAmmo()
    {
        int allRemainingAmmoToBeReturned = m_RemainingAmmo;

        m_RemainingAmmo = 0;

        return allRemainingAmmoToBeReturned;
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        //If the gun hits the ground hard, there should be a slight chance it misfires.
        if (collision.relativeVelocity.magnitude > 10)
        {
            bool willMisFire = Random.Range(0, 1) <= 0.50; /*%*/

            if (willMisFire)
            {
                Fire();
            }
        }
    }

    protected override void DoPlayerPickUp()
    {
        base.DoPlayerPickUp();

        //Reload weapon onPickup

        if (m_CurrClip <= 0)
            Reload();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Gun))]
public class GunEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Gun pickup = (Gun)target;

        if (pickup.m_PickupBehaviour == PickupBehaviour.BobAndSpin)
        {
            pickup.m_SpinRate = EditorGUILayout.FloatField("Spin Rate", pickup.m_SpinRate);
            pickup.m_OscillationSpeed = EditorGUILayout.FloatField("Spin Rate", pickup.m_OscillationSpeed);
            pickup.m_OscillationSpeed = EditorGUILayout.FloatField("Height Oscillation", pickup.m_HeightOscillationRate);
        }
    }
}
#endif