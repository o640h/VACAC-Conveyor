using UnityEngine;

public class ConveyorProduct : MonoBehaviour
{
    [Header("Starting path")]
    [SerializeField] private ConveyorSegment startingSegment;

    [Header("Movement")]
    [SerializeField, Min(0.01f)] private float speed = 0.5f;
    [SerializeField] private float surfaceOffset = 0.01f;
    [SerializeField] private bool loopOnCurrentSegment = true;

    private Transform[] pathPoints;
    private int targetPointIndex;

    private void Start()
    {
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

        if (loopOnCurrentSegment)
        {
            transform.position = GetPathPosition(0);
            targetPointIndex = 1;
        }
        else
        {
            enabled = false;
        }
    }

    public bool BeginMovingOn(ConveyorSegment segment)
    {
        if (segment == null)
        {
            Debug.LogError(
                "ConveyorProduct requires a starting segment.",
                this);

            return false;
        }

        Transform[] segmentPoints = segment.PathPoints;

        if (segmentPoints == null ||
            segmentPoints.Length < 2)
        {
            Debug.LogError(
                "The starting conveyor requires at least two path points.",
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
                    "The starting conveyor has an empty path point.",
                    segment);

                return false;
            }
        }

        startingSegment = segment;
        pathPoints = segmentPoints;
        targetPointIndex = 1;
        transform.position = GetPathPosition(0);

        return true;
    }

    private Vector3 GetPathPosition(int index)
    {
        return pathPoints[index].position +
               Vector3.up * surfaceOffset;
    }
}