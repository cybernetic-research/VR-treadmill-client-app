using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class encoder : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField]
    public class ControllerData
    {
        public Controller LeftController = new Controller();
        public Controller RightController = new Controller();
    }

    public class Controller
    {
        public Buttons Buttons = new Buttons();
        public Joystick Joystick = new Joystick();
        public double Trigger = 0.0f;
        public double Grip = 0.0f;
    }

    public class Buttons
    {
        public bool A { get; set; }
        public bool B { get; set; }
        public bool X { get; set; }
        public bool Y { get; set; }
    }

    public class Joystick
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
    public string ConvertStructToJson(ControllerData data)
    {
        // Manually construct the JSON string
        string json = $@"
            {{
                ""LeftController"": {{
                    ""Buttons"": {{
                        ""A"": {data.LeftController.Buttons.A.ToString().ToLower()},
                        ""B"": {data.LeftController.Buttons.B.ToString().ToLower()},
                        ""X"": {data.LeftController.Buttons.X.ToString().ToLower()},
                        ""Y"": {data.LeftController.Buttons.Y.ToString().ToLower()}
                    }},
                    ""Joystick"": {{
                        ""X"": {data.LeftController.Joystick.X},
                        ""Y"": {data.LeftController.Joystick.Y}
                    }},
                    ""Trigger"": {data.LeftController.Trigger},
                    ""Grip"": {data.LeftController.Grip}
                }},
                ""RightController"": {{
                    ""Buttons"": {{
                        ""A"": {data.RightController.Buttons.A.ToString().ToLower()},
                        ""B"": {data.RightController.Buttons.B.ToString().ToLower()},
                        ""X"": {data.RightController.Buttons.X.ToString().ToLower()},
                        ""Y"": {data.RightController.Buttons.Y.ToString().ToLower()}
                    }},
                    ""Joystick"": {{
                        ""X"": {data.RightController.Joystick.X},
                        ""Y"": {data.RightController.Joystick.Y}
                    }},
                    ""Trigger"": {data.RightController.Trigger},
                    ""Grip"": {data.RightController.Grip}
                }}
            }}";

        return json;
    }
}
