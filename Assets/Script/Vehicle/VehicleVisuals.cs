using UnityEngine;

public class VehicleVisuals : MonoBehaviour
{
    [SerializeField] Light[] tailLights;
    [SerializeField] Light[] headLights;

    [SerializeField]
    [Range(0, 1)]
    private float minTailLightIntensity = 0.25F;

    [SerializeField]
    [Range(0, 1)]
    private float tailLightResponseTime = 0.25F;

    [SerializeField]
    [Range(0, 25)]
    private float actualTailLightMaxIntensity = 10;

    [SerializeField]
    [Range(0, 25)]
    private float actualHeadLightMaxIntensity = 10;

    private VehicleInput input;

    float tailLightVelocity;

    private void Awake()
    {
        input = GetComponent<VehicleInput>();

        if (tailLights.Length <= 0)
            Debug.LogWarning("Taillights not assigned to : " + transform.name);

        if (headLights.Length <= 0)
            Debug.LogWarning("Headlights not assigned to " + transform.name);
    }

    private void FixedUpdate()
    {
        //TO-DO : MANIPULATE EMISSIVE MATERIALS BASE ON VEHICLE INPUT

        if (tailLights.Length > 0)
        {
            HandleTailLights();
        }

        if (headLights.Length > 0)
        {
            HandleHeadLights();
        }
    }


    private void HandleTailLights()
    {
        for (int i = 0; i < tailLights.Length; i++)
        {
            float finalIntensity = input.Brake;

            tailLights[i].intensity = 
                Mathf.SmoothDamp
                (
                    tailLights[i].intensity,
                    
                        Mathf.Clamp
                        (   
                            finalIntensity,
                            minTailLightIntensity,
                            1
                        ) * actualHeadLightMaxIntensity,

                    ref tailLightVelocity, 
                    tailLightResponseTime
                );
        }
    }

    private void HandleHeadLights()
    {
        //TO-DO : MANIPULATE EMSSIVE MATERIALS
    }
}
