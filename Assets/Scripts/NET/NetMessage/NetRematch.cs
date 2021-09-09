using Unity.Networking.Transport;

public class NetRematch : NetMessage
{
    public int TeamId;
    public byte WantRematch;

    public NetRematch() // make box
    {
        Code = OpCode.Rematch;
    }

    public NetRematch(DataStreamReader reader) // recieve box
    {
        Code = OpCode.Rematch;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte) Code);
        writer.WriteInt(TeamId);
        writer.WriteByte(WantRematch);
    }

    public override void Deserialize(DataStreamReader reader)
    {
        TeamId = reader.ReadInt();
        WantRematch = reader.ReadByte();
    }

    public override void RecievedOnClient()
    {
        NetUtility.CRematch?.Invoke(this);
    }

    public override void RecievedOnServer(NetworkConnection cnn)
    {
        NetUtility.SRematch?.Invoke(this, cnn);
    }
}