using UnityEngine;

//Place this on a door transform.
public class DoorInteraction : Interactable, IInteractable
{
    const string PLAYER_TAG = "Player";

    [Header("This refers to the door hinge")]
    [SerializeField] Transform doorPivot;
    
    //Start the door of closed.
    DOOR_STATE doorState = DOOR_STATE.CLOSED;

    public override void OnTriggerStay(Collider other)
    {
        //GETS THE PLAYER
        if (other.CompareTag(PLAYER_TAG))
        {
            //TRYS TO GET THE PLAYER INPUT SCRIPT
            PlayerInput playerInput = other.GetComponent<PlayerInput>();

            //CHECKS FOR INPUT
            if (playerInput.Interact)
            {
                //CALLS THE TRIGGER_INTERACTABLE_METHOD
                TriggerInteractable();
            }
        }
    }

    public override void TriggerInteractable()
    {
        switch(doorState)
        {
            //This assumes the door is already open.
            case DOOR_STATE.OPEN:

                //PUT LOGIC FOR CLOSING DOORS IN HERE

                doorPivot.eulerAngles = Vector3.up * -90;

                //Switch to closed door state.
                doorState = DOOR_STATE.CLOSED;

                //TO-DO : PLAYSOUND (DOOR_CLOSE)

                break;

                //This assumes the door is closed.
            case DOOR_STATE.CLOSED:

                //PUT LOGIC FOR OPENING DOORS IN HERE
                doorPivot.eulerAngles = Vector3.up * 0;

                //Switch to open door state.
                doorState = DOOR_STATE.OPEN;

                //TO-DO : PLAYSOUND (DOOR_OPEN)

                break;
        }
    }
}

public enum DOOR_STATE
{
    OPEN,
    CLOSED
}