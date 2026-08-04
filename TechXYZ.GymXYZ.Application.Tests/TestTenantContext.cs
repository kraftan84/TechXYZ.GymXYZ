using TechXyz.GymXyz.Application.Interfaces;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// Ambient tenant for tests. Mutable so a single test can prove that the global
/// filter follows the tenant instead of freezing on the first one seen.
/// </summary>
internal sealed class TestTenantContext : ITenantContext
{
    public TestTenantContext(int tenantId, string? slug = null)
    {
        Current = tenantId;
        Slug = slug;
    }

    public int Current { get; set; }

    public bool IsResolved => Current != 0;

    public string? Slug { get; set; }

    public IDisposable UseTenant(int tenantId, string? slug = null)
    {
        var previousId = Current;
        var previousSlug = Slug;

        Current = tenantId;
        Slug = slug;

        return new Scope(() =>
        {
            Current = previousId;
            Slug = previousSlug;
        });
    }

    private sealed class Scope : IDisposable
    {
        private readonly Action _onDispose;

        public Scope(Action onDispose) => _onDispose = onDispose;

        public void Dispose() => _onDispose();
    }
}
