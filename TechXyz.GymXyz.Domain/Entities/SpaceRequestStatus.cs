namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// Where a request stands. Only <see cref="ToProcess"/> is reachable from the
/// public form — everything after it belongs to the console, which is the lot
/// after this one. Until then requests pile up here, on purpose: the form is the
/// material the console has to be designed against.
/// </summary>
public enum SpaceRequestStatus
{
    ToProcess,
    InProgress,
    Approved,
    Refused
}
