using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Common;

public static class AddressHelper
{
    public static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public static Address? BuildOptionalAddress(string? street, string? zipCode, string? city, string? country)
    {
        var normalizedStreet = NormalizeOptional(street);
        var normalizedZipCode = NormalizeOptional(zipCode);
        var normalizedCity = NormalizeOptional(city);
        var normalizedCountry = NormalizeOptional(country);

        if (normalizedStreet is null && normalizedZipCode is null && normalizedCity is null && normalizedCountry is null)
        {
            return null;
        }

        return new Address
        {
            Street = normalizedStreet ?? string.Empty,
            ZipCode = normalizedZipCode ?? string.Empty,
            City = normalizedCity ?? string.Empty,
            Country = normalizedCountry ?? string.Empty
        };
    }

    public static Address? Apply(Address? currentAddress, Address? updatedAddress)
    {
        if (updatedAddress is null)
        {
            return null;
        }

        if (currentAddress is null)
        {
            return updatedAddress;
        }

        currentAddress.Street = updatedAddress.Street;
        currentAddress.ZipCode = updatedAddress.ZipCode;
        currentAddress.City = updatedAddress.City;
        currentAddress.Country = updatedAddress.Country;

        return currentAddress;
    }
}
