using Content.Shared.Actions;
using Content.Shared.Clothing.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Inventory;
using Content.Shared.PDA;
using Robust.Shared.Player;

namespace Content.Shared._Impstation.PDA;

public sealed class AltPdaAccessSystem : EntitySystem
{
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AltPdaAccessComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<AltPdaAccessComponent, AltAccessPdaScreenEvent>(OnTogglePdaScreen);
        SubscribeLocalEvent<AltPdaAccessComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<AltPdaAccessComponent> ent, ref MapInitEvent args)
    {
        var (uid, comp) = ent;
        _actionContainer.EnsureAction(uid, ref comp.AltPdaAccessEntity, comp.AltPdaAccessAction);
        Dirty(uid, comp);
    }

    private void OnGetActions(Entity<AltPdaAccessComponent> ent, ref GetItemActionsEvent args)
    {
        var (uid, comp) = ent;

        if (!TryComp<HandsComponent>(uid, out _))
            return;

        if (_inventorySystem.InSlotWithFlags(uid, SlotFlags.IDCARD))
        {
            args.AddAction(comp.AltPdaAccessEntity);
            Dirty(uid, comp);
        }
    }

    private void OnTogglePdaScreen(Entity<AltPdaAccessComponent> ent, ref AltAccessPdaScreenEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        // TODO Inventory / Clothing
        // Add an easier way to get the entity that is wearing clothing in a valid slot.
        EntityUid? wearer = null;
        if (TryComp(ent, out ClothingComponent? clothing)
            && clothing.InSlotFlag is { } slotFlag
            && clothing.Slots.HasFlag(slotFlag))
        {
            wearer = Transform(ent).ParentUid;
        }

        if (wearer != null && TryComp<ActorComponent>(wearer, out var actor))
        {
            _userInterface.TryToggleUi(ent.Owner, PdaUiKey.Key, actor.PlayerSession);
        }
    }
}

public sealed partial class AltAccessPdaScreenEvent : InstantActionEvent;
