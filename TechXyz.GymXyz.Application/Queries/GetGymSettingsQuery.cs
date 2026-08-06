using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

/// <summary>
/// The whole Réglages screen in one read. Parameterless like
/// <c>GetPlansQuery</c>: the tenant is ambient, and the four panels are tabs of
/// one page rather than four things to ask for separately.
/// </summary>
public sealed record GetGymSettingsQuery : IRequest<GymSettingsPageDto>;
