namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// Field names as the user sees them. Without these, FluentValidation builds
/// its message from the C# property name and a French sentence comes back
/// carrying "First Name".
/// </summary>
public static class MemberFieldNames
{
    public const string Id = "L'identifiant";
    public const string FirstName = "Le prénom";
    public const string LastName = "Le nom";
    public const string Email = "L'adresse e-mail";
    public const string Phone = "Le téléphone";
    public const string Street = "La rue";
    public const string ZipCode = "Le code postal";
    public const string City = "La ville";
    public const string Country = "Le pays";
    public const string Notes = "La note interne";
    public const string BirthDate = "La date de naissance";
    public const string JoinedOn = "La date d'inscription";
}
