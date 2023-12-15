using UnityEngine;

public class PlayerVehicleInput : MonoBehaviour
{
    private VehicleInput input;

    public bool enablePlayerControl;

    public Transform camera_Focus;

    private void Awake()
    {
        input = GetComponent<VehicleInput>();
    }

    private void Update()
    {
        if (!enablePlayerControl)
            return;

        input.Throttle = Input.GetAxis("Vertical") > 0 ? Input.GetAxis("Vertical") : Mathf.Lerp(input.Throttle, 0, Time.deltaTime * 2F);
        input.Brake = Input.GetAxis("Vertical") < 0 ? -Input.GetAxis("Vertical") : Mathf.Lerp(input.Brake, 0, Time.deltaTime * 2F);
        input.Steering = Input.GetAxis("Horizontal");
        input.Handbrake = Input.GetAxis("Jump");
    }
}
