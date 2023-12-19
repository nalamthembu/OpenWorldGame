using UnityEngine;

public class CarAudio : MonoBehaviour
{
    private RealisticEngineSound res;

    private VehicleEngine engine;

    private VehicleTransmission transmission;

    [SerializeField] bool simulated = true;

    private void Start()
    {
        res = GetComponentInChildren<RealisticEngineSound>();

        engine = GetComponent<VehicleEngine>();

        transmission = GetComponent<VehicleTransmission>();

        res.maxRPMLimit = engine.MaxRPM;
    }

    private void Update()
    {
        res.transform.gameObject.SetActive(simulated);

        if (simulated)
        {
            res.engineCurrentRPM = engine.RPM;
            res.gasPedalPressing = engine.Throttle != 0;
            //res.isReversing = transmission.IsInReverse;
        }
    }
}