using System.Collections;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public static HUDManager instance;

    public HUDMiniMap hudMiniMap;

    private PlayerInput input;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        input = FindObjectOfType<PlayerInput>();
    }

    private void Start()
    {
        hudMiniMap.Start();
    }

    private void OnValidate()
    {
        hudMiniMap.OnValidate();
    }

    private void Update()
    {
        //Make sure we have access to player input.
        if (input is null)
        {
            input = FindObjectOfType<PlayerInput>();
        }

        hudMiniMap.Update();
    }

    private IEnumerator SetCanvasGroupVisible(CanvasGroup group, bool isVisible)
    {
        float velocity = 0;

        if (isVisible)
            group.gameObject.SetActive(isVisible);

            while (group.alpha < 1)
            {
                group.alpha += Mathf.SmoothDamp(group.alpha, 1, ref velocity, 0.25F);

                yield return new WaitForEndOfFrame();
            }

            StopCoroutine(SetCanvasGroupVisible(group, isVisible));

        if (!isVisible)
            group.gameObject.SetActive(isVisible);
    }
}

[System.Serializable]
public struct HUDMiniMap
{
    [SerializeField] Camera miniMapCam;
    [SerializeField][Range(1, 1000)] float cameraHeight;
    [SerializeField][Range(1, 1000)] float cameraSize;
    [SerializeField][Range(1, 90)] float cameraAngle;
    [SerializeField] GameObject playerIconPrefab;

    GameObject playerIconSpriteInstance;

    public void Start()
    {
        if (playerIconSpriteInstance is null)
        {
            playerIconSpriteInstance = Object.Instantiate(playerIconPrefab);
        }
    }

    public void Update()
    {
        SetMiniMap(cameraAngle, cameraHeight);
    }

    public void SetMiniMap(float angle, float height)
    {
        //Follow Player Position.
        miniMapCam.transform.position = Vector3.up * height + PlayerController.Instance.transform.position;

        //Rotate with camera Y Rotation (controlled by mouse, joystick, whatever)
        miniMapCam.transform.eulerAngles = Vector3.right * angle + Vector3.forward * -CameraController.Instance.transform.eulerAngles.y;

        //Zoom out to see the world and player
        miniMapCam.orthographicSize = Mathf.Lerp(miniMapCam.orthographicSize, cameraSize, Time.deltaTime);

        //Make sure the player sprite icon follows the player
        playerIconSpriteInstance.transform.position = PlayerController.Instance.transform.position + PlayerController.Instance.transform.up * (height - 100.0F);

        //Rotate the player icon with the players Y rot, but because its 2D use the Y rot to rotate about the Z Axis (Vector3.forward)
        Quaternion b = Quaternion.Euler(Vector3.right * 90 + -Vector3.forward * PlayerController.Instance.transform.eulerAngles.y);

        //Apply the rotation.
        playerIconSpriteInstance.transform.rotation = Quaternion.Lerp(playerIconSpriteInstance.transform.rotation, b, Time.deltaTime * 4.0F);
    }

    public void OnValidate()
    {
        if (cameraAngle <= 0)
        {
            cameraAngle = 90;
        }

        if (cameraSize <= 0)
        {
            cameraSize = 20;
        }

        if (cameraHeight <= 0)
        {
            cameraHeight = 100;
        }

        if (miniMapCam is not null && miniMapCam.farClipPlane < cameraHeight)
        {
            miniMapCam.farClipPlane = cameraHeight + 1;
        }
    }
}