using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{

    [Header("Vector Inputs")]
    public Vector2 moveInput;
    public Vector2 lookInput;

    [Header("Buttons")]
    public bool doJump;
    public bool doCrouch;
    public bool isSprinting;

    [Header("DeviceControl")]
    public bool usingGamepad;

    private void LateUpdate()
    {
        // Set button inputs to false after Update loop

        if (doJump)
            doJump = false;

        if (doCrouch)
            doCrouch = false;
    }


    void OnWalk(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        doJump = true;
    }

    void OnCrouch(InputValue value)
    {
        doCrouch = true;
    }

    void OnSprint(InputValue value)
    {
        isSprinting = value.Get<float>() == 1  ? true : false;
    }

    public void OnControlsChanged(PlayerInput input)
    {
        // Assuming that keyboard/mouse is the default
        if (input.defaultControlScheme == input.currentControlScheme)
            usingGamepad = false;
        else
            usingGamepad = true;
    }
}
