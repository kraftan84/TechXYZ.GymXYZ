using TechXyz.GymXyz.Domain.Common;

namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// A GymXYZ customer. Carries the white-label identity (theme, mark, wordmark),
/// the customer's own contact details and its GymXYZ subscription plan.
/// Adding a customer is a row here plus a block of tokens in themes.css —
/// never a screen change.
/// </summary>
public class Tenant : EntityBase<int>
{
    public Tenant(string name, string slug, string themeKey)
    {
        Name = name;
        Slug = slug;
        ThemeKey = themeKey;
        DisplayName = name;
    }

    /// <summary>Legal/internal name of the customer.</summary>
    public string Name { get; set; }

    /// <summary>Host prefix used to resolve the tenant (<c>teamtrainers</c>.gymxyz.fr).</summary>
    public string Slug { get; set; }

    /// <summary>
    /// Value of the <c>data-theme</c> attribute rendered on &lt;html&gt;. Kept as a
    /// string on purpose: a new brand is a row plus a token block, not a code change.
    /// </summary>
    public string ThemeKey { get; set; }

    /// <summary>Name shown in the UI (topbar, sheets).</summary>
    public string DisplayName { get; set; }

    /// <summary>Tagline shown under the brand ("Révélez-vous").</summary>
    public string? Baseline { get; set; }

    // ---- Brand lockup -------------------------------------------------------

    /// <summary>
    /// Mark asset for light surfaces. Null for a customer that has not supplied
    /// one — the lockup then shows the wordmark alone. There is no default mark:
    /// GymXYZ's own would be a brand leak in a white-label product.
    /// </summary>
    public string? LogoPath { get; set; }

    /// <summary>Mark asset for dark surfaces (dark sidebar themes).</summary>
    public string? LogoDarkPath { get; set; }

    /// <summary>Displays the mark clipped to a circle (Leyssa).</summary>
    public bool CircleLogo { get; set; }

    /// <summary>Whole wordmark when it is not split ("TEAM TRAINER'S").</summary>
    public string? WordmarkText { get; set; }

    /// <summary>First half of a split wordmark ("GYM").</summary>
    public string? WordmarkPrefix { get; set; }

    /// <summary>Second half, rendered in the accent colour ("XYZ").</summary>
    public string? WordmarkAccent { get; set; }

    // ---- Contact ------------------------------------------------------------

    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Siret { get; set; }

    // The address is flat here rather than a link to the Address entity: Address is
    // tenant-scoped (and therefore filtered), while the Tenant sits above the filter.
    public string? Street { get; set; }
    public string? ZipCode { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }

    /// <summary>
    /// Shown instead of a postal address for customers who work on the move
    /// ("Thonon et alentours"). When set, no address is displayed anywhere.
    /// </summary>
    public string? AreaLabel { get; set; }

    public int? Capacity { get; set; }

    /// <summary>Solo coach: the Coachs section disappears from the navigation.</summary>
    public bool IsSolo { get; set; }

    // ---- GymXYZ plan (what the customer pays TechXYZ) ------------------------

    public string? GymPlan { get; set; }
    public decimal? PlanPrice { get; set; }
    public DateOnly? PlanRenewalDate { get; set; }

    public ICollection<Gym>? Gyms { get; set; }

    public void AddGym(Gym gym)
    {
        Gyms ??= new List<Gym>();
        Gyms.Add(gym);
    }
}
