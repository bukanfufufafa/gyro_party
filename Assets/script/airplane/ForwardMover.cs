using UnityEngine;

public class ForwardMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Kecepatan laju pesawat ke depan.")]
    [SerializeField] private float forwardSpeed = 20f;

    // UBAH: Default menjadi false agar tidak langsung jalan
    private bool isMoving = false; 

    void Update()
    {
        if (!isMoving) return;
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime, Space.World);
    }

    // Fungsi baru untuk memulai laju pesawat
    public void StartMovement()
    {
        isMoving = true;
    }

    public void StopMovement()
    {
        isMoving = false;
    }
}