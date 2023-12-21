using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager instance;

    [SerializeField] HUDMiniMap hudMiniMap;

    [SerializeField] HUDNotifications hudNotifications;

    [SerializeField] HUDWeaponSystem hudWeaponSystem;

    private PlayerInput input;

    public HUDMiniMap GetMiniMap() => hudMiniMap;

    public HUDNotifications GetNotifications() => hudNotifications;

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
        hudWeaponSystem.OnValidate();
        hudNotifications.OnValidate();
    }

    private void Update()
    {
        //Make sure we have access to player input.
        if (input is null)
        {
            input = FindObjectOfType<PlayerInput>();
        }

        hudMiniMap.Update();

        hudNotifications.Update();

        hudWeaponSystem.Update();
    }

    private IEnumerator SetCanvasGroupVisible(CanvasGroup canvasGroup, bool isVisible, float fadeDuration)
    {
        float velocity = 0;

        if (isVisible)
        {
            while (canvasGroup.alpha < 1)
            {
                canvasGroup.alpha = Mathf.SmoothDamp(canvasGroup.alpha, 1, ref velocity, fadeDuration);

                yield return new WaitForEndOfFrame();
            }

            yield break;
        }
        else
        {
            while (canvasGroup.alpha > 0)
            {
                canvasGroup.alpha = Mathf.SmoothDamp(canvasGroup.alpha, 0, ref velocity, fadeDuration);

                yield return new WaitForEndOfFrame();
            }

            yield break;
        }
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
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField][Range(0, 2)] float fadeDuration;

    GameObject playerIconSpriteInstance;

    public bool IsVisible;
    public CanvasGroup GetCanvasGroup() => canvasGroup;

    public float GetFadeDuration() => fadeDuration;

    public void Start()
    {
        if (playerIconSpriteInstance is null)
        {
            playerIconSpriteInstance = Object.Instantiate(playerIconPrefab);
        }
    }

    public void Update()
    {
        miniMapCam.gameObject.SetActive(IsVisible);
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

[System.Serializable]
public class HUDNotifications
{
    [SerializeField]TMP_Text txtNotificationContent;
    [SerializeField]CanvasGroup canvasGroup;
    [SerializeField][Range(0, 2)] float fadeDuration;
    [SerializeField][Range(1, 10)] float notificationDuration;

    public bool IsVisible { get; set; }

    public CanvasGroup GetCanvasGroup => canvasGroup;
    public float GetFadeDuration =>  fadeDuration;

    public void ShowNotification(string message, float specifiedDuration = 4)
    {
        IsVisible = true;

        txtNotificationContent.text = message;

        notificationDuration = specifiedDuration;
    }

    float notificationTimer;

    public void Update()
    {
        if (IsVisible)
        {
            canvasGroup.alpha = 1;

            notificationTimer += Time.deltaTime;

            if (notificationTimer >= notificationDuration)
            {
                notificationTimer = 0;

                canvasGroup.alpha = 0;

                IsVisible = false;
            }
        }
    }

    public void OnValidate()
    {
        if (txtNotificationContent is null)
            Debug.LogError("Notification content text is not assigned!");

        if (canvasGroup is null)
            Debug.LogError("Notification UI Canvas Group is not assigned");
    }
}

[System.Serializable]
public class HUDWeaponSystem
{
    [System.Serializable]
    public class Crosshair
    {
        public GameObject root;
        public GameObject hitIndicator;
        public float hitIndicatorDelay = 0.4F;
        public bool hasHitSomething;
        public bool IsActive;

        private float hitIndTimer = 0;

        public void OnValidate()
        {
            if (root is null)
                Debug.LogError("Crosshair root is not assigned");

            if (hitIndicator is null)
                Debug.LogError("Crosshair hit indicator is not assigned");
        }

        public void Update()
        {
            IsActive = PlayerController.Instance.IsAiming;

            root.SetActive(IsActive);

            if (hasHitSomething)
            {
                hitIndTimer += Time.deltaTime;

                hitIndicator.SetActive(true);

                if (hitIndTimer >= hitIndicatorDelay)
                {
                    hitIndicator.SetActive(false);

                    hitIndTimer = 0;
                }
            }
        }
    }

    public Crosshair crossHair;

    public void Update()
    {
        if (PlayerController.Instance != null && PlayerController.Instance.WeaponInventory.HasWeaponEquipped)
        {
            crossHair.Update();
        }
    }

    public void OnValidate()
    {
        crossHair.OnValidate();
    }
}