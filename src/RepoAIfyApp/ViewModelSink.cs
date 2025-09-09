// RepoAIfyApp/ViewModelSink.cs
using Serilog.Core;
using Serilog.Events;

namespace RepoAIfyApp;

public class ViewModelSink : ILogEventSink
{
    private readonly Action<string> _logAction;
    private readonly IFormatProvider? _formatProvider;

    public ViewModelSink(Action<string> logAction, IFormatProvider? formatProvider = null)
    {
        _logAction = logAction;
        _formatProvider = formatProvider;
    }

    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage(_formatProvider);
        _logAction(message);
    }
}
