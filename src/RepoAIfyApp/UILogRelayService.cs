namespace RepoAIfyApp
{
    // This service is a simple, thread-safe event broadcaster.
    public class UILogRelayService
    {
        public event Action<string>? LogMessagePublished;

        public void Publish(string message)
        {
            LogMessagePublished?.Invoke(message);
        }
    }
}