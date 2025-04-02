this is a simple bridge program written in python, it 
expects json in this format from the TCP/IP server

{
                "LeftController": {
                    "Buttons": {
                        "A": false,
                        "B": false,
                        "X": false,
                        "Y": false
                    },
                    "Joystick": {
                        "X": 0,
                        "Y": 0
                    },
                    "Trigger": 0,
                    "Grip": 0
                },
                "RightController": {
                    "Buttons": {
                        "A": false,
                        "B": false,
                        "X": false,
                        "Y": false
                    },
                    "Joystick": {
                        "X": 0,
                        "Y": 0
                    },
                    "Trigger": 0,
                    "Grip": 0
                }
            }

if Left Joystick X is non zero treadmill shall move
if right Joystick Y is non-zero treadmill shall move
treadmill shall stop otherwise