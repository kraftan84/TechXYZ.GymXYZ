namespace TechXyz.GymXyz.Application.Interfaces;

/// <summary>
/// Marks a request that runs <b>outside every customer</b> — it reads or writes
/// rows that belong to GymXYZ itself rather than to a tenant, and it must work
/// with no ambient tenant at all.
/// <para>
/// The whole product is built on the opposite assumption: entities implement
/// <c>ITenantScoped</c>, a global query filter narrows them to the current
/// customer, and anything that escapes that filter is a partitioning bug. The
/// demande d'ouverture is the first thing that legitimately sits before it — it
/// is filled in by a stranger, for a customer that does not exist yet.
/// </para>
/// <para>
/// Which is exactly why it is named. "It works because the entity happens not to
/// implement <c>ITenantScoped</c>" is a fact nobody can grep for and everybody
/// has to rediscover; this can be grepped, and
/// <c>PlatformScopedPerimeterTests</c> makes a new request say out loud which
/// side of the line it is on. The console lot has more of these coming — the
/// customer list, the plan catalogue, the platform log — and they should all
/// arrive wearing the same word.
/// </para>
/// <para>
/// It grants nothing. Reaching a platform screen is still a matter of
/// <c>GymPolicies.PlatformAdmin</c> on the page; this only says that no tenant
/// filter applies, and that the request had better not assume one.
/// </para>
/// </summary>
public interface IPlatformScoped;
