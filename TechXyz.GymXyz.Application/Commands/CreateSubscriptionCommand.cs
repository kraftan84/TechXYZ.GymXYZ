using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateSubscriptionCommand : IRequest<int>
{
    public CreateSubscriptionCommand(int memberId, DateOnly? startDate, DateOnly? endDate, int numberOfLessons)
    {
        var effectiveStartDate = startDate ?? DateOnly.FromDateTime(DateTime.Today);

        MemberId = memberId;
        StartDate = effectiveStartDate;
        EndDate = endDate ?? effectiveStartDate.AddYears(1);
        NumberOfLessons = numberOfLessons;
    }

    public int MemberId { get; }
    public DateOnly StartDate { get; }
    public DateOnly EndDate { get; }
    public int NumberOfLessons { get; }
}
