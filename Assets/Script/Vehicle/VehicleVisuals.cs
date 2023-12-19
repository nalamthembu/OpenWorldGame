using UnityEngine;

public class VehicleVisuals : MonoBehaviour
{
    [SerializeField]
    [Range(0, 25)]
    private float minTailLightIntensity = 0.25F;

    [SerializeField]
    [Range(0, 25)]
    private float maxTailLightIntensity = 10;

    [SerializeField]
    [Range(0, 25)]
    private float maxHeadLightIntensity = 10;

    [SerializeField]
    [Range(0, 25)]
    private float minHeadlightIntensity = 10;

    [SerializeField] Light[] headLightLightSources;

    private VehicleInput input;

    private VehicleEngine engine;

    private Material mat;

    private bool AllHeadLightSourcesEnabled;

    private void Start()
    {
        input = GetComponent<VehicleInput>();
    }

    private void Update()
    {
        if (engine is null)
        {
            engine = GetComponent<VehicleEngine>();
            return;
        }

        if (mat is null)
        {
            if (TryGetComponent<MeshRenderer>(out var meshRenderer))
            {
                mat = meshRenderer.material;

                return;
            }
        }

        HandleHeadLights();
        HandleTailLights();

    }

    private void HandleTailLights()
    {
        if (engine.IsRunning)
        {
            mat.SetFloat("_Braking", Mathf.Lerp(minTailLightIntensity, maxTailLightIntensity, input.Brake));
        }
        else
        {
            mat.SetFloat("_Braking", 0);
        }

    }

    private void HandleHeadLights()
    {
        if (engine.IsRunning)
        {
            mat.SetFloat("_HeadLightBrightness", minHeadlightIntensity);
            if (!AllHeadLightSourcesEnabled)
            {
                int numberOfEnabledSources = 0;

                for (int i = 0; i < headLightLightSources.Length; i++)
                {
                    headLightLightSources[i].enabled = true;

                    numberOfEnabledSources++;
                }

                AllHeadLightSourcesEnabled = numberOfEnabledSources >= headLightLightSources.Length;
            }
        }
        else
        {
            mat.SetFloat("_HeadLightBrightness", 0);
            for (int i = 0; i < headLightLightSources.Length; i++)
            {
                headLightLightSources[i].enabled = false;
            }

            AllHeadLightSourcesEnabled = false;
        }


    }
}
