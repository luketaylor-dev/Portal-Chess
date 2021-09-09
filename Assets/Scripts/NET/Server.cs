using System;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class Server : MonoBehaviour
{
    private const float KeepAliveTickRate = 20.0f;

    public Action ConnectionDropped;
    private NativeList<NetworkConnection> _connections;

    public NetworkDriver Driver;

    private bool _isActive;
    private float _lastKeepAlive;

    public void Update()
    {
        if (!_isActive)
            return;

        KeepAlive();

        Driver.ScheduleUpdate().Complete();
        CleanupConnections();
        AcceptNewConnections();
        UpdateMessagePump();
    }

    public void OnDestroy()
    {
        Shutdown();
    }

    //Methods
    public void Init(ushort port)
    {
        Driver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = port;

        if (Driver.Bind(endpoint) != 0)
        {
            Debug.Log("Unable to bind on port " + endpoint.Port);
            return;
        }

        Driver.Listen();
        Debug.Log("Currently listening on port " + endpoint.Port);

        _connections = new NativeList<NetworkConnection>(2, Allocator.Persistent);
        _isActive = true;
    }

    public void Shutdown()
    {
        if (_isActive)
        {
            Driver.Dispose();
            _connections.Dispose();
            _isActive = false;
        }
    }

    private void KeepAlive()
    {
        if (Time.time - _lastKeepAlive > KeepAliveTickRate)
        {
            _lastKeepAlive = Time.time;
            Broadcast(new NetKeepAlive());
        }
    }

    private void CleanupConnections()
    {
        for (var i = 0; i < _connections.Length; i++)
            if (!_connections[i].IsCreated)
            {
                _connections.RemoveAtSwapBack(i);
                --i;
            }
    }

    private void AcceptNewConnections()
    {
        NetworkConnection c;
        while ((c = Driver.Accept()) != default) _connections.Add(c);
    }

    private void UpdateMessagePump()
    {
        DataStreamReader stream;
        for (var i = 0; i < _connections.Length; i++)
        {
            NetworkEvent.Type cmd;
            while ((cmd = Driver.PopEventForConnection(_connections[i], out stream)) != NetworkEvent.Type.Empty)
                if (cmd == NetworkEvent.Type.Data)
                {
                    NetUtility.OnData(stream, _connections[i], this);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from server");
                    _connections[i] = default;
                    ConnectionDropped?.Invoke();
                    Shutdown(); //only needed because 2 person game
                }
        }
    }

    //Server specific
    public void SendToClient(NetworkConnection connection, NetMessage msg)
    {
        DataStreamWriter writer;
        Driver.BeginSend(connection, out writer);
        msg.Serialize(ref writer);
        Driver.EndSend(writer);
    }

    public void Broadcast(NetMessage msg)
    {
        for (var i = 0; i < _connections.Length; i++)
            if (_connections[i].IsCreated)
                //Debug.Log($"Sending {msg.Code} to : {connections[i].InternalId}");
                SendToClient(_connections[i], msg);
    }

    #region singleton implementation

    public static Server Instance { get; set; }
    //public object NetUtility { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    #endregion
}