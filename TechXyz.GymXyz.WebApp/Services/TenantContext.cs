using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.WebApp.Services;

/// <summary>
/// Scoped holder for the ambient tenant. Filled once per scope by
/// <c>TenantScope</c> (HTTP request or Blazor circuit) from the signed-in user's
/// claims, with the host as a fallback before authentication.
/// </summary>
public sealed class TenantContext : ITenantContext
{
    private int _tenantId;
    private string? _slug;
    private int? _overrideTenantId;
    private string? _overrideSlug;

    public int Current => _overrideTenantId ?? _tenantId;

    public bool IsResolved => Current != 0;

    public string? Slug => _overrideTenantId.HasValue ? _overrideSlug : _slug;

    public void SetTenant(int tenantId, string? slug)
    {
        _tenantId = tenantId;
        _slug = slug;
    }

    public IDisposable UseTenant(int tenantId, string? slug = null)
    {
        var previousId = _overrideTenantId;
        var previousSlug = _overrideSlug;

        _overrideTenantId = tenantId;
        _overrideSlug = slug;

        return new Scope(() =>
        {
            _overrideTenantId = previousId;
            _overrideSlug = previousSlug;
        });
    }

    private sealed class Scope : IDisposable
    {
        private readonly Action _onDispose;
        private bool _disposed;

        public Scope(Action onDispose) => _onDispose = onDispose;

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _onDispose();
        }
    }
}
