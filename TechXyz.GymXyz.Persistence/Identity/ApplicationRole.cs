using Microsoft.AspNetCore.Identity;

namespace TechXyz.GymXyz.Persistence.Identity;

public class ApplicationRole : IdentityRole
{
    public ApplicationRole()
    {
    }

    public ApplicationRole(string roleName) : base(roleName)
    {
    }
}
