using UnityEngine;
using UnityEngine.InputSystem;

public class ConveyorPlacementManager : MonoBehaviour
{
    [Header("Scene references")]
    [SerializeField] private Camera placementCamera;
    [SerializeField]
    private ConveyorSegment[] conveyorPrefabs =
        new ConveyorSegment[3];

    [Header("Placement settings")]
    [SerializeField] private float snapDistance = 0.5f;
    [SerializeField] private float rotationStep = 90f;

    private readonly Plane groundPlane =
        new Plane(Vector3.up, Vector3.zero);

    private ConveyorSegment preview;
    private float previewYaw;

    private void Awake()
    {
        if (placementCamera == null)
        {
            placementCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (Keyboard.current == null ||
            Mouse.current == null ||
            placementCamera == null)
        {
            return;
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            BeginPlacement(0);
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            BeginPlacement(1);
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            BeginPlacement(2);
        }

        if (preview == null)
        {
            return;
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            previewYaw = Mathf.Repeat(
                previewYaw + rotationStep,
                360f);
        }

        UpdatePreview();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            ConfirmPlacement();
        }

        if (Mouse.current.rightButton.wasPressedThisFrame ||
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CancelPlacement();
        }
    }

    private void BeginPlacement(int prefabIndex)
    {
        if (prefabIndex < 0 ||
            prefabIndex >= conveyorPrefabs.Length ||
            conveyorPrefabs[prefabIndex] == null)
        {
            return;
        }

        CancelPlacement();

        preview = Instantiate(conveyorPrefabs[prefabIndex]);
        preview.name = conveyorPrefabs[prefabIndex].name + "_Preview";
        previewYaw = 0f;
    }

    private void UpdatePreview()
    {
        Ray ray = placementCamera.ScreenPointToRay(
            Mouse.current.position.ReadValue());

        if (!groundPlane.Raycast(ray, out float distance))
        {
            return;
        }

        Vector3 groundPosition = ray.GetPoint(distance);

        preview.transform.SetPositionAndRotation(
            groundPosition,
            Quaternion.Euler(0f, previewYaw, 0f));

        SnapPreviewToClosestOutput();
    }

    private void SnapPreviewToClosestOutput()
    {
        ConveyorSegment[] segments =
            FindObjectsByType<ConveyorSegment>();

        ConveyorSegment closestSegment = null;
        float closestDistance = snapDistance;

        foreach (ConveyorSegment segment in segments)
        {
            if (segment == preview)
            {
                continue;
            }

            Vector3 previewPosition = preview.InputSnap.position;
            Vector3 outputPosition = segment.OutputSnap.position;

            // Compare horizontally so elevated outputs also work.
            previewPosition.y = 0f;
            outputPosition.y = 0f;

            float distance = Vector3.Distance(
                previewPosition,
                outputPosition);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSegment = segment;
            }
        }

        if (closestSegment == null)
        {
            return;
        }

        AlignPreviewDirection(closestSegment);

        Vector3 correction =
            closestSegment.OutputSnap.position -
            preview.InputSnap.position;

        preview.transform.position += correction;
    }

    private void AlignPreviewDirection(ConveyorSegment target)
    {
        Transform[] previewPoints = preview.PathPoints;
        Transform[] targetPoints = target.PathPoints;

        if (previewPoints.Length < 2 ||
            targetPoints.Length < 2)
        {
            return;
        }

        Vector3 previewDirection =
            previewPoints[1].position -
            previewPoints[0].position;

        Vector3 targetDirection =
            targetPoints[targetPoints.Length - 1].position -
            targetPoints[targetPoints.Length - 2].position;

        previewDirection.y = 0f;
        targetDirection.y = 0f;

        if (previewDirection.sqrMagnitude < 0.0001f ||
            targetDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        float rotation =
            Vector3.SignedAngle(
                previewDirection,
                targetDirection,
                Vector3.up);

        preview.transform.Rotate(
            Vector3.up,
            rotation,
            Space.World);
    }

    private void ConfirmPlacement()
    {
        preview.name = preview.name.Replace("_Preview", "");
        preview = null;
    }

    private void CancelPlacement()
    {
        if (preview == null)
        {
            return;
        }

        preview.gameObject.SetActive(false);
        Destroy(preview.gameObject);
        preview = null;
    }
}