using UnityEngine;

public class PlayerVehicleInput : MonoBehaviour
{
    private VehicleInput input;

    public bool playerControlEnabled;

    public Transform camera_Focus;

    private void Awake()
    {
        input = GetComponent<VehicleInput>();
    }

    private void Update()
    {
        if (!playerControlEnabled)
            return;

        input.RawThrottle = Input.GetAxis("Vertical");
        input.Throttle = Input.GetAxis("Vertical") > 0 ? Input.GetAxis("Vertical") : Mathf.Lerp(input.Throttle, 0, Time.deltaTime * 2F);
        input.Brake = Input.GetAxis("Vertical") < 0 ? Mathf.Abs(Input.GetAxis("Vertical")) : Mathf.Lerp(input.Brake, 0, Time.deltaTime * 2F);
        input.Steering = Input.GetAxis("Horizontal");
        input.Handbrake = Input.GetAxis("Jump");
        if (Input.GetKeyDown(KeyCode.R))
            input.IsInReverse = !input.IsInReverse;
    }
}
