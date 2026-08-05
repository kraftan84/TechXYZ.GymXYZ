using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

/// <summary>
/// Everything the Présences screen shows before a sheet is opened.
/// <para>
/// It takes no day filter. The prototype declares one — <c>useState('today')</c>
/// — and then never renders a control for it nor reads the value; what it
/// actually draws is two fixed sections, "À pointer" and "Séances récentes".
/// Building the filter would have meant inventing a control the mockup does not
/// have.
/// </para>
/// </summary>
public sealed class GetAttendanceOverviewQuery : IRequest<AttendanceOverviewDto>
{
}
