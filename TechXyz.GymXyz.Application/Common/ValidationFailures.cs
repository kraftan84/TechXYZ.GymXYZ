using FluentValidation;
using FluentValidation.Results;

namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// How a handler refuses a write in a way the user actually reads.
/// </summary>
public static class ValidationFailures
{
    /// <summary>
    /// Refuses the write, naming the field and saying what is wrong.
    /// <para>
    /// <c>new ValidationException(text)</c> fills the exception's own message but
    /// leaves <c>Errors</c> empty, and the toast is built from <c>Errors</c> — so
    /// a plain-text throw reaches the screen as "Validation invalide". Rules
    /// exist to say which sheet is closed or which room is taken, so they are
    /// raised as a failure, not as a message.
    /// </para>
    /// </summary>
    public static ValidationException Refuse(string field, string message) =>
        new([new ValidationFailure(field, message)]);
}
