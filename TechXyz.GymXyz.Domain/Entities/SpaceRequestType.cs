namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// What the applicant runs. Chosen at step 1, and the answer reshapes the form
/// after it: a gym gives an address and a member count, a coach gives an area and
/// a client count.
/// </summary>
public enum SpaceRequestType
{
    Gym,
    Coach
}
