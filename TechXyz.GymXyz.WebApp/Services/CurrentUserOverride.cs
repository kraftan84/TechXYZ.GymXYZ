using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.WebApp.Services;

public class CurrentUserOverride : ICurrentUserOverride
{
    private static readonly AsyncLocal<string?> _override = new();

    public static string? Current => _override.Value;

    public IDisposable UseTechnicalUser(string userName = "technical")
    {
        var previous = _override.Value;
        _override.Value = (userName);
        return new Restore(() => _override.Value = previous);
    }

    private sealed class Restore : IDisposable
    {
        private readonly Action _restore;
        public Restore(Action restore) => _restore = restore;
        public void Dispose() => _restore();
    }
}