using TechXyz.GymXyz.Domain.Common;

namespace TechXyz.GymXyz.Domain.Entities;

public class Address : EntityBase<int>
{
    public string Street { get; set; }
    public string ZipCode { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
}