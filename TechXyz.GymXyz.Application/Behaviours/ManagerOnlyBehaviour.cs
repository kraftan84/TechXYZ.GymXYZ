using MediatR;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Behaviours;

/// <summary>
/// Refuses every caller but a manager on a command marked
/// <see cref="IManagerOnly"/>, before the handler runs.
/// <para>
/// Runs ahead of the handler's own validation on purpose: a coach must not learn
/// from the error message whether the row they aimed at exists, nor which of its
/// fields would have been wrong.
/// </para>
/// </summary>
public sealed class ManagerOnlyBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUserService _currentUser;

    public ManagerOnlyBehaviour(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is IManagerOnly)
        {
            ManagerOnly.Require(_currentUser);
        }

        return next(cancellationToken);
    }
}
