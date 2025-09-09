using System.Windows.Controls;
using System.Windows.Threading;

using Serilog.Core;
using Serilog.Events;

namespace RepoAIfyApp;

public class TextBoxSink : ILogEventSink
{
    private readonly TextBox _textBox;
    private readonly IFormatProvider? _formatProvider;

    public TextBoxSink(TextBox textBox, IFormatProvider? formatProvider)
    {
        _textBox = textBox;
        _formatProvider = formatProvider;
    }

    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage(_formatProvider);
        _textBox.Dispatcher.Invoke(() =>
        {
            _textBox.AppendText(message + Environment.NewLine);
            _textBox.ScrollToEnd();
        });
    }
}
