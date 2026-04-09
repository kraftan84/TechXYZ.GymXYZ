using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateLessonThemeCommandHandler : IRequestHandler<UpdateLessonThemeCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<UpdateLessonThemeCommand> _validator;

    public UpdateLessonThemeCommandHandler(IGymDbContext dbContext, IValidator<UpdateLessonThemeCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<bool> Handle(UpdateLessonThemeCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var theme = await _dbContext.LessonThemes
            .FirstOrDefaultAsync(candidate => candidate.Id == request.Id && candidate.IsActive, cancellationToken);
        if (theme is null)
        {
            return false;
        }

        theme.Name = request.Name.Trim();
        theme.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
