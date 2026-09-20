using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class RuntimeCameraController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0.1f)] private float moveSpeed = 4f;
    [SerializeField, Min(1f)] private float fastMultiplier = 3f;

    [Header("Mouse look")]
    [SerializeField, Min(0.01f)] private float lookSensitivity = 0.12f;
    [SerializeField, Range(1f, 89f)] private float maximumPitch = 85f;

    private float yaw;
    private float pitch;

    private void Awake()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = NormalizeAngle(angles.x);
    }

    private void Update()
    {
        if (Keyboard.current == null || Mouse.current == null)
        {
            return;
        }

        UpdateMouseLook();
        UpdateMovement();
    }

    private void UpdateMouseLook()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (!Mouse.current.rightButton.isPressed)
        {
            return;
        }

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        yaw += mouseDelta.x * lookSensitivity;
        pitch -= mouseDelta.y * lookSensitivity;
        pitch = Mathf.Clamp(pitch, -maximumPitch, maximumPitch);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void UpdateMovement()
    {
        Vector3 flatForward = transform.forward;
        flatForward.y = 0f;
        flatForward.Normalize();

        Vector3 flatRight = transform.right;
        flatRight.y = 0f;
        flatRight.Normalize();

        Vector3 movement = Vector3.zero;

        if (Keyboard.current.wKey.isPressed)
        {
            movement += flatForward;
        }

        if (Keyboard.current.sKey.isPressed)
        {
            movement -= flatForward;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            movement += flatRight;
        }

        if (Keyboard.current.aKey.isPressed)
        {
            movement -= flatRight;
        }

        if (Keyboard.current.eKey.isPressed)
        {
            movement += Vector3.up;
        }

        if (Keyboard.current.qKey.isPressed)
        {
            movement -= Vector3.up;
        }

        if (movement.sqrMagnitude < 0.0001f)
        {
            return;
        }

        float currentSpeed = moveSpeed;

        if (Keyboard.current.leftShiftKey.isPressed ||
            Keyboard.current.rightShiftKey.isPressed)
        {
            currentSpeed *= fastMultiplier;
        }

        transform.position +=
            movement.normalized *
            currentSpeed *
            Time.unscaledDeltaTime;
    }

    private static float NormalizeAngle(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
