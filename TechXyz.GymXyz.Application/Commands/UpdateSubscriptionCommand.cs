using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateSubscriptionCommand : IRequest<bool>
{
    public UpdateSubscriptionCommand(int id, DateOnly startDate, DateOnly endDate, int numberOfLessons)
    {
        Id = id;
        StartDate = startDate;
        EndDate = endDate;
        NumberOfLessons = numberOfLessons;
    }

    public int Id { get; }
    public DateOnly StartDate { get; }
    public DateOnly EndDate { get; }
    public int NumberOfLessons { get; }
}
