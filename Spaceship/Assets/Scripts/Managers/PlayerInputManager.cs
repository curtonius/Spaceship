using System;
using UnityEngine;

public class PlayerInputManager : MonoSingleton<PlayerInputManager>
{
    float lastHorizontal;
    float lastVertical;
    float lastFiring;
    bool lastImpact;
    bool lastRepair;
    bool lastDodge;
    Vector3 lastMousePosition;

    public override void OnInitialize()
    {
        base.OnInitialize();
        this.StayAlive = true;
    }

    void Update()
    {
        float currentCheck = Input.GetAxisRaw("Horizontal");
        if (currentCheck != lastHorizontal)
        {
            EventManager.Instance.Raise<float>("UpdateHorizontal", currentCheck);
            lastHorizontal = currentCheck;
        }

        currentCheck = Input.GetAxisRaw("Vertical");
        if (currentCheck != lastVertical)
        {
            EventManager.Instance.Raise<float>("UpdateVertical", currentCheck);
            lastVertical = currentCheck;
        }


        currentCheck = Input.GetAxisRaw("Jump");
        if (currentCheck != lastFiring)
        {
            EventManager.Instance.Raise<float>("UpdateFiring", currentCheck);
            lastFiring = currentCheck;
        }

        bool check = Input.GetKey(KeyCode.Tab);
        if (check != lastImpact)
        {
            EventManager.Instance.Raise<bool>("UpdateImpact", check);
            lastImpact = check;
        }

        check = Input.GetKey(KeyCode.LeftControl);
        if (check != lastRepair)
        {
            EventManager.Instance.Raise<bool>("UpdateRepair", check);
            lastRepair = check;
        }

        check = Input.GetKey(KeyCode.LeftShift);
        if (check != lastDodge)
        {
            EventManager.Instance.Raise<bool>("UpdateDodge", check);
            lastDodge = check;
        }

        Vector3 currentMousePosition = Input.mousePosition;
        if (currentMousePosition != lastMousePosition)
        {
            EventManager.Instance.Raise<Vector3>("MovedMouse", currentMousePosition);
            lastMousePosition = currentMousePosition;
        }
    }
}
