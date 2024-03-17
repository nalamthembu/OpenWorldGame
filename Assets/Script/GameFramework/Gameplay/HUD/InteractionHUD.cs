using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionHUD : BaseHUD
{
    [Header("-----------General----------")]
    [SerializeField] TMP_Text m_ObjectText;
    [SerializeField] TMP_Text m_OptionsText;
    [SerializeField] Image m_OptionImage; //TODO : FIGURE THIS OUT.
    [SerializeField] ControlSpriteData m_ControlSpriteData;

    protected override void OnEnable()
    {
        BasePickup.OnEnterPickupRadius += OnStartPickupInteraction;
        BasePickup.OnExitPickupRadius += OnEndPickupInteraction;
    }

    protected override void OnDisable()
    {
        BasePickup.OnEnterPickupRadius -= OnStartPickupInteraction;
        BasePickup.OnExitPickupRadius -= OnEndPickupInteraction;
    }

    private void OnEndPickupInteraction(BasePickup pickUp) => m_HUDObject.SetActive(false);
    private void OnStartPickupInteraction(BasePickup pickup)
    {
        m_OptionsText.text = "Pick up";
        if (pickup.PickupData != null)
            m_ObjectText.text = pickup.PickupData.GetPickupName();
        m_HUDObject.SetActive(true);
    }
}
