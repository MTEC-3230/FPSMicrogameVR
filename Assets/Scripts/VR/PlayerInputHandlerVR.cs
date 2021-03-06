using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;


public class PlayerInputHandlerVR : PlayerInputHandler
{




    void Start()
    {
        m_PlayerCharacterController = GetComponent<PlayerCharacterControllerVR>();
        DebugUtility.HandleErrorIfNullGetComponent<PlayerCharacterControllerVR, PlayerInputHandlerVR>(
            m_PlayerCharacterController, this, gameObject);
        m_GameFlowManager = FindObjectOfType<GameFlowManager>();
        DebugUtility.HandleErrorIfNullFindObject<GameFlowManager, PlayerInputHandlerVR>(m_GameFlowManager, this);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }



    void LateUpdate()
    {
        //m_FireInputWasHeld = GetFireInputHeld();
    }

    public override bool CanProcessInput()
    {
        return Cursor.lockState == CursorLockMode.Locked && !m_GameFlowManager.GameIsEnding;
    }

    public override Vector3 GetMoveInput()
    {
        return Vector3.zero;
    }

    public override float GetLookInputsHorizontal()
    {
        return 0f; 
    }

    public override float GetLookInputsVertical()
    {
        return 0f;
    }

    public override bool GetJumpInputDown()
    {

        return false;
    }

    public override bool GetJumpInputHeld()
    {
        return false;
    }

    public override bool GetFireInputDown()
    {
        Debug.Log("GetFireInputDown");
        return GetFireInputHeld() && !m_FireInputWasHeld;
    }

    public override bool GetFireInputReleased()
    {
        return !GetFireInputHeld() && m_FireInputWasHeld;
    }

    public override bool GetFireInputHeld()
    {
        if (CanProcessInput())
        {
            //bool isGamepad = Input.GetAxis(GameConstants.k_ButtonNameGamepadFire) != 0f;
            //if (isGamepad)
            //{
            //    return Input.GetAxis(GameConstants.k_ButtonNameGamepadFire) >= TriggerAxisThreshold;
            //}
            //else
            //{
            //    return Input.GetButton(GameConstants.k_ButtonNameFire);
            //}

            // For Testing

            //if(Input.GetKeyDown(KeyCode.Space))
            //{
            //    Debug.Log("GetFireInputHeld!");
            //    return true; 
            //}
        }

        return false;
    }

    public override bool GetAimInputHeld()
    {
        //if (CanProcessInput())
        //{
        //    bool isGamepad = Input.GetAxis(GameConstants.k_ButtonNameGamepadAim) != 0f;
        //    bool i = isGamepad
        //        ? (Input.GetAxis(GameConstants.k_ButtonNameGamepadAim) > 0f)
        //        : Input.GetButton(GameConstants.k_ButtonNameAim);
        //    return i;
        //}

        return false;
    }

    public override bool GetSprintInputHeld()
    {
        //if (CanProcessInput())
        //{
        //    return Input.GetButton(GameConstants.k_ButtonNameSprint);
        //}

        return false;
    }

    public override bool GetCrouchInputDown()
    {
        //if (CanProcessInput())
        //{
        //    return Input.GetButtonDown(GameConstants.k_ButtonNameCrouch);
        //}

        return false;
    }

    public override bool GetCrouchInputReleased()
    {
        //if (CanProcessInput())
        //{
        //    return Input.GetButtonUp(GameConstants.k_ButtonNameCrouch);
        //}

        return false;
    }

    public override bool GetReloadButtonDown()
    {
        //if (CanProcessInput())
        //{
        //    return Input.GetButtonDown(GameConstants.k_ButtonReload);
        //}

        return false;
    }

    public override int GetSwitchWeaponInput()
    {
        //if (CanProcessInput())
        //{

        //    bool isGamepad = Input.GetAxis(GameConstants.k_ButtonNameGamepadSwitchWeapon) != 0f;
        //    string axisName = isGamepad
        //        ? GameConstants.k_ButtonNameGamepadSwitchWeapon
        //        : GameConstants.k_ButtonNameSwitchWeapon;

        //    if (Input.GetAxis(axisName) > 0f)
        //        return -1;
        //    else if (Input.GetAxis(axisName) < 0f)
        //        return 1;
        //    else if (Input.GetAxis(GameConstants.k_ButtonNameNextWeapon) > 0f)
        //        return -1;
        //    else if (Input.GetAxis(GameConstants.k_ButtonNameNextWeapon) < 0f)
        //        return 1;
        //}

        return 0;
    }

    public override int GetSelectWeaponInput()
    {
        if (CanProcessInput())
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                return 1;
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                return 2;
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                return 3;
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                return 4;
            else if (Input.GetKeyDown(KeyCode.Alpha5))
                return 5;
            else if (Input.GetKeyDown(KeyCode.Alpha6))
                return 6;
            else if (Input.GetKeyDown(KeyCode.Alpha7))
                return 7;
            else if (Input.GetKeyDown(KeyCode.Alpha8))
                return 8;
            else if (Input.GetKeyDown(KeyCode.Alpha9))
                return 9;
            else
                return 0;
        }

        return 0;
    }

    float GetMouseOrStickLookAxis(string mouseInputName, string stickInputName)
    {
        if (CanProcessInput())
        {
            // Check if this look input is coming from the mouse
            bool isGamepad = Input.GetAxis(stickInputName) != 0f;
            float i = isGamepad ? Input.GetAxis(stickInputName) : Input.GetAxisRaw(mouseInputName);

            // handle inverting vertical input
            if (InvertYAxis)
                i *= -1f;

            // apply sensitivity multiplier
            i *= LookSensitivity;

            if (isGamepad)
            {
                // since mouse input is already deltaTime-dependant, only scale input with frame time if it's coming from sticks
                i *= Time.deltaTime;
            }
            else
            {
                // reduce mouse input amount to be equivalent to stick movement
                i *= 0.01f;
#if UNITY_WEBGL
                    // Mouse tends to be even more sensitive in WebGL due to mouse acceleration, so reduce it even more
                    i *= WebglLookSensitivityMultiplier;
#endif
            }

            return i;
        }

        return 0f;
    }
}
