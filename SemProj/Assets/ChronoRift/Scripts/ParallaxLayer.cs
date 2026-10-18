using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private float parallaxFactorX = 0.5f;
    [SerializeField] private float parallaxFactorY = 0.2f;

    private Transform cam;
    private Vector3 lastCamPos;

    private void Start()
    {
        if (Camera.main != null)
        {
            cam = Camera.main.transform;
            lastCamPos = cam.position;
        }
    }

    private void LateUpdate()
    {
        if (cam == null)
        {
            if (Camera.main != null)
            {
                cam = Camera.main.transform;
                lastCamPos = cam.position;
            }
            return;
        }

        Vector3 delta = cam.position - lastCamPos;
        transform.position += new Vector3(delta.x * parallaxFactorX, delta.y * parallaxFactorY, 0f);
        lastCamPos = cam.position;
    }
}
