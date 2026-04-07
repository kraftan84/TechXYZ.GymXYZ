using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateLocationCommand : IRequest<bool>
{
    public UpdateLocationCommand(
        int id,
        string name,
        string street,
        string zipCode,
        string city,
        string country)
    {
        Id = id;
        Name = name;
        Street = street;
        ZipCode = zipCode;
        City = city;
        Country = country;
    }

    public int Id { get; }
    public string Name { get; }
    public string Street { get; }
    public string ZipCode { get; }
    public string City { get; }
    public string Country { get; }
}
