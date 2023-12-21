using UnityEngine;

public class VehicleVisuals : MonoBehaviour
{

    [SerializeField] Indicators indicators;

    [SerializeField] Headlights headlights;

    [SerializeField] Taillights tailLights;

    private VehicleInput input;

    private VehicleEngine engine;

    private Vehicle vehicle;

    private Material mat;

    private void Start()
    {
        vehicle = GetComponent<Vehicle>();

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

                indicators.Initialise(mat);

                headlights.Initialise(mat);

                tailLights.Initialise(mat, input);

                return;
            }
        }

        indicators.alarmSensesDisturbance = vehicle.securitySystem.DetectedDisturbance;
        headlights.alarmSensesDisturbance = vehicle.securitySystem.DetectedDisturbance;
        tailLights.alarmSensesDisturbance = vehicle.securitySystem.DetectedDisturbance;

        tailLights.enabled = engine.IsRunning;

        indicators.Update();
        headlights.Update();
        tailLights.Update();
    }
}

[System.Serializable]
public struct Indicators
{
    public bool leftIndicator;

    public bool rightIndicator;

    public bool hazards;

    public float indicatorDelay;

    [SerializeField] float indicatorIntensity;

    [HideInInspector] public bool alarmSensesDisturbance;
    [HideInInspector] public bool leftIndicatorOn;
    [HideInInspector] public bool rightIndicatorOn;

    float leftIndicatorTimer;
    float rightIndicatorTimer;
    float hazardTimer;

    Material mat;

    public void Initialise(Material mat)
    {
        this.mat = mat;

        mat.SetFloat("_IndicatorIntensity", indicatorIntensity);
    }

    public void Update()
    {
        if (alarmSensesDisturbance)
        {
            AlarmMode();
            return;
        }

        if (hazards)
        {
            hazardTimer += Time.deltaTime;

            if (hazardTimer >= indicatorDelay)
            {
                leftIndicatorOn = !leftIndicatorOn;
                rightIndicatorOn = !rightIndicatorOn;

                hazardTimer = 0;
            }

            mat.SetFloat("_LeftIndicator", leftIndicatorOn ? 1 : 0);
            mat.SetFloat("_RightIndicator", rightIndicatorOn ? 1 : 0);

            return;
        }

        if (leftIndicator)
        {
            leftIndicatorTimer += Time.deltaTime;

            if (leftIndicatorTimer >= indicatorDelay)
            {
                leftIndicatorOn = !leftIndicatorOn;

                leftIndicatorTimer = 0;
            }
        }

        if (rightIndicator)
        {
            rightIndicatorTimer += Time.deltaTime;

            if (rightIndicatorTimer >= indicatorDelay)
            {
                rightIndicatorOn = !rightIndicatorOn;

                rightIndicatorTimer = 0;
            }
        }

        if (!leftIndicator)
        {
            leftIndicatorOn = false;
            leftIndicatorTimer = 0;
        }


        if (!rightIndicator)
        {
            rightIndicatorOn = false;
            rightIndicatorTimer = 0;
        }


        mat.SetFloat("_RightIndicator", rightIndicatorOn ? 1 : 0);
        mat.SetFloat("_LeftIndicator", leftIndicatorOn ? 1 : 0);

    }

    private void AlarmMode()
    {
        hazardTimer += Time.deltaTime;

        if (hazardTimer >= indicatorDelay)
        {
            leftIndicatorOn = !leftIndicatorOn;
            rightIndicatorOn = !rightIndicatorOn;

            hazardTimer = 0;
        }

        mat.SetFloat("_LeftIndicator", leftIndicatorOn ? 1 : 0);
        mat.SetFloat("_RightIndicator", rightIndicatorOn ? 1 : 0);
    }
}

[System.Serializable]
public class Headlights
{
    [SerializeField]
    [Range(0, 25)] float maxHeadLightIntensity;

    [SerializeField]
    [Range(0, 25)] float minHeadlightIntensity;

    [SerializeField] Light leftHeadLight, rightHeadLight;

    [SerializeField][Range(0, 10)] float minLightSourceBrightness, maxLightSourceBrightness;

    [SerializeField] float alarmModeDelay;

    private float alarmModeTimer;

    Material mat;

    public bool enabled;

    public bool brights;

    [HideInInspector] public bool alarmSensesDisturbance;

    public void Initialise(Material mat) => this.mat = mat;

    public void Update()
    {
        if (alarmSensesDisturbance)
        {
            AlarmMode();

            return;
        }

        if (enabled)
        {
            leftHeadLight.enabled = rightHeadLight.enabled = true;
            leftHeadLight.intensity = rightHeadLight.intensity = Mathf.Lerp(minLightSourceBrightness, maxLightSourceBrightness, brights ? 1 : 0);
            mat.SetFloat("_HeadLightBrightness", Mathf.Lerp(minHeadlightIntensity, maxHeadLightIntensity, brights ? 1 : 0));
        }
        else
        {
            leftHeadLight.enabled = rightHeadLight.enabled = false;
            mat.SetFloat("_HeadLightBrightness", 0);
        }
    }

    public void AlarmMode()
    {
        alarmModeTimer += Time.deltaTime;

        if (alarmModeTimer >= alarmModeDelay)
        {
            leftHeadLight.enabled = !leftHeadLight.enabled;
            rightHeadLight.enabled = !rightHeadLight.enabled;
            alarmModeTimer = 0;
        }

        mat.SetFloat("_HeadLightBrightness", leftHeadLight.enabled || rightHeadLight.enabled ? maxHeadLightIntensity : 0);
    }
}

[System.Serializable]
public class Taillights
{
    [SerializeField]
    [Range(0, 25)] float maxTailLightIntensity;

    [SerializeField]
    [Range(0, 25)] float minTailLightIntensity;

    [SerializeField] Light leftTailLight, rightTailLight;

    [SerializeField][Range(0, 10)] float minLightSourceBrightness, maxLightSourceBrightness;

    [SerializeField] float alarmModeDelay;

    private float alarmModeTimer;

    Material mat;

    public bool enabled;

    private VehicleInput input;

    [HideInInspector] public bool alarmSensesDisturbance;

    public void Initialise(Material mat, VehicleInput input)
    {
        this.mat = mat;
        this.input = input;
    }

    public void Update()
    {
        if (alarmSensesDisturbance)
        {
            AlarmMode();

            return;
        }

        if (enabled)
        {
            leftTailLight.enabled = rightTailLight.enabled = true;
            rightTailLight.intensity = leftTailLight.intensity = Mathf.Lerp(minLightSourceBrightness, maxLightSourceBrightness, input.Brake);
            mat.SetFloat("_Braking", Mathf.Lerp(minTailLightIntensity, maxTailLightIntensity, input.Brake));
        }
        else
        {
            leftTailLight.enabled = rightTailLight.enabled = false;
            mat.SetFloat("_Braking", 0);
        }
    }

    public void AlarmMode()
    {
        alarmModeTimer += Time.deltaTime;

        if (alarmModeTimer >= alarmModeDelay)
        {
            leftTailLight.enabled = !leftTailLight.enabled;
            rightTailLight.enabled = !rightTailLight.enabled;
            alarmModeTimer = 0;
        }

        mat.SetFloat("_Braking", leftTailLight.enabled || rightTailLight.enabled ? maxTailLightIntensity : 0);
    }
}