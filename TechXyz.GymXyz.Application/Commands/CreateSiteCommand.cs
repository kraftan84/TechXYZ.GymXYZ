using MediatR;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateSiteCommand : IRequest<int>, IManagerOnly
{
    public CreateSiteCommand(
        string name,
        string street,
        string zipCode,
        string city,
        string country)
    {
        Name = name;
        Street = street;
        ZipCode = zipCode;
        City = city;
        Country = country;
    }

    public string Name { get; }
    public string Street { get; }
    public string ZipCode { get; }
    public string City { get; }
    public string Country { get; }
}
