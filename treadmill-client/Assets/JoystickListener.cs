using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class JoystickListener : MonoBehaviour
{
    bool foundLeft = false;
    bool foundRight = false;
    private InputDevice leftController;
    private InputDevice rightController;
    const float MIN_DELTA = 0.01f;

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

    

    void Update()
    {
        bool change = false;
        if (foundLeft) { if (UpdateLeftJoypad()) { change = true; } }
        if (foundRight) { if (UpdateRightJoypad()) { change = true; } }
        CheckDevices();
        if (change)
        {   //send latest update
            SendLatestControllerState();

            LeftX = controllerData.LeftController.Joystick.X.ToString("0.00");
            LeftY = controllerData.LeftController.Joystick.Y.ToString("0.00");
            RightX = controllerData.RightController.Joystick.X.ToString("0.00");
            RightY = controllerData.RightController.Joystick.Y.ToString("0.00");
        }
    }

    void SendLatestControllerState()
    {
        string msg = joyEncoder.ConvertStructToJson(controllerData);
        serverScript.SendNetworkMessage(msg); // Call the test() function
    }





    private double previousLeftX = 0.0f;
    private double previousLeftY = 0.0f;

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
        
        if (Math.Abs(controllerData.LeftController.Joystick.X - previousLeftX) > MIN_DELTA ||
            Math.Abs(controllerData.LeftController.Joystick.Y - previousLeftY) > MIN_DELTA)
        {
            previousLeftX = controllerData.LeftController.Joystick.X;
            previousLeftY = controllerData.LeftController.Joystick.Y;
            bResult = true;
        }
        return (bResult);
    }

    private double previousRightX = 0.0f;
    private double previousRightY = 0.0f;
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
        
        if (Math.Abs(controllerData.RightController.Joystick.X - previousRightX) > MIN_DELTA ||
            Math.Abs(controllerData.RightController.Joystick.Y - previousRightY) > MIN_DELTA)
        {
            previousRightX = controllerData.RightController.Joystick.X;
            previousRightY = controllerData.RightController.Joystick.Y;
            bResult = true;
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
