using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteLessonThemeCommandHandler : IRequestHandler<DeleteLessonThemeCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<DeleteLessonThemeCommand> _validator;

    public DeleteLessonThemeCommandHandler(IGymDbContext dbContext, IValidator<DeleteLessonThemeCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<bool> Handle(DeleteLessonThemeCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var theme = await _dbContext.LessonThemes
            .FirstOrDefaultAsync(candidate => candidate.Id == request.Id && candidate.IsActive, cancellationToken);
        if (theme is null)
        {
            return false;
        }

        var linkedLessons = await _dbContext.Lessons
            .Where(lesson => lesson.IsActive && lesson.Theme != null && lesson.Theme.Id == request.Id)
            .ToListAsync(cancellationToken);

        foreach (var lesson in linkedLessons)
        {
            lesson.Theme = null;
        }

        theme.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
