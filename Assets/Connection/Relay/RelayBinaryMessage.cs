


public class RelayBinaryMessage : RelayMessage
{
    public byte[] Buffer { get; }

    public RelayBinaryMessage(ushort id, byte[] buffer) : base(id)
    {
        Buffer = buffer;
    }
}