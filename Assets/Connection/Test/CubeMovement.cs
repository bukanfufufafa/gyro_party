


using UnityEngine;

public class CubeMovement : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {

    }

    public void StartTest()
    {
        // Ambil controller.
        Controller controller = ControllerService.Instance.GetController(0);

        // Listen OnSensorChanged.
        controller.OnSensorChanged += HandleOnSensorChanged;
    }

    private void HandleOnSensorChanged(object sender, ControllerSensorData data)
    {
        transform.rotation = data.Rotation;
    }

    public void CalibrateRotation()
    {
        // Ambil controller.
        Controller controller = ControllerService.Instance.GetController(0);

        // Request calibration.
        controller.CallibratePosition();
    }
}