namespace TechXyz.GymXyz.WebApp.Components.Shared;

public interface IPersonEditModel
{
    string FirstName { get; set; }
    string LastName { get; set; }
    string? Email { get; set; }
    string? Phone { get; set; }
    string Street { get; set; }
    string ZipCode { get; set; }
    string City { get; set; }
    string Country { get; set; }
}
