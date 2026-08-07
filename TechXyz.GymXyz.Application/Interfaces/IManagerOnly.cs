namespace TechXyz.GymXyz.Application.Interfaces;

/// <summary>
/// Marks a command only a gym manager — or a platform admin inside a customer —
/// may run. <c>ManagerOnlyBehaviour</c> refuses every other caller before the
/// handler is reached.
/// <para>
/// Declared on the command rather than checked inside each handler because the
/// perimeter is about twenty commands wide: repeating the check twenty times
/// means the twenty-first is the one somebody forgets, and a forgotten check
/// looks exactly like a command that is deliberately open. As a marker it is
/// greppable, and <c>ManagerOnlyPerimeterTests</c> pins the expected list so a
/// new command has to state which side of the line it is on.
/// </para>
/// </summary>
public interface IManagerOnly;
