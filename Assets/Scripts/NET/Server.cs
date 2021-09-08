using System;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class Server : MonoBehaviour
{
    private const float keepAliveTickRate = 20.0f;

    public Action connectionDropped;
    private NativeList<NetworkConnection> connections;

    public NetworkDriver driver;

    private bool isActive;
    private float lastKeepAlive;

    public void Update()
    {
        if (!isActive)
            return;

        KeepAlive();

        driver.ScheduleUpdate().Complete();
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
        driver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = port;

        if (driver.Bind(endpoint) != 0)
        {
            Debug.Log("Unable to bind on port " + endpoint.Port);
            return;
        }

        driver.Listen();
        Debug.Log("Currently listening on port " + endpoint.Port);

        connections = new NativeList<NetworkConnection>(2, Allocator.Persistent);
        isActive = true;
    }

    public void Shutdown()
    {
        if (isActive)
        {
            driver.Dispose();
            connections.Dispose();
            isActive = false;
        }
    }

    private void KeepAlive()
    {
        if (Time.time - lastKeepAlive > keepAliveTickRate)
        {
            lastKeepAlive = Time.time;
            Broadcast(new NetKeepAlive());
        }
    }

    private void CleanupConnections()
    {
        for (var i = 0; i < connections.Length; i++)
            if (!connections[i].IsCreated)
            {
                connections.RemoveAtSwapBack(i);
                --i;
            }
    }

    private void AcceptNewConnections()
    {
        NetworkConnection c;
        while ((c = driver.Accept()) != default) connections.Add(c);
    }

    private void UpdateMessagePump()
    {
        DataStreamReader stream;
        for (var i = 0; i < connections.Length; i++)
        {
            NetworkEvent.Type cmd;
            while ((cmd = driver.PopEventForConnection(connections[i], out stream)) != NetworkEvent.Type.Empty)
                if (cmd == NetworkEvent.Type.Data)
                {
                    NetUtility.OnData(stream, connections[i], this);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from server");
                    connections[i] = default;
                    connectionDropped?.Invoke();
                    Shutdown(); //only needed because 2 person game
                }
        }
    }

    //Server specific
    public void SendToClient(NetworkConnection connection, NetMessage msg)
    {
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        msg.Serialize(ref writer);
        driver.EndSend(writer);
    }

    public void Broadcast(NetMessage msg)
    {
        for (var i = 0; i < connections.Length; i++)
            if (connections[i].IsCreated)
                //Debug.Log($"Sending {msg.Code} to : {connections[i].InternalId}");
                SendToClient(connections[i], msg);
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