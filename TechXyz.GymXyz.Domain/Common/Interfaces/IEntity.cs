namespace TechXyz.GymXyz.Domain.Common.Interfaces;

public interface IEntity<T>
{
    T Id { get; set; }
}