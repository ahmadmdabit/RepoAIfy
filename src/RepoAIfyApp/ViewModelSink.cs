using Serilog.Core;
using Serilog.Events;

namespace RepoAIfyApp;

public class ViewModelSink : ILogEventSink
{
    private readonly UILogRelayService _logRelay;
    private readonly IFormatProvider? _formatProvider;

    public ViewModelSink(UILogRelayService logRelay, IFormatProvider? formatProvider = null)
    {
        _logRelay = logRelay;
        _formatProvider = formatProvider;
    }

    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage(_formatProvider);
        _logRelay.Publish(message); // Send the message to the relay.
    }
}