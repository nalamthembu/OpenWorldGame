using UnityEngine;
using UnityEngine.UI;

public class WeaponHUD : BaseHUD
{
    [Header("----------Crosshair----------")]
    [SerializeField] Image m_CrosshairRegular; //The regular crosshair
    [SerializeField] Image m_CrosshairHit; //The crosshair sprite when we hit something.

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    private void Update()
    {
        ProcessAimInput();
    }

    private void ProcessAimInput()
    {
        m_CrosshairRegular.gameObject.SetActive(PlayerCharacter.Instance && PlayerCharacter.Instance.IsAiming);

        if (m_CrosshairHit.color.a >= 1)
        {
            m_CrosshairHit.CrossFadeAlpha(0, 0.25F, false);
        }
    }
}