using Tcc.Themes.Fallback;

namespace Tcc.DesktopHost;

public sealed class MainWindowViewModel
{
    public MainWindowViewModel(
        BuiltInThemePresentationSnapshot presentation,
        string startupNotice)
    {
        Presentation = presentation ?? throw new ArgumentNullException(nameof(presentation));
        Status = startupNotice ?? throw new ArgumentNullException(nameof(startupNotice));
    }

    public string Status { get; }

    public BuiltInThemePresentationSnapshot Presentation { get; }
}
