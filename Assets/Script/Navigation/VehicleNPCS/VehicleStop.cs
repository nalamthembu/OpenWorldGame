using UnityEngine;


public class VehicleStop : VehicleWaypoint
{
    [SerializeField] float StopTimer = 0;

    [SerializeField] TrafficStopStatus status;

    public TrafficStopStatus GetStatus() => status;

    public override void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 1);

        Gizmos.DrawSphere(transform.position, 0.25f);

        if (drawPath)
        {
            Gizmos.color = Color.red;

            for (int i = 0; i < nextCheckpoints.Length; i++)
            {
                if (nextCheckpoints[i] is null)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(transform.position, transform.up);
                    continue;
                }

                Gizmos.DrawLine(transform.position, nextCheckpoints[i].transform.position);
            }


        }
    }
}

public enum TrafficStopStatus
{
    STOP,
    AMBER,
    GO
}