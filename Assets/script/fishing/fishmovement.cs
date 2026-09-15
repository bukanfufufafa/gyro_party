using UnityEngine;

public class FishRandomMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 5f;

    [Header("Fish Data")]
    public int score = 5;

    [Range(1, 3)]
    public int fishingDifficulty = 1;

    [Header("Fishing Resistance")]
    public float resistanceSpeed = 0.3f;
    public float resistanceInterval = 1f;
    public float sliderDrainSpeed = 10f;

    private Vector3 moveDirection;

    public bool baited = false;
    public Transform mouth;

    private float resistanceTimer;


    void Start()
    {
        moveDirection = Random.insideUnitSphere.normalized;

        moveDirection.y *= 0.5f;
        moveDirection.Normalize();

        SetFishingDifficulty();
    }


    void Update()
    {
        if (!baited)
        {
            // Gerakan normal ikan
            transform.position +=
                moveDirection * moveSpeed * Time.deltaTime;

            // Menghadap arah gerak
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(moveDirection);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }
        else
        {
            // Ikan sedang melawan
            resistanceTimer += Time.deltaTime;

            if (resistanceTimer >= resistanceInterval)
            {
                ResistFishing();
                resistanceTimer = 0f;
            }
        }
    }


    private void ResistFishing()
    {
        // Gerakkan BAIT/FISHING, bukan ikan
        if (transform.parent != null)
        {
            Vector3 direction =
                Random.insideUnitSphere.normalized;

            direction.y *= 0.5f;
            direction.Normalize();

            transform.parent.position +=
                direction * resistanceSpeed;
        }
    }


    private void SetFishingDifficulty()
    {
        switch (fishingDifficulty)
        {
            case 1:
                // Mudah
                resistanceSpeed = 0.2f;
                resistanceInterval = 1.5f;
                sliderDrainSpeed = 7f;
                break;

            case 2:
                // Sedang
                resistanceSpeed = 0.35f;
                resistanceInterval = 1f;
                sliderDrainSpeed = 10f;
                break;

            case 3:
                // Sulit
                resistanceSpeed = 0.5f;
                resistanceInterval = 0.6f;
                sliderDrainSpeed = 14f;
                break;
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (!baited)
        {
            Vector3 normal =
                collision.contacts[0].normal;

            moveDirection =
                Vector3.Reflect(
                    moveDirection,
                    normal
                );

            moveDirection +=
                Random.insideUnitSphere * 0.2f;

            moveDirection.Normalize();
        }
    }
}