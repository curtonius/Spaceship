using System;
using UnityEngine;

public class PlayerInputManager : MonoSingleton<PlayerInputManager>
{
    float lastHorizontal;
    float lastVertical;
    float lastFiring;
    float lastImpact;
    float lastRepair;
    float lastDodge;
    Vector3 lastMousePosition;

    public override void OnInitialize()
    {
        base.OnInitialize();
        this.StayAlive = true;
    }

    void Update()
    {
        float currentCheck = Input.GetAxis("Horizontal");
        if (currentCheck != lastHorizontal)
        {
            EventManager.Instance.Raise<float>("UpdateHorizontal", currentCheck);
            lastHorizontal = currentCheck;
        }

        currentCheck = Input.GetAxis("Vertical");
        if (currentCheck != lastVertical)
        {
            EventManager.Instance.Raise<float>("UpdateVertical", currentCheck);
            lastVertical = currentCheck;
        }


        currentCheck = Input.GetAxisRaw("Fire");
        if (currentCheck != lastFiring)
        {
            EventManager.Instance.Raise<float>("UpdateFiring", currentCheck);
            lastFiring = currentCheck;
        }

        currentCheck = Input.GetAxisRaw("Shield");
        if (currentCheck != lastImpact)
        {
            bool check = (currentCheck == 1);
            EventManager.Instance.Raise<bool>("UpdateImpact", check);
            lastImpact = currentCheck;
        }

        currentCheck = Input.GetAxisRaw("Repair");
        if (currentCheck != lastRepair)
        {
            bool check = (currentCheck == 1);
            EventManager.Instance.Raise<bool>("UpdateRepair", check);
            lastRepair = currentCheck;
        }

        currentCheck = Input.GetAxisRaw("Dodge");
        if (currentCheck != lastDodge)
        {
            bool check = (currentCheck == 1);
            EventManager.Instance.Raise<bool>("UpdateDodge", check);
            lastDodge = currentCheck;
        }

        Vector3 currentMousePosition = Input.mousePosition;
        if (currentMousePosition != lastMousePosition)
        {
            EventManager.Instance.Raise<Vector3>("MovedMouse", currentMousePosition);
            lastMousePosition = currentMousePosition;
        }
    }
}
