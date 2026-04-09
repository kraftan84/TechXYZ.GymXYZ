using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateLessonThemeCommand : IRequest<int>
{
    public CreateLessonThemeCommand(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    public string Name { get; }
    public string? Description { get; }
}
