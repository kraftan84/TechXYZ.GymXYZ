using MediatR;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Deletes refused requests three months after the refusal.
/// <para>
/// This is not a retention policy somebody chose — it is a sentence the applicant
/// was made to tick: « Données hébergées en France, supprimées sous 3 mois en cas
/// de refus. » A promise made at the moment of collection, so the code that keeps
/// it ships with the form that made it, not with the console that will one day
/// pronounce the refusals.
/// </para>
/// <para>
/// <see cref="IPlatformScoped"/>, like everything else touching these rows, and
/// run by a background sweep with no user at all.
/// </para>
/// </summary>
public sealed class PurgeRefusedSpaceRequestsCommand : IRequest<int>, IPlatformScoped
{
    /// <summary>What was promised: three months, counted from the refusal.</summary>
    public static readonly TimeSpan RetentionAfterRefusal = TimeSpan.FromDays(90);
}
