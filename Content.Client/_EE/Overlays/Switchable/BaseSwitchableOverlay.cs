using System.Numerics;
using Content.Shared._EE.Overlays.Switchable;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._EE.Overlays.Switchable;

public sealed class BaseSwitchableOverlay<TComp> : Overlay where TComp : SwitchableOverlayComponent
{
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IGameTiming _timing = default!;


    public override bool RequestScreenTexture => true;
    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    private readonly SharedPointLightSystem _light;
    private readonly ShaderInstance _shader;
    private readonly TransformSystem _transform;
    public TComp? Comp = null;
    public float LightRadius;
    public bool IsActive = true;

    private EntityUid? _lightEntity;

    public BaseSwitchableOverlay()
    {
        IoCManager.InjectDependencies(this);
        _shader = _prototype.Index<ShaderPrototype>("NightVision").InstanceUnique();
        _transform = _entity.System<TransformSystem>();
        _light = _entity.System<SharedPointLightSystem>();

        ZIndex = -1;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture is null || Comp is null || !IsActive)
            return;

        var player = _player.LocalEntity;

        if (!_entity.TryGetComponent(player, out TransformComponent? playerXform))
            return;

        _shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _shader.SetParameter("tint", Comp.Tint);
        _shader.SetParameter("luminance_threshold", Comp.Strength);
        _shader.SetParameter("noise_amount", Comp.Noise);

        var worldHandle = args.WorldHandle;

        var accumulator = Math.Clamp(Comp.PulseAccumulator, 0f, Comp.PulseTime);
        var alpha = Comp.PulseTime <= 0f ? 0.6f : float.Lerp(1f, 0f, accumulator / Comp.PulseTime);

        // Add a clientside light to ensure there is always some visibility - Imp edit start
        if (LightRadius > 0)
        {
            _lightEntity ??= _entity.SpawnAttachedTo(null, playerXform.Coordinates);
            _transform.SetParent(_lightEntity.Value, player.Value);
            var light = _entity.EnsureComponent<PointLightComponent>(_lightEntity.Value);
            _light.SetRadius(_lightEntity.Value, LightRadius, light);
            _light.SetEnergy(_lightEntity.Value, alpha, light);
            _light.SetColor(_lightEntity.Value, Comp.Color, light);
        }
        else
            ResetLight();

        worldHandle.SetTransform(Matrix3x2.Identity);
        worldHandle.UseShader(_shader);
        worldHandle.DrawRect(args.WorldBounds, Comp.Color.WithAlpha(alpha));
        worldHandle.UseShader(null);
    }

    public void ResetLight(bool checkFirstTimePredicted = true)
    {
        if (_lightEntity == null || checkFirstTimePredicted && !_timing.IsFirstTimePredicted)
            return;

        _entity.DeleteEntity(_lightEntity);
        _lightEntity = null;
    }
}
