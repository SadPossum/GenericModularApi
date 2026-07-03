namespace Shared.Administration;

public interface IAdminActorContextAccessor : IAdminActorContext
{
    void SetActor(AdminActor actor);
}
