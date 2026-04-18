using Content.Shared.Actions.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Impstation.PDA;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AltPdaAccessComponent : Component
{

    /// <summary>
    /// The action id for toggling the PDA screen.
    /// </summary>
    [DataField]
    public EntProtoId<InstantActionComponent> AltPdaAccessAction = "ActionAccessPDA";

    [DataField, AutoNetworkedField]
    public EntityUid? AltPdaAccessEntity;

}
