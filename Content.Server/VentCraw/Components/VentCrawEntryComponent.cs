namespace Content.Server.VentCraw.Components
{
    [RegisterComponent]
    [Access(typeof(VentCrawTubeSystem))]
    public sealed partial class VentCrawEntryComponent : Component
    {
        public const string HolderPrototypeId = "VentCrawHolder";
    }
}
