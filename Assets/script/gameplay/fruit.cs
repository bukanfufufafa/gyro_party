using UnityEngine;

public class Fruit : MonoBehaviour
{
    private Vector3[] originalPositions;
    private Quaternion[] originalRotations;

    private Rigidbody[] rigidbodies;

    [Header("Cut Effect")]
    public float cutDistance = 0.2f;

    void Awake()
    {
        int childCount = transform.childCount;

        originalPositions = new Vector3[childCount];
        originalRotations = new Quaternion[childCount];

        rigidbodies = GetComponentsInChildren<Rigidbody>(true);

        // Simpan posisi dan rotasi awal setiap child
        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);

            originalPositions[i] = child.localPosition;
            originalRotations[i] = child.localRotation;
        }

        // Semua Rigidbody tetap kinematic
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
        }
    }

    public void ResetFruit()
    {
        // Reset posisi dan rotasi child
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            child.localPosition = originalPositions[i];
            child.localRotation = originalRotations[i];
        }

        // Pastikan semua Rigidbody tetap kinematic
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    public void CutPart(Collider hitCollider)
    {
        // Collider yang terkena pisau
        Transform part = hitCollider.transform;

        // Geser part sedikit ke arah X
        part.localPosition += Vector3.right * cutDistance;
    }
}