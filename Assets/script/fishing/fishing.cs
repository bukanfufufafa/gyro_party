using UnityEngine;
using UnityEngine.UI;

public class fishing : MonoBehaviour
{
    [Header("Bait")]
    public Transform bait;

    [Header("Fishing Minigame")]
    public Slider fishingSlider;

    public float startValue = 30f;
    public float tapPower = 10f;

    [Header("Rod")]
    public Transform rod;
    public float pullAngle = 40f;
    public float rodReturnSpeed = 8f;

    [Header("Return")]
    public float returnSpeed = 3f;

    [Header("Player")]
    public PlayerMovement playerMovement;

    private Vector3 startPosition;
    private FishRandomMovement caughtFish;

    private bool fishingActive = false;

    private Quaternion rodStartRotation;
    private bool rodPulling = false;

    public int score;


    private void Start()
    {
        startPosition = transform.position;

        if (rod != null)
        {
            rodStartRotation = rod.localRotation;
        }

        if (fishingSlider != null)
        {
            fishingSlider.minValue = 0;
            fishingSlider.maxValue = 100;
            fishingSlider.value = 0;
            fishingSlider.gameObject.SetActive(false);
        }
    }


    private void Update()
    {
        // Rod kembali ke posisi awal
        if (rod != null && !rodPulling)
        {
            rod.localRotation = Quaternion.Slerp(
                rod.localRotation,
                rodStartRotation,
                rodReturnSpeed * Time.deltaTime
            );
        }


        if (!fishingActive)
            return;


        // ====================================
        // SLIDER TURUN
        // ====================================

        fishingSlider.value -=
            caughtFish.sliderDrainSpeed * Time.deltaTime;


        // ====================================
        // TAP
        // ====================================

        if (Input.GetKeyDown(KeyCode.Space))
        {
            fishingSlider.value += tapPower;

            PullRod();
        }


        // ====================================
        // IKAN LEPAS
        // ====================================

        if (fishingSlider.value <= 0)
        {
            FishEscaped();
            return;
        }


        // ====================================
        // IKAN BERHASIL
        // ====================================

        if (fishingSlider.value >= 100)
        {
            CatchFish();
            return;
        }


        // ====================================
        // FISHING KEMBALI KE POSISI AWAL
        // ====================================

        transform.position = Vector3.MoveTowards(
            transform.position,
            startPosition,
            returnSpeed * Time.deltaTime
        );
    }


    private void PullRod()
    {
        if (rod == null)
            return;

        rodPulling = true;

        rod.localRotation =
            rodStartRotation *
            Quaternion.Euler(pullAngle, 0f, 0f);

        Invoke(nameof(ReleaseRod), 0.15f);
    }


    private void ReleaseRod()
    {
        rodPulling = false;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (fishingActive)
            return;

        if (collision.gameObject.TryGetComponent(
            out FishRandomMovement fish))
        {
            caughtFish = fish;

            fish.baited = true;

            fish.transform.SetParent(transform);

            StartFishing();
        }
    }


    private void StartFishing()
    {
        fishingActive = true;

        fishingSlider.value = startValue;
        fishingSlider.gameObject.SetActive(true);

        // Player tidak bisa bergerak
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
    }


    private void FishEscaped()
    {
        fishingActive = false;

        if (caughtFish != null)
        {
            caughtFish.baited = false;
            caughtFish.transform.SetParent(null);
            caughtFish = null;
        }

        EndFishing();
    }


    private void CatchFish()
    {
        fishingActive = false;

        if (caughtFish != null)
        {
            Debug.Log(
                "Ikan didapat! Score: "
                + caughtFish.score
            );
            score += caughtFish.score;
            Destroy(caughtFish.gameObject);
            caughtFish = null;
        }

        EndFishing();
    }


    private void EndFishing()
    {
        fishingSlider.gameObject.SetActive(false);

        // Player bisa bergerak lagi
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        rodPulling = false;
    }
}