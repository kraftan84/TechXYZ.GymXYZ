namespace TechXyz.GymXyz.Application.Interfaces;

public interface ICurrentUserOverride
{
    IDisposable UseTechnicalUser(string userName = "technical");
}