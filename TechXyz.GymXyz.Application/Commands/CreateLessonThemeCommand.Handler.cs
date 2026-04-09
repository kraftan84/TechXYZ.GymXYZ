using FluentValidation;
using MediatR;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateLessonThemeCommandHandler : IRequestHandler<CreateLessonThemeCommand, int>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<CreateLessonThemeCommand> _validator;

    public CreateLessonThemeCommandHandler(IGymDbContext dbContext, IValidator<CreateLessonThemeCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<int> Handle(CreateLessonThemeCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var theme = new LessonTheme(request.Name.Trim())
        {
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim()
        };

        _dbContext.LessonThemes.Add(theme);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return theme.Id;
    }
}
