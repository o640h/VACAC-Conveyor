using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ConveyorPlacementManager : MonoBehaviour
{
    [Header("Scene references")]
    [SerializeField] private Camera placementCamera;

    [SerializeField]
    private ConveyorSegment[] conveyorPrefabs =
        new ConveyorSegment[4];

    [Header("Placement settings")]
    [SerializeField] private float rotationStep = 90f;

    [SerializeField, Range(0.5f, 10f)]
    private float connectorRaySnapAngle = 4f;

    [SerializeField, Min(0.001f)]
    private float occupiedConnectorDistance = 0.05f;

    [Header("Placement feedback")]
    [SerializeField]
    private Color freePlacementColor =
        new Color(1f, 0.65f, 0.1f, 1f);

    [SerializeField]
    private Color snappedPlacementColor =
        new Color(0.15f, 1f, 0.25f, 1f);

    [SerializeField]
    private Color blockedPlacementColor =
        new Color(1f, 0.1f, 0.1f, 1f);

    private readonly Plane groundPlane =
        new Plane(Vector3.up, Vector3.zero);

    private ConveyorSegment preview;
    private float previewYaw;
    private ConveyorSegment snappedSegment;
    private SnapMode snappedMode;
    private bool placementBlocked;
    private Renderer[] previewRenderers;
    private MaterialPropertyBlock previewPropertyBlock;

    private enum SnapMode
    {
        None,
        PreviewInputToTargetOutput,
        PreviewOutputToTargetInput
    }

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

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            BeginPlacement(3);
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
            if (EventSystem.current == null ||
                !EventSystem.current.IsPointerOverGameObject())
            {
                ConfirmPlacement();
            }
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
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
        preview.name =
            conveyorPrefabs[prefabIndex].name + "_Preview";

        previewYaw = 0f;
        placementBlocked = false;
        previewRenderers =
            preview.GetComponentsInChildren<Renderer>();
        previewPropertyBlock = new MaterialPropertyBlock();
        SetPreviewColor(freePlacementColor);
    }

    public void SetPlacementEnabled(bool placementEnabled)
    {
        if (!placementEnabled)
        {
            CancelPlacement();
        }

        enabled = placementEnabled;
    }

    private void UpdatePreview()
    {
        Ray mouseRay = placementCamera.ScreenPointToRay(
            Mouse.current.position.ReadValue());

        if (groundPlane.Raycast(
            mouseRay,
            out float groundDistance))
        {
            Vector3 groundPosition =
                mouseRay.GetPoint(groundDistance);

            preview.transform.SetPositionAndRotation(
                groundPosition,
                Quaternion.Euler(0f, previewYaw, 0f));
        }

        // Connector detection remains available when the cursor is aimed
        // directly at an elevated endpoint instead of at the ground.
        SnapPreviewToClosestConnector(mouseRay);
    }

    private void SnapPreviewToClosestConnector(
        Ray mouseRay)
    {
        snappedSegment = null;
        snappedMode = SnapMode.None;
        placementBlocked = false;
        SetPreviewColor(freePlacementColor);

        ConveyorSegment[] segments =
            FindObjectsByType<ConveyorSegment>();

        ConveyorSegment closestSegment = null;
        SnapMode closestMode = SnapMode.None;
        bool closestConnectorOccupied = false;
        float closestAngle = connectorRaySnapAngle;

        foreach (ConveyorSegment segment in segments)
        {
            if (segment == preview)
            {
                continue;
            }

            if (segment.OutputSnap != null)
            {
                ConsiderConnector(
                    segment,
                    segment.OutputSnap,
                    SnapMode.PreviewInputToTargetOutput,
                    IsOutputOccupied(segment, segments),
                    mouseRay,
                    ref closestSegment,
                    ref closestMode,
                    ref closestConnectorOccupied,
                    ref closestAngle);
            }

            if (segment.InputSnap != null)
            {
                ConsiderConnector(
                    segment,
                    segment.InputSnap,
                    SnapMode.PreviewOutputToTargetInput,
                    IsInputOccupied(segment, segments),
                    mouseRay,
                    ref closestSegment,
                    ref closestMode,
                    ref closestConnectorOccupied,
                    ref closestAngle);
            }
        }

        if (closestSegment == null)
        {
            return;
        }

        if (closestConnectorOccupied)
        {
            placementBlocked = true;
            SetPreviewColor(blockedPlacementColor);
            return;
        }

        snappedSegment = closestSegment;
        snappedMode = closestMode;
        SetPreviewColor(snappedPlacementColor);

        if (closestMode ==
            SnapMode.PreviewInputToTargetOutput)
        {
            AlignPreviewInputToOutput(closestSegment);

            Vector3 correction =
                closestSegment.OutputSnap.position -
                preview.InputSnap.position;

            preview.transform.position += correction;
        }
        else if (closestMode ==
                 SnapMode.PreviewOutputToTargetInput)
        {
            AlignPreviewOutputToInput(closestSegment);

            Vector3 correction =
                closestSegment.InputSnap.position -
                preview.OutputSnap.position;

            preview.transform.position += correction;
        }
    }

    private void ConsiderConnector(
        ConveyorSegment segment,
        Transform connector,
        SnapMode mode,
        bool connectorOccupied,
        Ray mouseRay,
        ref ConveyorSegment closestSegment,
        ref SnapMode closestMode,
        ref bool closestConnectorOccupied,
        ref float closestAngle)
    {
        Vector3 directionToConnector =
            connector.position - mouseRay.origin;

        // Ignore connectors behind the camera.
        if (Vector3.Dot(
            mouseRay.direction,
            directionToConnector) <= 0f)
        {
            return;
        }

        float angle = Vector3.Angle(
            mouseRay.direction,
            directionToConnector);

        if (angle >= closestAngle)
        {
            return;
        }

        closestAngle = angle;
        closestSegment = segment;
        closestMode = mode;
        closestConnectorOccupied = connectorOccupied;
    }

    private bool IsOutputOccupied(
        ConveyorSegment target,
        ConveyorSegment[] segments)
    {
        foreach (ConveyorSegment segment in segments)
        {
            if (segment == target ||
                segment == preview ||
                segment.InputSnap == null)
            {
                continue;
            }

            float distance = Vector3.Distance(
                target.OutputSnap.position,
                segment.InputSnap.position);

            if (distance <= occupiedConnectorDistance)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsInputOccupied(
        ConveyorSegment target,
        ConveyorSegment[] segments)
    {
        foreach (ConveyorSegment segment in segments)
        {
            if (segment == target ||
                segment == preview ||
                segment.OutputSnap == null)
            {
                continue;
            }

            float distance = Vector3.Distance(
                target.InputSnap.position,
                segment.OutputSnap.position);

            if (distance <= occupiedConnectorDistance)
            {
                return true;
            }
        }

        return false;
    }

    private void AlignPreviewInputToOutput(
        ConveyorSegment target)
    {
        Transform[] previewPoints = preview.PathPoints;
        Transform[] targetPoints = target.PathPoints;

        if (previewPoints.Length < 2 ||
            targetPoints.Length < 2)
        {
            return;
        }

        Vector3 previewEntryDirection =
            previewPoints[1].position -
            previewPoints[0].position;

        Vector3 targetExitDirection =
            targetPoints[targetPoints.Length - 1].position -
            targetPoints[targetPoints.Length - 2].position;

        AlignHorizontalDirections(
            previewEntryDirection,
            targetExitDirection);
    }

    private void AlignPreviewOutputToInput(
        ConveyorSegment target)
    {
        Transform[] previewPoints = preview.PathPoints;
        Transform[] targetPoints = target.PathPoints;

        if (previewPoints.Length < 2 ||
            targetPoints.Length < 2)
        {
            return;
        }

        Vector3 previewExitDirection =
            previewPoints[previewPoints.Length - 1].position -
            previewPoints[previewPoints.Length - 2].position;

        Vector3 targetEntryDirection =
            targetPoints[1].position -
            targetPoints[0].position;

        AlignHorizontalDirections(
            previewExitDirection,
            targetEntryDirection);
    }

    private void AlignHorizontalDirections(
        Vector3 previewDirection,
        Vector3 targetDirection)
    {
        previewDirection.y = 0f;
        targetDirection.y = 0f;

        if (previewDirection.sqrMagnitude < 0.0001f ||
            targetDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        float rotation = Vector3.SignedAngle(
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
        if (placementBlocked)
        {
            return;
        }

        ClearPreviewColor();

        if (snappedSegment != null)
        {
            if (snappedMode ==
                SnapMode.PreviewInputToTargetOutput)
            {
                snappedSegment.ConnectNext(preview);
            }
            else if (snappedMode ==
                     SnapMode.PreviewOutputToTargetInput)
            {
                preview.ConnectNext(snappedSegment);
            }
        }

        preview.name =
            preview.name.Replace("_Preview", "");

        preview = null;
        snappedSegment = null;
        snappedMode = SnapMode.None;
        placementBlocked = false;
        previewRenderers = null;
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
        snappedSegment = null;
        snappedMode = SnapMode.None;
        placementBlocked = false;
        previewRenderers = null;
    }

    private void SetPreviewColor(Color color)
    {
        if (previewRenderers == null ||
            previewPropertyBlock == null)
        {
            return;
        }

        previewPropertyBlock.Clear();
        previewPropertyBlock.SetColor("_BaseColor", color);
        previewPropertyBlock.SetColor("_Color", color);

        foreach (Renderer previewRenderer in previewRenderers)
        {
            previewRenderer.SetPropertyBlock(
                previewPropertyBlock);
        }
    }

    private void ClearPreviewColor()
    {
        if (previewRenderers == null)
        {
            return;
        }

        foreach (Renderer previewRenderer in previewRenderers)
        {
            previewRenderer.SetPropertyBlock(null);
        }
    }
}
