using System.Collections.Generic;
using UnityEngine;

public class ConveyorSegment : MonoBehaviour
{
    [Header("Connection points")]
    [SerializeField] private Transform inputSnap;
    [SerializeField] private Transform outputSnap;

    [Header("Movement path")]
    [SerializeField] private Transform[] pathPoints = new Transform[3];

    public Transform InputSnap => inputSnap;
    public Transform OutputSnap => outputSnap;
    public Transform[] PathPoints => pathPoints;
    public ConveyorSegment PreviousSegment { get; private set; }
    public ConveyorSegment NextSegment { get; private set; }

    public void ConnectNext(ConveyorSegment nextSegment)
    {
        if (nextSegment == null || nextSegment == this)
        {
            return;
        }

        DisconnectNext();
        nextSegment.DisconnectPrevious();

        NextSegment = nextSegment;
        nextSegment.PreviousSegment = this;
    }

    public ConveyorSegment FindFirstSegment()
    {
        ConveyorSegment current = this;
        HashSet<ConveyorSegment> visited = new();

        while (current.PreviousSegment != null &&
               visited.Add(current))
        {
            current = current.PreviousSegment;
        }

        return current;
    }

    public static void RebuildConnectionsFromSnaps(
        float connectionDistance)
    {
        ConveyorSegment[] segments =
            FindObjectsByType<ConveyorSegment>();

        foreach (ConveyorSegment segment in segments)
        {
            segment.DisconnectNext();
            segment.DisconnectPrevious();
        }

        float maximumDistanceSquared =
            connectionDistance * connectionDistance;

        foreach (ConveyorSegment segment in segments)
        {
            if (!segment.IsAvailableForNetwork() ||
                segment.OutputSnap == null)
            {
                continue;
            }

            ConveyorSegment closest = null;
            float closestDistanceSquared =
                maximumDistanceSquared;

            foreach (ConveyorSegment candidate in segments)
            {
                if (candidate == segment ||
                    !candidate.IsAvailableForNetwork() ||
                    candidate.InputSnap == null ||
                    candidate.PreviousSegment != null)
                {
                    continue;
                }

                float distanceSquared =
                    (segment.OutputSnap.position -
                     candidate.InputSnap.position).sqrMagnitude;

                if (distanceSquared > closestDistanceSquared)
                {
                    continue;
                }

                closest = candidate;
                closestDistanceSquared = distanceSquared;
            }

            if (closest != null)
            {
                segment.ConnectNext(closest);
            }
        }
    }

    private bool IsAvailableForNetwork()
    {
        return gameObject.activeInHierarchy &&
               !name.EndsWith("_Preview");
    }

    private void DisconnectNext()
    {
        if (NextSegment == null)
        {
            return;
        }

        ConveyorSegment oldNext = NextSegment;
        NextSegment = null;

        if (oldNext.PreviousSegment == this)
        {
            oldNext.PreviousSegment = null;
        }
    }

    private void DisconnectPrevious()
    {
        if (PreviousSegment == null)
        {
            return;
        }

        ConveyorSegment oldPrevious = PreviousSegment;
        PreviousSegment = null;

        if (oldPrevious.NextSegment == this)
        {
            oldPrevious.NextSegment = null;
        }
    }

    private void OnDestroy()
    {
        DisconnectNext();
        DisconnectPrevious();
    }

    private void Reset()
    {
        inputSnap = transform.Find("InputSnap");
        outputSnap = transform.Find("OutputSnap");

        Transform path = transform.Find("Path");

        if (path != null)
        {
            pathPoints = new Transform[]
            {
                path.Find("P0"),
                path.Find("P1"),
                path.Find("P2")
            };
        }
    }
}
