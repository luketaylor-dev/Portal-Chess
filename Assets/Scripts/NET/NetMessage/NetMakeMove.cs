using Unity.Networking.Transport;

public class NetMakeMove : NetMessage
{
    public int DestinationX;
    public int DestinationY;
    public int OriginalX;
    public int OriginalY;
    public int TeamId;

    public NetMakeMove() // make box
    {
        Code = OpCode.MakeMove;
    }

    public NetMakeMove(DataStreamReader reader) // recieve box
    {
        Code = OpCode.MakeMove;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte) Code);
        writer.WriteInt(OriginalX);
        writer.WriteInt(OriginalY);
        writer.WriteInt(DestinationX);
        writer.WriteInt(DestinationY);
        writer.WriteInt(TeamId);
    }

    public override void Deserialize(DataStreamReader reader)
    {
        OriginalX = reader.ReadInt();
        OriginalY = reader.ReadInt();
        DestinationX = reader.ReadInt();
        DestinationY = reader.ReadInt();
        TeamId = reader.ReadInt();
    }

    public override void RecievedOnClient()
    {
        NetUtility.CMakeMove?.Invoke(this);
    }

    public override void RecievedOnServer(NetworkConnection cnn)
    {
        NetUtility.SMakeMove?.Invoke(this, cnn);
    }
}