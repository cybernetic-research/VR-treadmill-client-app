using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static encoder;

public class JoystickListener : MonoBehaviour
{
    bool foundLeft = false;
    bool foundRight = false;
    private InputDevice leftController;
    private InputDevice rightController;
    const float MIN_DELTA = 0.01f;
    const float MOTION_THRESHOLD = 0.25f;

    public encoder joyEncoder;

    encoder.ControllerData controllerData = new encoder.ControllerData();
    // Reference to the server50505 script
    public tcpipserver50505 serverScript;
    [SerializeField]
    private string statusMsg = "";
    [SerializeField]
    private string LeftX;
    [SerializeField]
    private string LeftY;
    [SerializeField]
    private string RightX;
    [SerializeField]
    private string RightY;

    void Start()
    {
        StartCoroutine(InitializeControllers());
        SendLatestControllerState();
    }

    IEnumerator InitializeControllers()
    {
        yield return new WaitForSeconds(3.0f); // Wait for controllers to initialize

        leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        UpdateStatus($"Left Controller Valid? {leftController.isValid}");
        UpdateStatus($"Right Controller Valid? {rightController.isValid}");

        if (!leftController.isValid)
        {
            UpdateStatus("Left Controller is not valid! Check connections and profiles.");
        }
        if (!rightController.isValid)
        {
            UpdateStatus("Right Controller is not valid! Check connections and profiles.");
        }
    }

    void OnEnable()
    {
        InputDevices.deviceConnected += OnDeviceConnected;
    }

    void OnDisable()
    {
        InputDevices.deviceConnected -= OnDeviceConnected;
    }

    private void OnDeviceConnected(InputDevice device)
    {
        if (device.characteristics.HasFlag(InputDeviceCharacteristics.Left))
        {
            leftController = device;
            UpdateStatus("Left Controller connected.");
        }
        else if (device.characteristics.HasFlag(InputDeviceCharacteristics.Right))
        {
            rightController = device;
            UpdateStatus("Right Controller connected.");
        }
    }


    void UpdateStatus(string input)
    {
        statusMsg = input;
        Debug.Log(statusMsg);
    }


    public float interval = 0.1f; // Delay in seconds (500ms by default)
    private float timeSinceLastCall = 0f; // Tracks time elapsed since the last call
    void Update()
    {
        bool change = false;
        if (foundLeft) { if (UpdateLeftJoypad()) { change = true; } }
        if (foundRight) { if (UpdateRightJoypad()) { change = true; } }
        CheckDevices();
        timeSinceLastCall += Time.deltaTime;

        if (timeSinceLastCall >= interval)
        {
            
            timeSinceLastCall = 0f; // Reset the timer
        }
        TransmitState(change);
    }

    const uint GO = 1;
    const uint STOP = 0;
    uint oldState = STOP;
    uint currentState = STOP;
    uint leftState = STOP;
    uint rightState = STOP;

    void TransmitState(bool change)
    {
        if ((rightState == STOP) && (leftState == STOP))
        {
            currentState = STOP;
        }
        else if ((rightState == GO) || (leftState == GO))
        {
            currentState = GO;
        }

        if (oldState != currentState)
        {
            oldState = currentState;
            if (currentState == STOP)
            {
                controllerData.LeftController.Joystick.X = 0.0f;
                controllerData.LeftController.Joystick.Y = 0.0f;
                controllerData.RightController.Joystick.X = 0.0f;
                controllerData.RightController.Joystick.Y = 0.0f;
            }
            else
            {
                controllerData.LeftController.Joystick.X = 1.0f;
                controllerData.LeftController.Joystick.Y = 0.0f;
                controllerData.RightController.Joystick.X = 0.0f;
                controllerData.RightController.Joystick.Y = 0.0f;
            }
            SendLatestControllerState();

            LeftX =  controllerData.LeftController.Joystick.X.ToString("0.00");
            LeftY =  controllerData.LeftController.Joystick.Y.ToString("0.00");
            RightX = controllerData.RightController.Joystick.X.ToString("0.00");
            RightY = controllerData.RightController.Joystick.Y.ToString("0.00");
        }
    }

    void SendLatestControllerState()
    {
        string msg = joyEncoder.ConvertStructToJson(controllerData);
        serverScript.SendNetworkMessage(msg); // Call the test() function
    }




    bool UpdateLeftJoypad()
    {
        bool bResult = false;
        controllerData.LeftController.Joystick.X = 0.0f;
        controllerData.LeftController.Joystick.Y = 0.0f;
        if (leftController.isValid)
        {
            Vector2 joystickValue;
            if (leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out joystickValue))
            {
                controllerData.LeftController.Joystick.X = joystickValue.x;
                controllerData.LeftController.Joystick.Y = joystickValue.y;
            }
        }


        if (Math.Abs(controllerData.LeftController.Joystick.Y) > MOTION_THRESHOLD)
        {
            leftState = GO;
        }
        else 
        {
            leftState = STOP;
        }
        return (bResult);
    }

 
    bool UpdateRightJoypad()
    {
        bool bResult = false;
        controllerData.RightController.Joystick.X = 0.0f; 
        controllerData.RightController.Joystick.Y = 0.0f;
        if (rightController.isValid)
        {
            Vector2 joystickValue;
            if (rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out joystickValue))
            {
                controllerData.RightController.Joystick.X = joystickValue.x;
                controllerData.RightController.Joystick.Y = joystickValue.y;
            }
        }

        if (Math.Abs(controllerData.RightController.Joystick.X) > MOTION_THRESHOLD)
        {
            rightState = GO;
        }
        else
        {
            rightState = STOP;
        }

        return (bResult);
    }

    void CheckDevices()
    {
        var devices = new List<InputDevice>();
        InputDevices.GetDevices(devices);

        foreach (var device in devices)
        {
            if (device.characteristics.HasFlag(InputDeviceCharacteristics.Left))
            {
                leftController = device;
                UpdateStatus("Left Controller found via enumeration.");
                foundLeft = true;
            }
            if (device.characteristics.HasFlag(InputDeviceCharacteristics.Right))
            {
                rightController = device;
                UpdateStatus("Right Controller found via enumeration.");
                foundRight = true;
            }
        }
    }

   
}
