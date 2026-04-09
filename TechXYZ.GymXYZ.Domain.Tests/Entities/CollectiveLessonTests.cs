using Bogus;
using Shouldly;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Domain.Tests;

public class CollectiveLessonTests
{
    [Fact]
    public void NumberOfParticipants_ShouldReturnZero_WhenParticipantsIsNull()
    {
        var lesson = new CollectiveLesson();

        lesson.NumberOfParticipants.ShouldBe(0);
    }

    [Fact]
    public void NumberOfParticipants_ShouldReturnCollectionCount_WhenParticipantsIsSet()
    {
        var faker = Faker();
        var lesson = new CollectiveLesson
        {
            Participants = new List<Member>
            {
                new(faker.Name.FirstName(), faker.Name.LastName()),
                new(faker.Name.FirstName(), faker.Name.LastName())
            }
        };

        lesson.NumberOfParticipants.ShouldBe(2);
    }

    private static Faker Faker() => new("en");
}
