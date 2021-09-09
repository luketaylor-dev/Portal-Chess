using System;
using Unity.Networking.Transport;
using UnityEngine;

public class Client : MonoBehaviour
{
    private NetworkConnection _connection;

    public Action ConnectionDropped;

    public NetworkDriver Driver;

    private bool _isActive;

    public void Update()
    {
        if (!_isActive)
            return;

        Driver.ScheduleUpdate().Complete();
        CheckAlive();
        UpdateMessagePump();
    }

    public void OnDestroy()
    {
        Shutdown();
    }

    //Methods
    public void Init(string ip, ushort port)
    {
        Driver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.Parse(ip, port);

        _connection = Driver.Connect(endpoint);

        Debug.Log("Attemtping to connect to server on " + endpoint.Address);

        _isActive = true;

        RegisterToEvent();
    }

    public void Shutdown()
    {
        if (_isActive)
        {
            UnregisterToEvent();
            Driver.Dispose();
            _isActive = false;
            _connection = default;
        }
    }

    private void CheckAlive()
    {
        if (!_connection.IsCreated && _isActive)
        {
            Debug.Log("Something went wrong, lost connection to server");
            ConnectionDropped?.Invoke();
            Shutdown();
        }
    }

    private void UpdateMessagePump()
    {
        DataStreamReader stream;
        NetworkEvent.Type cmd;
        while ((cmd = _connection.PopEvent(Driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                SendToServer(new NetWelcome());
                Debug.Log("We're connected :D");
            }

            if (cmd == NetworkEvent.Type.Data)
            {
                NetUtility.OnData(stream, default);
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server");
                _connection = default;
                ConnectionDropped?.Invoke();
                Shutdown();
            }
        }
    }

    public void SendToServer(NetMessage msg)
    {
        DataStreamWriter writer;
        Driver.BeginSend(_connection, out writer);
        msg.Serialize(ref writer);
        Driver.EndSend(writer);
    }

    private void RegisterToEvent()
    {
        NetUtility.CKeepAlive += OnKeepAlive;
    }

    private void UnregisterToEvent()
    {
        NetUtility.CKeepAlive -= OnKeepAlive;
    }

    private void OnKeepAlive(NetMessage nm)
    {
        SendToServer(nm);
    }

    #region singleton implementation

    public static Client Instance { get; set; }
    //public object NetUtility { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    #endregion
}