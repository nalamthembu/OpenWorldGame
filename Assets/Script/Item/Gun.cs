using UnityEngine;

public class Gun : Weapon, IGun
{
    [SerializeField] protected Transform m_LeftHandIKTransform;
    [SerializeField] protected Transform m_BulletSpawn;
    public Transform LeftHandIK { get { return m_LeftHandIKTransform; } }

    public Transform BulletSpawnTransform { get { return m_BulletSpawn; } }

    public GunData GunData;

    [SerializeField] DebugGun debug;

    protected int m_CurrentClip;
    protected int m_RemainingAmmo;

    private float m_NextTimeToFire = 0;

    Camera m_MainCamera;

    private AudioSource m_ShotSource, m_TailSource;

    protected override void Awake()
    {
        base.Awake();

        if (m_WeaponData != null && m_WeaponData is GunData gunData)
        {
            GunData = gunData;
        }

        m_MainCamera = Camera.main;

        InitialiseSoundSources();

        debug = new(this);
    }

    private void InitialiseSoundSources()
    {
        m_ShotSource = gameObject.AddComponent<AudioSource>();
        m_ShotSource.spatialBlend = 1;
        m_ShotSource.playOnAwake = false;
        m_ShotSource.minDistance = 10;
        m_ShotSource.maxDistance = 500;

        m_TailSource = gameObject.AddComponent<AudioSource>();
        m_TailSource.spatialBlend = 1;
        m_TailSource.playOnAwake = false;
        m_TailSource.minDistance = 10;
        m_TailSource.maxDistance = 500;
    }

    private void Update()
    {
        if (m_IsPickedUp)
        {
            if (!m_ShotSource.enabled)
                m_ShotSource.enabled = true;

            if (m_TailSource.enabled)
                m_TailSource.enabled = true;

            if (m_IsEquipped)
            {
                //Check parent
                if (transform.parent != m_Owner.Animator.GetBoneTransform(HumanBodyBones.RightHand))
                    transform.parent = m_Owner.Animator.GetBoneTransform(HumanBodyBones.RightHand);


                //if owner is not aiming.
                if (!m_Owner.IsAiming)
                {
                    transform.SetLocalPositionAndRotation(
                    GunData.relaxedPosRot.position,
                    Quaternion.Euler(GunData.relaxedPosRot.rotation)
                    );
                }

                //If owner is aiming.
                if (m_Owner.IsAiming)
                {
                    transform.SetLocalPositionAndRotation(
                    GunData.aimingPosRot.position,
                    Quaternion.Euler(GunData.aimingPosRot.rotation)
                    );

                    //Firing weapon
                    if (m_Owner.IsFiring)
                    {
                        if (Time.time >= m_NextTimeToFire)
                        {
                            m_NextTimeToFire = Time.time + 1.0F / GunData.fireRate;

                            Fire();
                        }
                    }
                }
            }
            else
            {
                //Check parent
                switch (m_WeaponData.weaponType)
                {
                    case WeaponType.PRIMARY:
                        if (transform.parent != m_Owner.WeaponInventory.PrimaryWeaponHolster)
                            transform.parent = m_Owner.WeaponInventory.PrimaryWeaponHolster;
                        break;

                    case WeaponType.SECONARY:
                        if (transform.parent != m_Owner.WeaponInventory.SecondaryWeaponHolster)
                            transform.parent = m_Owner.WeaponInventory.SecondaryWeaponHolster;
                        break;
                }

                transform.SetLocalPositionAndRotation(
                    WeaponData.holsteredVector.position,
                    Quaternion.Euler(WeaponData.holsteredVector.rotation)
                    );
            }
        }
    }

    public void AddAmmo(int amount) => m_RemainingAmmo += amount;

    public void Fire()
    {
        if (ObjectPoolManager.Instance != null)
        {
            GameObject GOBullet = GunData.gunType switch
            {
                //Assuming both pistols and smgs use 9mm bullets.
                GunType.Pistol or GunType.SMG => ObjectPoolManager.Instance.GetPool("Bullet_Handgun").GetGameObject(),
                _ => ObjectPoolManager.Instance.GetPool("Bullet_Rifle").GetGameObject(),
            };

            GOBullet.transform.position = m_BulletSpawn.position;
            GOBullet.transform.forward = m_MainCamera.transform.forward;
            Bullet CBullet = GOBullet.GetComponent<Bullet>();
            CBullet.InitialiseBullet(100, GunData.damage, GunData.range);

            //Play Sound

            string fireSoundID = GunData.gunType switch
            {
                GunType.Rifle => "WeaponFX_Shot_Rifle",
                GunType.Pistol => "WeaponFX_Shot_Pistol",
                _ => "WeaponFX_Shot_Pistol"
            };

            SoundManager.Instance.PlayInGameSound(fireSoundID, m_ShotSource, false, true);

            //Play Shot tail sound
            string tailSoundID = GunData.gunType switch
            {
                GunType.Rifle => "WeaponFX_Shot_Rifle_Tail",
                GunType.Pistol => "WeaponFX_Shot_Pistol_Tail",
                _ => "WeaponFX_Shot_Pistol_Tail"
            };

            SoundManager.Instance.PlayInGameSound(tailSoundID, m_TailSource, false, true, true);

            m_CurrentClip--;
        }
    }

    private void OnDrawGizmos()
    {
        debug.OnDrawGizmos();
    }
}

[System.Serializable]
public class DebugGun
{
    public bool enabled;

    public bool drawBulletStraightTrejectory;

    private Gun gun;

    private Camera mainCamera;

    public  DebugGun(Gun gun)
    {
        this.gun = gun;

        mainCamera = Camera.main;
    }

    public void OnDrawGizmos()
    {
        if (!enabled)
            return;

        if (drawBulletStraightTrejectory)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(gun.BulletSpawnTransform.position, gun.BulletSpawnTransform.position + mainCamera.transform.forward * gun.GunData.range);
        }
    }
}