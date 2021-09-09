using Unity.Networking.Transport;

public class NetKeepAlive : NetMessage
{
    public NetKeepAlive() // make box
    {
        Code = OpCode.KeepAlive;
    }

    public NetKeepAlive(DataStreamReader reader) // recieve box
    {
        Code = OpCode.KeepAlive;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte) Code);
    }

    public override void Deserialize(DataStreamReader reader)
    {
    }

    public override void RecievedOnClient()
    {
        NetUtility.CKeepAlive?.Invoke(this);
    }

    public override void RecievedOnServer(NetworkConnection cnn)
    {
        NetUtility.SKeepAlive?.Invoke(this, cnn);
    }
}