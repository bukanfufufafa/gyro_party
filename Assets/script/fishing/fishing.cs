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

    [Tooltip("Centang untuk Player 1 (Q), matikan untuk Player 2 (M)")]
    public bool player1 = true;

    private Vector3 startPosition;
    private Vector3 fishingStartPosition;

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
        // ==============================
        // ROD KEMBALI
        // ==============================

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


        // Pastikan ikan masih ada
        if (caughtFish == null)
        {
            EndFishing();
            return;
        }


        // ==============================
        // SLIDER TURUN
        // ==============================

        fishingSlider.value -=
            caughtFish.sliderDrainSpeed * Time.deltaTime;


        // ==============================
        // STRIKE
        // ==============================

        bool strike = false;

        if (player1)
        {
            // P1 = Q
            strike = Input.GetKeyDown(KeyCode.Q);
        }
        else
        {
            // P2 = M
            strike = Input.GetKeyDown(KeyCode.M);
        }


        if (strike)
        {
            fishingSlider.value += tapPower;

            PullRod();
        }


        // ==============================
        // IKAN LEPAS
        // ==============================

        if (fishingSlider.value <= 0)
        {
            FishEscaped();
            return;
        }


        // ==============================
        // IKAN BERHASIL
        // ==============================

        if (fishingSlider.value >= 100)
        {
            CatchFish();
            return;
        }


        // ==============================
        // PLAYER KEMBALI KE POSISI AWAL
        // ==============================

        transform.position = Vector3.MoveTowards(
            transform.position,
            fishingStartPosition,
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

        CancelInvoke(nameof(ReleaseRod));

        Invoke(
            nameof(ReleaseRod),
            0.15f
        );
    }


    private void ReleaseRod()
    {
        rodPulling = false;
    }


    private void OnCollisionEnter(Collision collision)
    {
        // Sudah sedang fishing
        if (fishingActive)
            return;


        // Cari ikan
        if (!collision.gameObject.TryGetComponent(
            out FishRandomMovement fish))
            return;


        // ==========================================
        // IKAN SUDAH DIMILIKI PLAYER LAIN
        // ==========================================

        if (fish.baited)
            return;


        // ==========================================
        // SIMPAN POSISI SEBELUM FISHING
        // ==========================================

        fishingStartPosition = transform.position;


        // ==========================================
        // AMBIL IKAN
        // ==========================================

        caughtFish = fish;

        fish.baited = true;

        fish.currentFisher = this;

        fish.transform.SetParent(transform);


        // Mulai fishing
        StartFishing();
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


        // ==========================================
        // LEPASKAN IKAN
        // ==========================================

        if (caughtFish != null)
        {
            caughtFish.baited = false;

            caughtFish.currentFisher = null;

            caughtFish.transform.SetParent(null);

            caughtFish = null;
        }


        // ==========================================
        // KEMBALIKAN PLAYER KE POSISI AWAL
        // ==========================================

        transform.position = fishingStartPosition;


        EndFishing();
    }


    private void CatchFish()
    {
        fishingActive = false;


        // ==========================================
        // DAPAT IKAN
        // ==========================================

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


        // ==========================================
        // KEMBALIKAN PLAYER
        // ==========================================

        transform.position = fishingStartPosition;


        EndFishing();
    }


    private void EndFishing()
    {
        fishingActive = false;


        // Sembunyikan slider
        if (fishingSlider != null)
        {
            fishingSlider.gameObject.SetActive(false);
        }


        // Player bisa bergerak lagi
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }


        // Rod berhenti ditarik
        rodPulling = false;


        // Batalkan Invoke rod
        CancelInvoke(nameof(ReleaseRod));
    }
}