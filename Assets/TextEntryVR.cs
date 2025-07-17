using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TMPro;

public class TextEntryVR : MonoBehaviour
{
    public TextMeshPro  inputField;
    UdpClient udpClient;
    IPEndPoint endPoint;

    // Public event for external listeners (like Calibration.cs)
    public delegate void CharReceivedHandler(string character);
    public event CharReceivedHandler OnCharReceived;

    void Start()
    {
        udpClient = new UdpClient(5005);
        endPoint = new IPEndPoint(IPAddress.Any, 0);
        udpClient.Client.Blocking = false; // So Receive() doesn't freeze Unity
    }

    void Update()
    {
        try
        {
            if (udpClient.Available > 0)
            {
                byte[] data = udpClient.Receive(ref endPoint);
                string receivedChar = Encoding.UTF8.GetString(data);

                Debug.Log("Received: " + receivedChar);

                if (inputField != null)
                    inputField.text += receivedChar;  // Append only current char

                OnCharReceived?.Invoke(receivedChar);
            }
        }
        catch (SocketException)
        {
            // No data available — do nothing
        }
    }

    void OnApplicationQuit()
    {
        udpClient.Close();
    }
}