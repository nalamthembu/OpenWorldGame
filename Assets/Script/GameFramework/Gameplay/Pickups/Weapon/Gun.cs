﻿using UnityEngine;
using System.Collections;
using System;
using Random = UnityEngine.Random;
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
    [SerializeField] float m_Spread = 0;

    AudioSource m_ShotFireSource;
    AudioSource m_ShotTailSource;
    AudioSource m_SweetenerSource;

    protected int m_CurrClip, m_RemainingAmmo;
    private GunData m_GunData;
    private bool m_IsReloading;

    public Transform LeftHandIK { get { return m_LeftHandIK; } }
    public bool IsReloading { get { return m_IsReloading; } }

    //This gun can be reloaded if the current clip is less than the max & we have additional ammo to fill that clip.
    public bool CanReload { get { return m_CurrClip < m_GunData.MaxClip && m_RemainingAmmo > 0; } }

    //Is there anything left in the clip?
    public bool ShouldReload { get { return m_CurrClip <= 0; } }

    //There is a bullet in the clip
    public bool HasBulletsInClip { get { return m_CurrClip > 0; } }

    //Waiting for the character to let go of the trigger (semi-autos only)
    private bool m_WaitingForTriggerReset;

    public static event Action<Gun> OnReload;
    public static event Action<Gun> OnDoneReloading;
    public static event Action<Gun> OnGunIsEmpty;

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
        base.Start();

        if (Owner == null)
        {
            m_RemainingAmmo = Random.Range(30, 900);

            Reload();
        }
    }

    protected virtual void InitialiseAudioSources()
    {
        //Shot Fire
        m_ShotFireSource = gameObject.AddComponent<AudioSource>();
        m_ShotFireSource.outputAudioMixerGroup = m_GunData.mixerGroup;
        m_ShotFireSource.playOnAwake = false;
        m_ShotFireSource.spatialBlend = 1;
        m_ShotFireSource.volume = 1;

        //Shot Tail
        m_ShotTailSource = gameObject.AddComponent<AudioSource>();
        m_ShotTailSource.outputAudioMixerGroup = m_GunData.mixerGroup;
        m_ShotTailSource.playOnAwake = false;
        m_ShotTailSource.volume = 0.75F;

        //Sweetener
        m_SweetenerSource = gameObject.AddComponent<AudioSource>();
        m_SweetenerSource.outputAudioMixerGroup = m_GunData.mixerGroup;
        m_SweetenerSource.playOnAwake = false;
        m_SweetenerSource.volume = 1;
    }

    public void Reload()
    {
        if (m_IsReloading)
            return;

        if (!CanReload)
        { //theres nothing left.
            OnGunIsEmpty?.Invoke(this);
            return;
        }

        StartCoroutine(ReloadSequence(m_GunData.ReloadTime));
    }

    protected virtual IEnumerator ReloadSequence(float reloadDelay)
    {
        if (m_IsReloading)
            yield return null;

        float timer = 0;

        OnReload?.Invoke(this);

        m_IsReloading = true;

        while (timer < reloadDelay)
        {
            timer += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        m_IsReloading = false;

        if (ShouldReload && CanReload)
        {
            if (m_RemainingAmmo >= m_GunData.MaxClip)
            {
                m_CurrClip = m_GunData.MaxClip;

                m_RemainingAmmo -= m_CurrClip;

                OnDoneReloading?.Invoke(this);

                yield return null;
            }

            if (m_RemainingAmmo <= m_GunData.MaxClip)
            {
                m_CurrClip = m_RemainingAmmo;

                m_RemainingAmmo -= m_CurrClip;

                OnDoneReloading?.Invoke(this);

                yield return null;
            }
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

        //CHECK IF EQUIPPED AND OWNER IS AIMING
        if (Owner != null && Owner is BaseCharacter ownerCharacter && IsEquipped && ownerCharacter.IsAiming)
        {
            if (ShouldReload && CanReload)
                Reload();

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

        if (HasBulletsInClip)
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
                        //Middle of the screen.
                        Ray ray = ThirdPersonCamera.Instance.CameraComponent.ViewportPointToRay(new(0.5f, 0.5f));

                        if (Physics.Raycast(ray, out var hit))
                            target = hit.point;
                        else
                            target = ray.GetPoint(m_GunData.range);
                    }
                }

                Vector3 lookDir = (target - m_BulletSpawn.position).normalized;

                //Calculate Bullet Spread
                float xSpread = Random.Range(-m_Spread, m_Spread);
                float ySpread = Random.Range(-m_Spread, m_Spread);
                Vector3 lookDirWithSpread = lookDir + (Vector3)new(xSpread, ySpread);

                //Spawn Bullet
                GameObject bulletGO = Instantiate(m_GunData.BulletPrefab, m_BulletSpawn.position, Quaternion.identity);
                bulletGO.transform.forward = lookDir;


                if (bulletGO.TryGetComponent<Bullet>(out var bullet))
                {
                    bullet.InitialiseProjectile(Owner, m_WeaponData.damage, m_GunData.range, lookDirWithSpread);
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

                PlayShotSound();

                //If the gun isn't picked up, there should be some visible kickback (assuming the gun misfired)
                if (m_RigidBody && Owner == null)
                {
                    m_RigidBody.AddForce(-m_BulletSpawn.forward * 2, ForceMode.Impulse);
                }
            }
        }
        else
        {
            Reload();
        }
    }

    protected virtual void PlayShotSound()
    {
        //Detect Occulusion
        Vector3 startPosition = m_BulletSpawn.position + m_BulletSpawn.forward;
        Vector3 endPosition = m_BulletSpawn.position + Vector3.up * 100;
        bool IsThereASurfaceAboveTheGun = Physics.Linecast(startPosition, endPosition, out _, -1, QueryTriggerInteraction.Ignore);

        //Set Clips
        m_ShotFireSource.clip = m_GunData.GunSound.GetShotClip();
        m_ShotTailSource.clip = IsThereASurfaceAboveTheGun ? m_GunData.GunSound.GetShotIndoorTail() : m_GunData.GunSound.GetShotTail();
        m_SweetenerSource.clip = m_GunData.GunSound.GetSweetener();

        //Randomise Pitch
        m_ShotFireSource.pitch = Random.Range(0.95F, 1.15F);
        m_ShotTailSource.pitch = Random.Range(0.95F, 1.15F);
        m_SweetenerSource.pitch = Random.Range(0.95F, 1.15F);

        //Play Sweetener 
        //Play Gunshot Sound
        //Play Gunshot Tail Sound
        m_ShotFireSource.Play();
        m_ShotTailSource.Play();
        m_SweetenerSource.Play();
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
        if (collision.impulse.magnitude >= 10)
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

        if (ShouldReload && CanReload)
            Reload();
        else
            Debug.Log("The player picked up an empty gun.");
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