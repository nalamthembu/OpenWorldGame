using UnityEngine;

public class VehicleVisuals : MonoBehaviour
{
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
    }

    private void FixedUpdate()
    {

    }

    private void HandleTailLights()
    {
    }

    private void HandleHeadLights()
    {

        //TO-DO : MANIPULATE EMSSIVE MATERIALS
    }
}
