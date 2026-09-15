using UnityEngine;

public class Rope : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;

    private LineRenderer line;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 2;
    }

    void Update()
    {
        line.SetPosition(0, pointA.position);
        line.SetPosition(1, pointB.position);
    }
}