using Unity.Networking.Transport;

public class NetStartGame : NetMessage
{
    public NetStartGame()
    {
        Code = OpCode.StartGame;
    }

    public NetStartGame(DataStreamReader reader)
    {
        Code = OpCode.StartGame;
        Deserialize(reader);
    }

    public override void RecievedOnClient()
    {
        NetUtility.CStartGame?.Invoke(this);
    }

    public override void RecievedOnServer(NetworkConnection cnn)
    {
        NetUtility.SStartGame?.Invoke(this, cnn);
    }
}