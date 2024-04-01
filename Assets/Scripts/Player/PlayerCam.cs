using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    PlayerInputManager pm;

    public Transform orientation;

    public float sensXMouse;
    public float sensYMouse;

    public float sensXController;
    public float sensYController;

    float xRotation;
    float yRotation;

    // Start is called before the first frame update
    void Start()
    {
        pm = FindObjectOfType<PlayerInputManager>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {

        yRotation += pm.lookInput.x * Time.deltaTime * (pm.usingGamepad ? sensXController : sensXMouse);

        xRotation -= pm.lookInput.y * Time.deltaTime * (pm.usingGamepad ? sensYController : sensXController);

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);

        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
