using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateSubscriptionCommand : IRequest<bool>
{
    public UpdateSubscriptionCommand(int id, DateOnly startDate, DateOnly endDate, int numberOfSessions)
    {
        Id = id;
        StartDate = startDate;
        EndDate = endDate;
        NumberOfSessions = numberOfSessions;
    }

    public int Id { get; }
    public DateOnly StartDate { get; }
    public DateOnly EndDate { get; }
    public int NumberOfSessions { get; }
}
