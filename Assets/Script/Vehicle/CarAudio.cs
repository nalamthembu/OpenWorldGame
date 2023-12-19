using UnityEngine;

public class CarAudio : MonoBehaviour
{
    private RealisticEngineSound res;

    private VehicleEngine engine;

    private VehicleTransmission transmission;

    [SerializeField] bool simulated = true;

    private void Awake()
    {
        res = GetComponentInChildren<RealisticEngineSound>();

        engine = GetComponent<VehicleEngine>();

        res.maxRPMLimit = engine.MaxRPM;

        transmission = engine.GetComponent<VehicleTransmission>();

    }

    private void Update()
    {
        if (GameManager.instance.IsInGarage)
            simulated = false;
        else
            simulated = true;

        res.transform.gameObject.SetActive(simulated);

        if (!simulated)
        {
            res.enabled = false;

            if (res != null)
                res.engineCurrentRPM = engine.RPM; // set Realistic Engine Sound script's current RPM to slider value
        }
        else
        {
            res.engineCurrentRPM = engine.RPM;
            res.gasPedalPressing = engine.Throttle != 0;
            //res.isReversing = transmission.IsInReverse;
        }

    }
}