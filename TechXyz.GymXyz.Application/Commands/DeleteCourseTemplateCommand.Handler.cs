using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteCourseTemplateCommandHandler : IRequestHandler<DeleteCourseTemplateCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<DeleteCourseTemplateCommand> _validator;

    public DeleteCourseTemplateCommandHandler(
        IGymDbContext dbContext,
        IValidator<DeleteCourseTemplateCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<bool> Handle(DeleteCourseTemplateCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var template = await _dbContext.CourseTemplates
            .FirstOrDefaultAsync(
                candidate => candidate.Id == request.Id && candidate.IsActive,
                cancellationToken);
        if (template is null)
        {
            return false;
        }

        template.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
