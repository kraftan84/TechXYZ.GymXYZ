namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// Field names as the user sees them. Without these, FluentValidation builds its
/// message from the C# property name and a French sentence comes back carrying
/// "Type Label".
/// </summary>
public static class LocationFieldNames
{
    public const string Id = "L'identifiant";
    public const string Name = "Le nom du lieu";
    public const string Kind = "La nature du lieu";
    public const string TypeLabel = "Le type";
    public const string IconKey = "L'icône";
    public const string Tone = "La couleur";
    public const string Capacity = "La capacité";
    public const string AreaSqm = "La surface";
    public const string Floor = "L'étage";
    public const string Note = "La description";
    public const string Site = "Le site";
    public const string Address = "L'adresse";
    public const string FallbackLocation = "Le lieu de repli";
    public const string Equipment = "L'équipement";
}
