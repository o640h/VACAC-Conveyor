using UnityEngine;

public class ConveyorProduct : MonoBehaviour
{
    [Header("Starting path")]
    [SerializeField] private ConveyorSegment startingSegment;

    [Header("Movement")]
    [SerializeField, Min(0.01f)] private float speed = 0.5f;
    [SerializeField] private float surfaceOffset = 0.01f;

    [Header("Connections")]
    [SerializeField, Min(0.01f)]
    private float connectionSearchDistance = 0.1f;

    private ConveyorSegment currentSegment;
    private Transform[] pathPoints;
    private int targetPointIndex;

    private void Start()
    {
        // A spawner may initialize the product before Start runs.
        if (currentSegment != null)
        {
            return;
        }

        if (!BeginMovingOn(startingSegment))
        {
            enabled = false;
        }
    }

    private void Update()
    {
        Vector3 targetPosition =
            GetPathPosition(targetPointIndex);

        Vector3 movementDirection =
            targetPosition - transform.position;

        if (movementDirection.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(
                movementDirection.normalized,
                Vector3.up);
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime);

        if ((transform.position - targetPosition).sqrMagnitude >
            0.000001f)
        {
            return;
        }

        targetPointIndex++;

        if (targetPointIndex < pathPoints.Length)
        {
            return;
        }

        MoveToNextSegment();
    }

    public bool BeginMovingOn(ConveyorSegment segment)
    {
        if (segment == null)
        {
            Debug.LogError(
                "ConveyorProduct requires a conveyor segment.",
                this);

            return false;
        }

        Transform[] segmentPoints = segment.PathPoints;

        if (segmentPoints == null ||
            segmentPoints.Length < 2)
        {
            Debug.LogError(
                "The conveyor requires at least two path points.",
                segment);

            return false;
        }

        for (int index = 0;
             index < segmentPoints.Length;
             index++)
        {
            if (segmentPoints[index] == null)
            {
                Debug.LogError(
                    "The conveyor has an empty path point.",
                    segment);

                return false;
            }
        }

        if (startingSegment == null)
        {
            startingSegment = segment;
        }

        currentSegment = segment;
        pathPoints = segmentPoints;
        targetPointIndex = 1;
        transform.position = GetPathPosition(0);
        enabled = true;

        return true;
    }

    private void MoveToNextSegment()
    {
        if (TryFindNextSegment(out ConveyorSegment nextSegment))
        {
            BeginMovingOn(nextSegment);
            return;
        }

        Destroy(gameObject);
    }

    private bool TryFindNextSegment(
        out ConveyorSegment nextSegment)
    {
        nextSegment = null;

        if (currentSegment.OutputSnap == null)
        {
            return false;
        }

        ConveyorSegment[] allSegments =
            FindObjectsByType<ConveyorSegment>();

        float closestDistance = connectionSearchDistance;

        foreach (ConveyorSegment candidate in allSegments)
        {
            if (candidate == currentSegment ||
                candidate.InputSnap == null)
            {
                continue;
            }

            float distance = Vector3.Distance(
                currentSegment.OutputSnap.position,
                candidate.InputSnap.position);

            if (distance > closestDistance)
            {
                continue;
            }

            closestDistance = distance;
            nextSegment = candidate;
        }

        return nextSegment != null;
    }

    private Vector3 GetPathPosition(int index)
    {
        return pathPoints[index].position +
               Vector3.up * surfaceOffset;
    }
}