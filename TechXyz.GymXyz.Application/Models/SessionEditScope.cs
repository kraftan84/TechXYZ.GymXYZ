namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// How far an edit or a cancellation reaches inside a series. Only forward:
/// rewriting occurrences that already happened is what copying the capacity onto
/// each row exists to prevent.
/// </summary>
public enum SessionEditScope
{
    /// <summary>This occurrence only. The default, and the only option for a one-off.</summary>
    ThisOne,

    /// <summary>This occurrence and every later one of the same series.</summary>
    ThisAndFollowing
}
