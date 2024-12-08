using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class ElementState : MonoBehaviour
{
    // singleton class
    public static ElementState instance;
    // check for existing instances of the class
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public enum State
    {
        none,
        fire,
        water,
        lightning,
        air
    }
    public State state;

    // Use events to notify other scripts when an element is active or deactivated
    public delegate void OnFireActive();
    public delegate void OnWaterActive();
    public delegate void OnAirActive();
    public delegate void OnLightningActive();
    public delegate void OnElementDeactivate();

    public static event OnFireActive onFireActive;
    public static event OnWaterActive onWaterActive;
    public static event OnAirActive onAirActive;
    public static event OnLightningActive onLightningActive;
    public static event OnElementDeactivate onElementDeactivate;


    private float elementSwapCooldown = 1f;
    private float elementSwapTimer = 1f;
    private float checkElementConditionsTimer = 0.1f;

    [SerializeField] private PoseScriptableObject pose;

    private void Start()
    {
        state = State.none;
        if (pose == null)
        {
            pose = FindObjectOfType<PoseScriptableObject>();
        }
        StartCoroutine(CheckElementConditions());
    }

    // Check element conditions every checkElementConditionsTimer seconds
    IEnumerator CheckElementConditions()
    {
        while (true)
        {
            elementSwapTimer -= Time.deltaTime;
            if (elementSwapTimer <= 0)
            {
                CheckElementState();
            }
            yield return new WaitForSeconds(checkElementConditionsTimer);
        }
    }

    // Check conditions for each element state
    private void CheckElementState()
    {
        switch (state)
        {
            // if not in a state assign state
            case State.none:
                // check for air state
                if (pose.isLeftHandAboveShoulder && pose.isRightHandAboveShoulder)
                {
                    SwapToElement(State.air);
                }
                // check for water state
                else if (!pose.isLeftHandAboveShoulder && pose.isRightHandAboveShoulder)
                {
                    SwapToElement(State.water);
                }
                // check for fire state
                else if (pose.isLeftHandAboveShoulder && !pose.isRightHandAboveShoulder)
                {
                    SwapToElement(State.fire);
                }
                else if (pose.closeHands)
                {
                    SwapToElement(State.lightning);
                }
                break;

            // stop water if right hand is above shoulder
            case State.water:
                if (!pose.isLeftHandAboveShoulder && pose.isRightHandAboveShoulder)
                {
                    SwapToElement(State.none);
                }
                break;

            // stop fire if left hand is above shoulder
            case State.fire:
                if (pose.isLeftHandAboveShoulder && !pose.isRightHandAboveShoulder)
                {
                    SwapToElement(State.none);
                }
                break;

            // stop lightning if hands are not close
            case State.lightning:
                if (!pose.closeHands)
                {
                    SwapToElement(State.none);
                }
                break;

            // stop air if both hands are not above shoulder
            case State.air:
                if (!pose.isLeftHandAboveShoulder && !pose.isRightHandAboveShoulder)
                {
                    SwapToElement(State.none);
                }
                break;
            default:
                break;
        }
    }


    // Swap to target element and initiate element swap cooldown
    private void SwapToElement(State element)
    {
        elementSwapTimer = elementSwapCooldown;
        state = element;

        // Notify other scripts that an element is active or deactivated
        switch (element)
        {
            case State.fire:
                onFireActive?.Invoke();
                break;
            case State.water:
                onWaterActive?.Invoke();
                break;
            case State.air:
                onAirActive?.Invoke();
                break;
            case State.lightning:
                onLightningActive?.Invoke();
                break;
            case State.none:
                onElementDeactivate?.Invoke();
                break;
            default:
                break;
        }
    }
}
