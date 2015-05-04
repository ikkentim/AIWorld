namespace AIWorld
{
    public interface IMessageHandler
    {
        void HandleMessage(int message, int contents);
    }
}