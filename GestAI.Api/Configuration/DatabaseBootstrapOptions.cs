namespace GestAI.Api.Configuration;

public sealed class DatabaseBootstrapOptions
{
    public const string SectionName = "DatabaseBootstrap";

    public bool ApplyMigrations { get; set; }
    public bool RunDemoSeed { get; set; }
    public DemoSeedOptions DemoSeed { get; set; } = new();

    public sealed class DemoSeedOptions
    {
        public string? AdminEmail { get; set; }
        public string? AdminPassword { get; set; }
        public string PropertyName { get; set; } = "Alma de Lago (Demo)";
        public string[] UnitNames { get; set; } = ["Cabaña 1", "Cabaña 2", "Cabaña 3"];
    }
}
