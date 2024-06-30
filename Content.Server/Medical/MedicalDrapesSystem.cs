using Content.Server.Medical.Components;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.MedicalScanner;
using Content.Shared.Mobs.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;
using Content.Shared.Medical;
using Content.Shared.Medical.Drapes;
namespace Content.Server.Medical;

public sealed class MedicalDrapesSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MedicalDrapesComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<MedicalDrapesComponent, MedicalDrapesDoAfterEvent>(OnDoAfter);
    }

    private void OnAfterInteract(Entity<MedicalDrapesComponent> uid, ref AfterInteractEvent args)
    {
        if (args.Target == null || !args.CanReach || !HasComp<MobStateComponent>(args.Target))
            return;

        _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, uid.Comp.UseDelay, new HealthAnalyzerDoAfterEvent(), uid, target: args.Target, used: uid)
        {
            NeedHand = true,
            BreakOnMove = true
        });
    }

    private void OnDoAfter(Entity<MedicalDrapesComponent> uid, ref MedicalDrapesDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target == null)
            return;
        OpenUserInterface(args.User, uid);
        args.Handled = true;
    }

    private void OpenUserInterface(EntityUid user, EntityUid analyzer)
    {
        if (!_uiSystem.HasUi(analyzer, MedicalDrapesUiKey.Key))
            return;

        _uiSystem.OpenUi(analyzer, MedicalDrapesUiKey.Key, user);
    }
}
