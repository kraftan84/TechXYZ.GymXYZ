using Microsoft.Extensions.DependencyInjection;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXyz.GymXyz.Persistence.Data;

public static class DbInitializer
{
    public static void Initialize(IServiceProvider serviceProvider, GymDbContext dbContext)
    {
        if (dbContext.Gyms.Any())
            return;

        var overrideUser = serviceProvider.GetRequiredService<ICurrentUserOverride>();

        using (overrideUser.UseTechnicalUser("DbInitializer"))
        {
            // Gym
            var teamTrainers = new Gym("Team Trainer's");

            dbContext.Gyms.Add(teamTrainers);
            dbContext.SaveChanges();

            // Location
            var mainLocation = new Location("Salle Allinges");
            mainLocation.Address = new Address
            {
                Street = "289 Route des Blaves",
                ZipCode = "74200",
                City = "Allinges",
                Country = "France"
            };

            mainLocation.AddRoom(new Room("Coaching"));
            mainLocation.AddRoom(new Room("RPM"));
            mainLocation.AddRoom(new Room("Flexibilité"));

            teamTrainers.AddLocation(mainLocation);
            dbContext.SaveChanges();

            // Coaches
            var aurelie = new Coach("Aurelie", "Siquier");
            teamTrainers.AddCoach(aurelie);
            
            var marine = new Coach("Marine", "Debord");
            teamTrainers.AddCoach(marine);
            
            var najate = new Coach("Najate", "Amzil");
            teamTrainers.AddCoach(najate);

            dbContext.SaveChanges();

            // Members
            var yaya = new Member("Laetitia", "Moriceau");
            teamTrainers.AddMember(yaya);
            
            dbContext.SaveChanges();
        }
    }
}