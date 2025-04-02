import asyncio
import socket
import sys
import re
import json
from bleak import BleakScanner, BleakClient, BleakError

# Constants for BLE
DEVICE_NAME = "VR Treadmill"
SERVICE_UUID = "de2f6573-5e52-4a14-b5f3-5e562ea02d70"
CONTROL_CHAR_UUID = "de2f6573-5e52-4a14-b5f3-5e562ea02d71"
STATE_CHAR_UUID = "de2f6573-5e52-4a14-b5f3-5e562ea02d72"

# Global BLE client
ble_client = None

HOST = sys.argv[1]  # Standard loopback interface address (localhost)
PORT = int(sys.argv[2])  # Port to listen on (non-privileged ports are > 1023)
ble_connected = False


async def connect_ble():
    """
    Connect to the BLE device using bleak.
    """
    global ble_client, ble_connected
    print("Scanning for VR Treadmill...")
    devices = await BleakScanner.discover()
    target_device = next((d for d in devices if d.name == DEVICE_NAME), None)

    if not target_device:
        print("VR Treadmill not found. Retrying in 5 seconds...")
        await asyncio.sleep(5)
        return False

    try:
        ble_client = BleakClient(target_device.address)
        await ble_client.connect()
        print("Connected to VR Treadmill")
        ble_connected = True
        return True
    except BleakError as e:
        print(f"BLE connection error: {e}")
        return False


async def send_ble_command(command):
    """
    Send a command to the BLE device.
    """
    global ble_client, ble_connected

    if not ble_connected or not ble_client or not ble_client.is_connected:
        print("Reconnecting to BLE...")
        ble_connected = await connect_ble()
        if not ble_connected:
            print("Failed to connect BLE. Skipping command.")
            return

    try:
        await ble_client.write_gatt_char(CONTROL_CHAR_UUID, bytearray([command]))
        print(f"Sent command {command} to BLE device")
    except Exception as e:
        print(f"Failed to send command to BLE device: {e}")



def extract_controller_joystick(data):
    """
    Extract joystick X and Y values for both LeftController and RightController.
    """
    try:
        json_data = json.loads(data)  # Parse the JSON string into a Python dictionary

        # Extract joystick values for LeftController
        left_joystick = json_data.get("LeftController", {}).get("Joystick", {})
        left_x = left_joystick.get("X", None)
        left_y = left_joystick.get("Y", None)

        # Extract joystick values for RightController
        right_joystick = json_data.get("RightController", {}).get("Joystick", {})
        right_x = right_joystick.get("X", None)
        right_y = right_joystick.get("Y", None)

        print(f"LeftController Joystick - X: {left_x}, Y: {left_y}")
        print(f"RightController Joystick - X: {right_x}, Y: {right_y}")

        return left_x, left_y, right_x, right_y

    except json.JSONDecodeError as e:
        print(f"Failed to parse JSON: {e}")
        return None, None, None, None


async def process_data(data):
    """
    Process received data from TCP/IP client.
    """
    print(f"Received {data!r}")

    # Extract Left and Right Joystick X and Y values
    #left_x, left_y, right_x, right_y = 0

    left_x, left_y, right_x, right_y = extract_controller_joystick(data)
    #if 'Joystick' in data:
    #    match = re.search(r'Joystick:\{(-?\d+\.?\d*):(-?\d+\.?\d*)\}', data)
    #    if match:
    #        left_x = float(match.group(1))
    #        right_y = float(match.group(2))

    # Debug print
    print(f"LeftJoystickX: {left_x}, RightJoystickY: {right_y}")

    # Check conditions for sending BLE commands
    if (left_x is not None and abs(left_x) >= 0.1) or (right_y is not None and abs(right_y) >= 0.1):
        print("GO")
        await send_ble_command(1)  # Send '1' for GO
    else:
        print("STOP")
        await send_ble_command(0)  # Send '0' for STOP


async def client_loop():
    """
    TCP/IP client loop.
    """
    while True:
        print(f"Connecting to server {HOST!r} {PORT!r}")
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            s.connect((HOST, PORT))
            s.sendall(b"Hello, world")
            while True:
                data = s.recv(1024)
                if not data:
                    break
                else:
                    await process_data(data.decode("utf-8"))


async def main():
    """
    Main function to run both TCP/IP and BLE operations.
    """
    global ble_connected
    ble_connected = await connect_ble()
    if not ble_connected:
        print("BLE connection failed. Exiting...")
        return

    try:
        await client_loop()
    except KeyboardInterrupt:
        print("\nExiting application.")
    finally:
        if ble_client and ble_client.is_connected:
            await ble_client.disconnect()
            print("Disconnected from BLE device.")


if __name__ == "__main__":
    asyncio.run(main())
