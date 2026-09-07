using System.Reflection;
using System.Text.Json;
using Tcc.Presentation.Contracts.Theme;

namespace Tcc.Architecture.Tests;

public sealed class PhaseTwoPublicApiGuardTests
{
    private static readonly string[] ForbiddenThemeApiMethods =
    [
        "placeOrder",
        "cancelOrder",
        "modifyOrder",
        "closePosition",
        "executeTrade",
        "writeConnectorData",
        "mutateTrade",
        "mutateRiskProfile",
        "mutatePermission",
        "overrideApproval",
        "appendAudit",
        "modifyAudit",
        "disableAccessibility",
        "readSecret",
        "readCredential",
        "openNetworkSocket",
        "writeArbitraryFile",
    ];

    private static readonly string[] AllowedCapabilities =
    [
        "presentation.tokens",
        "presentation.layout",
        "presentation.components",
        "presentation.icons",
        "presentation.copy.noncritical",
        "presentation.motion",
        "presentation.parallax",
        "presentation.cursor.showcase_limited",
        "presentation.audio.ui",
        "presentation.audio.bgm",
        "presentation.audio.ambient",
        "presentation.loading_empty_error",
        "presentation.time_of_day_scene",
        "presentation.trading_state_overlay",
        "presentation.workspace_appearance",
        "presentation.floating_workspace_appearance",
        "presentation.personalization",
    ];

    [Fact]
    public void ThemePublicInterfacesExposeNoBusinessMutationOrExecutionMethods()
    {
        string[] actualMethodNames = typeof(IThemePackage).Assembly
            .GetExportedTypes()
            .Where(type => type.IsInterface)
            .SelectMany(type => type.GetMethods())
            .Select(method => method.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.DoesNotContain(
            actualMethodNames,
            method => ForbiddenThemeApiMethods.Contains(method, StringComparer.OrdinalIgnoreCase));

        string[] prohibitedFragments =
        [
            "OrderPlacement",
            "OrderCancellation",
            "OrderModification",
            "OrderExecution",
            "BrokerExecution",
            "ConnectorWrite",
            "RiskMutation",
            "PermissionMutation",
            "AuditMutation",
            "CredentialRead",
        ];

        Assert.DoesNotContain(
            actualMethodNames,
            method => prohibitedFragments.Any(
                fragment => method.Contains(fragment, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void ManifestCapabilitySchemaIsPresentationOnlyAndFailClosed()
    {
        using JsonDocument schema = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            RepositoryPaths.ThemeContracts,
            "schemas",
            "ThemeManifest.schema.json")));
        string[] capabilityValues = schema.RootElement
            .GetProperty("properties")
            .GetProperty("capabilities")
            .GetProperty("items")
            .GetProperty("enum")
            .EnumerateArray()
            .Select(value => value.GetString()!)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(AllowedCapabilities.Order(StringComparer.Ordinal), capabilityValues);
        Assert.DoesNotContain(capabilityValues, capability => capability.Contains("domain", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(capabilityValues, capability => capability.Contains("risk", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(capabilityValues, capability => capability.Contains("permission", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(capabilityValues, capability => capability.Contains("audit", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(capabilityValues, capability => capability.Contains("connector", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(capabilityValues, capability => capability.Contains("order", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ConnectorContractRemainsStrictlyReadOnlyWithoutExecutionSurface()
    {
        using JsonDocument uxContract = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            RepositoryPaths.ThemeContracts,
            "ux-contract.v1.1.json")));

        JsonElement connectorModule = uxContract.RootElement
            .GetProperty("modules")
            .EnumerateArray()
            .Single(module => module.GetProperty("id").GetString() == "MOD-EXT-READONLY-CONNECTORS");
        JsonElement connectorFlow = uxContract.RootElement
            .GetProperty("flows")
            .EnumerateArray()
            .Single(flow => flow.GetProperty("id").GetString() == "FLOW-AE-READONLY-CONNECTOR");
        JsonElement connectorSurface = uxContract.RootElement
            .GetProperty("surfaces")
            .EnumerateArray()
            .Single(surface => surface.GetProperty("id").GetString() == "UX-CONN-001");

        Assert.Contains("read-only", connectorModule.GetProperty("semantics").GetString(), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("read-only", connectorFlow.GetProperty("semantics").GetString(), StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            connectorSurface.GetProperty("safety_constraints").EnumerateArray(),
            constraint => constraint.GetString()!.Contains("No write or execution", StringComparison.OrdinalIgnoreCase));

        Assert.DoesNotContain(
            typeof(IThemePackage).Assembly.GetExportedTypes().SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)),
            method => method.Name.Contains("Connector", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AudioAndMotionContractsCannotReplaceSafetySemantics()
    {
        using JsonDocument audio = JsonDocument.Parse(File.ReadAllText(Path.Combine(RepositoryPaths.ThemeContracts, "audio-contract.v1.json")));
        using JsonDocument motion = JsonDocument.Parse(File.ReadAllText(Path.Combine(RepositoryPaths.ThemeContracts, "motion-contract.v1.json")));

        Assert.True(audio.RootElement.GetProperty("supplementary_only").GetBoolean());
        Assert.True(audio.RootElement.GetProperty("critical_alert_visual_text_redundancy_required").GetBoolean());
        Assert.True(audio.RootElement.GetProperty("loss_celebration_forbidden").GetBoolean());
        Assert.True(motion.RootElement.GetProperty("motion_is_supplementary").GetBoolean());
        Assert.True(motion.RootElement.GetProperty("critical_alert_preempts_motion").GetBoolean());
        Assert.True(motion.RootElement.GetProperty("loss_celebration_forbidden").GetBoolean());
    }
}
