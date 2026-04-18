using Content.Shared.Actions;
using Content.Shared.Clothing.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Inventory;
using Content.Shared.PDA;
using Robust.Shared.Player;

namespace Content.Shared._Impstation.PDA;

public sealed class AltPdaAccessSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AltPdaAccessComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<AltPdaAccessComponent, AltAccessPdaScreenEvent>(OnTogglePdaScreen);
    }

    private void OnGetActions(Entity<AltPdaAccessComponent> ent, ref GetItemActionsEvent args)
    {
        if (!TryComp<HandsComponent>(ent.Owner, out _))
            return;

        if (_inventorySystem.InSlotWithFlags(ent.Owner, SlotFlags.IDCARD))
        {
            args.AddAction(ent.Comp.AltPdaAccessEntity);
            Dirty(ent.Owner, ent.Comp);
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
