namespace TechXyz.GymXyz.Domain.Entities;

public enum TenantMarkKind
{
    /// <summary>GymXYZ has no logo file: its mark is a kettlebell drawn in SVG.</summary>
    Kettlebell,

    /// <summary>The mark comes from an image asset (see <c>Tenant.LogoPath</c>).</summary>
    Image
}
