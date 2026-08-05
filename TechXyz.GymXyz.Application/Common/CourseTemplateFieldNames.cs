namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// Field names as the user sees them. Without these, FluentValidation builds
/// its message from the C# property name and a French sentence comes back
/// carrying "Duration Minutes".
/// </summary>
public static class CourseTemplateFieldNames
{
    public const string Id = "L'identifiant";
    public const string Name = "Le nom du cours";
    public const string Discipline = "La discipline";
    public const string IconKey = "L'icône";
    public const string DurationMinutes = "La durée";
    public const string Capacity = "La capacité";
    public const string DefaultLocation = "Le studio";
    public const string Level = "Le niveau";
    public const string Intensity = "L'intensité";
    public const string Price = "Le tarif";
    public const string Description = "La description";
    public const string Coaches = "Les coachs";
}
