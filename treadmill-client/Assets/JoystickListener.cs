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

    // Reference to the server50505 script
    public tcpipserver50505 serverScript;
    [SerializeField]
    private string statusMsg = "";

    void Start()
    {
        StartCoroutine(InitializeControllers());
    }

    IEnumerator InitializeControllers()
    {
        yield return new WaitForSeconds(1.0f); // Wait for controllers to initialize

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
        if (foundLeft) { CheckLeftJoypad(); }
        if (foundRight) { CheckRightJoypad(); }
        CheckDevices();
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

    void CheckLeftJoypad()
    {
        if (leftController.isValid)
        {
            Vector2 joystickValue;
            if (leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out joystickValue))
            {
                if (joystickValue.y != 0) // Moving left or right
                {
                    UpdateStatus("Left Joypad Moved Horizontally: " + joystickValue.y);
                    serverScript.SendNetworkMessage(
                        "{" +
                        "msg:leftstick," +
                        "data:{" +
                            "y:"+ joystickValue.y.ToString("0.0")+
                        "}" +
                        "}"); // Call the test() function
                }
            }
        }
    }

    void CheckRightJoypad()
    {
        if (rightController.isValid)
        {
            Vector2 joystickValue;
            if (rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out joystickValue))
            {
                if (joystickValue.x > 0) // Moving forward
                {
                    UpdateStatus("Right Joypad Moved Forward: " + joystickValue.x);
                    serverScript.SendNetworkMessage(
                        "{" +
                        "msg:rightstick," +
                        "data:{" +
                            "x:" + joystickValue.x.ToString("0.0") +
                        "}" +
                        "}"); // Call the test() function
                }
            }
        }
    }
}
