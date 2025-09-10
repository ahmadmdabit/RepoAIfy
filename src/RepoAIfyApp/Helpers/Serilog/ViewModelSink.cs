using RepoAIfyApp.Services;

using Serilog.Core;
using Serilog.Events;

namespace RepoAIfyApp.Helpers.Serilog;

public class ViewModelSink : ILogEventSink
{
    private readonly UILogRelayService logRelay;
    private readonly IFormatProvider? formatProvider;

    public ViewModelSink(UILogRelayService logRelay, IFormatProvider? formatProvider = null)
    {
        this.logRelay = logRelay;
        this.formatProvider = formatProvider;
    }

    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage(formatProvider);
        logRelay.Publish(message); // Send the message to the relay.
    }
}