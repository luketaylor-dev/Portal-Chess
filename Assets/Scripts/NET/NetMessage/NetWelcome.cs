using Unity.Networking.Transport;

public class NetWelcome : NetMessage
{
    public NetWelcome()
    {
        Code = OpCode.Welcome;
    }

    public NetWelcome(DataStreamReader reader)
    {
        Code = OpCode.Welcome;
        Deserialize(reader);
    }

    public int AssignedTeam { get; set; }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte) Code);
        writer.WriteInt(AssignedTeam);
    }

    public override void Deserialize(DataStreamReader reader)
    {
        AssignedTeam = reader.ReadInt();
    }

    public override void RecievedOnClient()
    {
        NetUtility.CWelcome?.Invoke(this);
    }

    public override void RecievedOnServer(NetworkConnection cnn)
    {
        NetUtility.SWelcome?.Invoke(this, cnn);
    }
}