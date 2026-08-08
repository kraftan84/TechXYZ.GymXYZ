using TechXyz.GymXyz.Domain.Common;

namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// Somebody asking for a space — the "demande d'ouverture" of the hand-off. Not a
/// customer, not an account: a request, filed by a stranger through a public form.
/// <para>
/// <b>It belongs to no tenant, and that is deliberate.</b> Every other business
/// entity implements <c>ITenantScoped</c> and is filtered to the customer being
/// served; this one is filled in before a customer exists, so there is nothing to
/// scope it to. It is not a hole in the partitioning, it is what sits before it —
/// the same place <see cref="Tenant"/> itself occupies. Nothing here is reachable
/// from inside a customer's space: the requests that read it carry
/// <c>IPlatformScoped</c> and are pinned by their own perimeter test.
/// </para>
/// </summary>
public class SpaceRequest : EntityBase<int>
{
    public SpaceRequest(string reference, SpaceRequestType type, string name)
    {
        Reference = reference;
        Type = type;
        Name = name;
    }

    /// <summary>DEM-2026-0149. Unique, and what the applicant quotes back.</summary>
    public string Reference { get; set; }

    /// <summary>
    /// Gym or solo coach. Chosen at step 1, and the answer drives the whole rest
    /// of the form — labels, address versus area, which size brackets are offered.
    /// </summary>
    public SpaceRequestType Type { get; set; }

    // ---- The structure ------------------------------------------------------

    public string Name { get; set; }

    /// <summary>French company registration number. A term of art, kept as such.</summary>
    public string? Siret { get; set; }

    /// <summary>The bracket as it was offered, not a number: "50 à 150 membres".</summary>
    public string? SizeLabel { get; set; }

    public string? Disciplines { get; set; }

    public string? Street { get; set; }

    /// <summary>
    /// Also decides the future space's school-holiday zone (<c>SchoolZones</c>),
    /// which is why it is asked here rather than again at provisioning.
    /// </summary>
    public string? ZipCode { get; set; }

    public string? City { get; set; }

    /// <summary>
    /// Where a coach works, when there is no address to give. Named after
    /// <c>Tenant.AreaLabel</c>, which is where it ends up at provisioning.
    /// </summary>
    public string? AreaLabel { get; set; }

    // ---- Who is asking ------------------------------------------------------

    public string ContactFirstName { get; set; } = string.Empty;

    public string ContactLastName { get; set; } = string.Empty;

    public string? ContactRole { get; set; }

    public string ContactEmail { get; set; } = string.Empty;

    public string? ContactPhone { get; set; }

    // ---- What they want -----------------------------------------------------

    /// <summary>A name from <c>PlatformPlans</c>, stored as written.</summary>
    public string RequestedPlan { get; set; } = string.Empty;

    public string? AccentHex { get; set; }

    public string? AccentLabel { get; set; }

    /// <summary>Wanted host prefix, already normalised to letters, digits and dashes.</summary>
    public string RequestedSubdomain { get; set; } = string.Empty;

    public string? LogoAssetPath { get; set; }

    public string? Message { get; set; }

    // ---- Where it stands ----------------------------------------------------

    public SpaceRequestStatus Status { get; set; } = SpaceRequestStatus.ToProcess;

    /// <summary>Who at GymXYZ picked it up. Filled by the console, not by this form.</summary>
    public string? AssigneeUserId { get; set; }

    /// <summary>"Formulaire en ligne", "Site vitrine", "Recommandation"…</summary>
    public string? Source { get; set; }

    public DateTime ReceivedOn { get; set; }

    /// <summary>
    /// When the refusal was pronounced. Null unless <see cref="Status"/> is
    /// <see cref="SpaceRequestStatus.Refused"/> — and it is what the three-month
    /// purge counts from, so a refusal without it would never be deleted.
    /// </summary>
    public DateTime? RefusedOn { get; set; }

    // ---- Consents -----------------------------------------------------------
    //
    // Stored rather than merely required, because the second one is a promise:
    // "supprimées sous 3 mois en cas de refus" is what the purge keeps, and a
    // consent nobody recorded is a promise nobody can show they made.

    public bool AcceptedTerms { get; set; }

    public bool AcceptedDataProcessing { get; set; }

    public bool OptedIntoNewsletter { get; set; }

    public ICollection<SpaceRequestActivity>? Activities { get; set; }

    public ICollection<SpaceRequestNote>? Notes { get; set; }

    /// <summary>Where the applicant is, in the words they used.</summary>
    public string? Where => Type == SpaceRequestType.Coach
        ? AreaLabel
        : string.Join(", ", new[] { City, ZipCode }.Where(part => !string.IsNullOrWhiteSpace(part)));
}
