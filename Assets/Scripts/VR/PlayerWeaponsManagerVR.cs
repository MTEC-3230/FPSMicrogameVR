using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;
using Unity.FPS.Gameplay; 

public class PlayerWeaponsManagerVR : PlayerWeaponsManager
{

    public override bool IsAiming { get; protected set; }
    public override bool IsPointingAtEnemy { get; protected set; }
    public override int ActiveWeaponIndex { get; protected set; }



    void Start()
    {

        m_InputHandler = GetComponent<PlayerInputHandlerVR>();
        DebugUtility.HandleErrorIfNullGetComponent<PlayerInputHandlerVR, PlayerWeaponsManagerVR>(m_InputHandler, this,
            gameObject);

        m_PlayerCharacterController = GetComponent<PlayerCharacterControllerVR>();
        DebugUtility.HandleErrorIfNullGetComponent<PlayerCharacterControllerVR, PlayerWeaponsManagerVR>(
            m_PlayerCharacterController, this, gameObject);


    }



    void Update()
    {
        // shoot handling
        WeaponController activeWeapon = GetActiveWeapon();

        if (activeWeapon != null && activeWeapon.IsReloading)
            return;

        if (activeWeapon != null && m_WeaponSwitchState == WeaponSwitchState.Up)
        {

            // Reload
            if (!activeWeapon.AutomaticReload && m_InputHandler.GetReloadButtonDown() && activeWeapon.CurrentAmmoRatio < 1.0f)
            {
                IsAiming = false;
                activeWeapon.StartReloadAnimation();
                return;
            }
            // handle aiming down sights
            IsAiming = m_InputHandler.GetAimInputHeld();

            // handle shooting
            bool hasFired = activeWeapon.HandleShootInputs(
                m_InputHandler.GetFireInputDown(),
                m_InputHandler.GetFireInputHeld(),
                m_InputHandler.GetFireInputReleased());

            // Handle accumulating recoil
            if (hasFired)
            {
                m_AccumulatedRecoil += Vector3.back * activeWeapon.RecoilForce;
                m_AccumulatedRecoil = Vector3.ClampMagnitude(m_AccumulatedRecoil, MaxRecoilDistance);
            }
        }

        // weapon switch handling
        if (!IsAiming &&
            (activeWeapon == null || !activeWeapon.IsCharging) &&
            (m_WeaponSwitchState == WeaponSwitchState.Up || m_WeaponSwitchState == WeaponSwitchState.Down))
        {
            int switchWeaponInput = m_InputHandler.GetSwitchWeaponInput();
            if (switchWeaponInput != 0)
            {
                bool switchUp = switchWeaponInput > 0;
                SwitchWeapon(switchUp);
            }
            else
            {
                switchWeaponInput = m_InputHandler.GetSelectWeaponInput();
                if (switchWeaponInput != 0)
                {
                    if (GetWeaponAtSlotIndex(switchWeaponInput - 1) != null)
                        SwitchToWeaponIndex(switchWeaponInput - 1);
                }
            }
        }

        // Pointing at enemy handling
        IsPointingAtEnemy = false;
        if (activeWeapon)
        {
            if (Physics.Raycast(WeaponCamera.transform.position, WeaponCamera.transform.forward, out RaycastHit hit,
                1000, -1, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.GetComponentInParent<Health>() != null)
                {
                    IsPointingAtEnemy = true;
                }
            }
        }
    }



    // Update various animated features in LateUpdate because it needs to override the animated arm position
    void LateUpdate()
    {
        //UpdateWeaponAiming();
        //UpdateWeaponBob();
        //UpdateWeaponRecoil();
        //UpdateWeaponSwitching();

        //// Set final weapon socket position based on all the combined animation influences
        //WeaponParentSocket.localPosition =
        //    m_WeaponMainLocalPosition + m_WeaponBobLocalPosition + m_WeaponRecoilLocalPosition;
    }




    // Sets the FOV of the main camera and the weapon camera simultaneously
    public override void SetFov(float fov)
    {

    }


    // Iterate on all weapon slots to find the next valid weapon to switch to
    public override void SwitchWeapon(bool ascendingOrder)
    {
        int newWeaponIndex = -1;


        Debug.Log("PlayerWeaponsManagerVR : SwitchWeapon : ");

        // Handle switching to the new weapon index
        SwitchToWeaponIndex(newWeaponIndex);
    }

    // Switches to the given weapon index in weapon slots if the new index is a valid weapon that is different from our current one
    public override void SwitchToWeaponIndex(int newWeaponIndex, bool force = false)
    {
        if (force || (newWeaponIndex != ActiveWeaponIndex && newWeaponIndex >= 0))
        {
            // Store data related to weapon switching animation
            m_WeaponSwitchNewWeaponIndex = newWeaponIndex;
            m_TimeStartedWeaponSwitch = Time.time;

            // Handle case of switching to a valid weapon for the first time (simply put it up without putting anything down first)
            if (GetActiveWeapon() == null)
            {
                ActiveWeaponIndex = m_WeaponSwitchNewWeaponIndex;

                WeaponController newWeapon = GetWeaponAtSlotIndex(m_WeaponSwitchNewWeaponIndex);
                if (OnSwitchedToWeapon != null)
                {
                    OnSwitchedToWeapon.Invoke(newWeapon);
                }
            }

        }
    }

    public override WeaponController HasWeapon(WeaponController weaponPrefab)
    {
        // Checks if we already have a weapon coming from the specified prefab
        for (var index = 0; index < m_WeaponSlots.Length; index++)
        {
            var w = m_WeaponSlots[index];
            if (w != null && w.SourcePrefab == weaponPrefab.gameObject)
            {
                return w;
            }
        }

        return null;
    }

    // Updates weapon position and camera FoV for the aiming transition
    protected override void UpdateWeaponAiming()
    {

    }

    // Updates the weapon bob animation based on character speed
    protected override void UpdateWeaponBob()
    {

    }

    // Updates the weapon recoil animation
    protected override void UpdateWeaponRecoil()
    {

    }

    // Updates the animated transition of switching weapons
    protected override void UpdateWeaponSwitching()
    {

        ActiveWeaponIndex = m_WeaponSwitchNewWeaponIndex;


        // Activate new weapon
        WeaponController newWeapon = GetWeaponAtSlotIndex(ActiveWeaponIndex);
        if (OnSwitchedToWeapon != null)
        {
            OnSwitchedToWeapon.Invoke(newWeapon);
        }

    }

    // Adds a weapon to our inventory
    public override bool AddWeapon(WeaponController weaponPrefab)
    {
        // if we already hold this weapon type (a weapon coming from the same source prefab), don't add the weapon
        if (HasWeapon(weaponPrefab) != null)
        {
            return false;
        }

        // search our weapon slots for the first free one, assign the weapon to it, and return true if we found one. Return false otherwise
        for (int i = 0; i < m_WeaponSlots.Length; i++)
        {
            // only add the weapon if the slot is free
            if (m_WeaponSlots[i] == null)
            {
                // spawn the weapon prefab as child of the weapon socket
                WeaponController weaponInstance = Instantiate(weaponPrefab, WeaponParentSocket);
                weaponInstance.transform.localPosition = Vector3.zero;
                weaponInstance.transform.localRotation = Quaternion.identity;

                // Set owner to this gameObject so the weapon can alter projectile/damage logic accordingly
                weaponInstance.Owner = gameObject;
                weaponInstance.SourcePrefab = weaponPrefab.gameObject;
                weaponInstance.ShowWeapon(false);

                // Assign the first person layer to the weapon
                int layerIndex =
                    Mathf.RoundToInt(Mathf.Log(FpsWeaponLayer.value,
                        2)); // This function converts a layermask to a layer index
                foreach (Transform t in weaponInstance.gameObject.GetComponentsInChildren<Transform>(true))
                {
                    t.gameObject.layer = layerIndex;
                }

                m_WeaponSlots[i] = weaponInstance;

                if (OnAddedWeapon != null)
                {
                    OnAddedWeapon.Invoke(weaponInstance, i);
                }

                return true;
            }
        }

        // Handle auto-switching to weapon if no weapons currently
        if (GetActiveWeapon() == null)
        {
            SwitchWeapon(true);
        }

        return false;
    }

    public override bool RemoveWeapon(WeaponController weaponInstance)
    {
        // Look through our slots for that weapon
        for (int i = 0; i < m_WeaponSlots.Length; i++)
        {
            // when weapon found, remove it
            if (m_WeaponSlots[i] == weaponInstance)
            {
                m_WeaponSlots[i] = null;

                if (OnRemovedWeapon != null)
                {
                    OnRemovedWeapon.Invoke(weaponInstance, i);
                }

                Destroy(weaponInstance.gameObject);

                // Handle case of removing active weapon (switch to next weapon)
                if (i == ActiveWeaponIndex)
                {
                    SwitchWeapon(true);
                }

                return true;
            }
        }

        return false;
    }

    public override WeaponController GetActiveWeapon()
    {
        return GetWeaponAtSlotIndex(ActiveWeaponIndex);
    }

    public override WeaponController GetWeaponAtSlotIndex(int index)
    {
        // find the active weapon in our weapon slots based on our active weapon index
        if (index >= 0 &&
            index < m_WeaponSlots.Length)
        {
            return m_WeaponSlots[index];
        }

        // if we didn't find a valid active weapon in our weapon slots, return null
        return null;
    }




    protected override void OnWeaponSwitched(WeaponController newWeapon)
    {
        if (newWeapon != null)
        {
            newWeapon.ShowWeapon(true);
        }
    }

}
