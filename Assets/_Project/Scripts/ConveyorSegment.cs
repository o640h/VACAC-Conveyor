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