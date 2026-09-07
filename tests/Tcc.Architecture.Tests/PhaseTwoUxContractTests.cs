using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Tcc.Architecture.Tests;

public sealed class PhaseTwoUxContractTests
{
    private static readonly IReadOnlyDictionary<string, string[]> ExpectedIds =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["domains"] =
            [
                "D-HOME","D-PLAN","D-RISK","D-POSITION","D-CLOSE","D-REVIEW","D-CALENDAR","D-TEAM",
                "D-COLLAB","D-AI","D-DATA","D-SETTINGS","D-THEME","D-EXT","D-UTILITY",
            ],
            ["workspaces"] =
            [
                "WS-PERSONAL","WS-TEAM","WS-DAILY","WS-PREMARKET","WS-TRADING","WS-REVIEW","WS-COLLAB",
                "WS-FLOAT","WS-CUSTOM-*",
            ],
            ["modules"] =
            [
                "MOD-HOME-COMMAND","MOD-RISK-PERMISSION","MOD-PLAN-WORKBENCH","MOD-PLAN-CHECKLIST-GATES",
                "MOD-PLAN-STRATEGY-TEMPLATES","MOD-RISK-PROFILES","MOD-RISK-ACCOUNTS-GROUPS",
                "MOD-POSITION-MANAGEMENT","MOD-CLOSING","MOD-REVIEW-TIMELINE","MOD-REVIEW-TRADE-HISTORY",
                "MOD-EVIDENCE-ATTACHMENTS-MARKUP","MOD-CALENDAR-PL","MOD-ANALYTICS-BUILDER",
                "MOD-REVIEW-REPORT-LIBRARY","MOD-REVIEW-MENTAL-STATE","MOD-AI-IMPROVEMENT-COACH",
                "MOD-AI-GPT-MANUAL","MOD-COLLAB-PERSONAL-TEAM","MOD-COLLAB-ROOMS",
                "MOD-UTILITY-ACTIVITY-FEED","MOD-UTILITY-COMMAND-PALETTE","MOD-UTILITY-NOTIFICATIONS",
                "MOD-UTILITY-FLOATING-WORKSPACE","MOD-THEME-MANAGEMENT","MOD-THEME-PERSONALIZATION",
                "MOD-SETTINGS-CENTER","MOD-DATA-RECOVERY","MOD-DATA-MIGRATION","MOD-EXT-PLUGIN-MANAGEMENT",
                "MOD-EXT-READONLY-CONNECTORS","MOD-SETTINGS-SECURITY-DEVICES","MOD-SETTINGS-ACCESSIBILITY",
                "MOD-ONBOARDING","MOD-AUTH-SECURITY","MOD-DATA-ARCHIVE-SOFT-DELETE",
                "MOD-LIFECYCLE-ORDER-TRACKING","MOD-LIFECYCLE-REPORTS","MOD-REVIEW-MILESTONES",
                "MOD-DATA-EXPORT-PORTABLE-ARCHIVE","MOD-SETTINGS-UPDATES",
            ],
            ["zones"] =
            [
                "ZONE-SAFETY-CORE","ZONE-CONTEXT-HEADER","ZONE-STATUS-STRIP","ZONE-PRIMARY-ACTION",
                "ZONE-EVIDENCE","ZONE-RISK","ZONE-DECISION","ZONE-TIMELINE","ZONE-CHECKLIST","ZONE-APPROVAL",
                "ZONE-COLLABORATION","ZONE-ATTACHMENTS","ZONE-AUDIT-HISTORY","ZONE-SECONDARY-UTILITY",
                "ZONE-RECOVERY","ZONE-PRIVACY","ZONE-CONFIRMATION",
            ],
            ["states"] =
            [
                "STATE-NORMAL","STATE-LOADING","STATE-EMPTY","STATE-SUCCESS","STATE-WARNING","STATE-BLOCKED",
                "STATE-ERROR","STATE-OFFLINE","STATE-STALE-DATA","STATE-SYNCING","STATE-CONFLICT",
                "STATE-PERMISSION-DENIED","STATE-APPROVAL-PENDING","STATE-APPROVAL-REJECTED",
                "STATE-RECOVERY-AVAILABLE","STATE-SAFE-MODE","STATE-DEGRADED-PERFORMANCE",
                "STATE-REDUCED-MOTION","STATE-REDUCED-TRANSPARENCY",
            ],
            ["flows"] =
            [
                "FLOW-A-PREMARKET","FLOW-B-CREATE-PLAN","FLOW-C-VALIDATE-PLAN","FLOW-D-PERMISSION-EVAL",
                "FLOW-E-HARD-BLOCK-RECOVERY","FLOW-F-FORMALIZE-OPEN","FLOW-G-MANAGE-POSITION",
                "FLOW-H-RISK-SENSITIVE-UPDATE","FLOW-I-CLOSE-TRADE","FLOW-J-QUICK-CLOSE",
                "FLOW-K-DELAYED-DEEP-REVIEW","FLOW-L-DECISION-TIMELINE","FLOW-M-CALENDAR-INVESTIGATION",
                "FLOW-N-STRATEGY-TEMPLATE","FLOW-O-RISK-PROFILE","FLOW-P-FOLLOW-SHARED-TRADE",
                "FLOW-Q-TEAM-COLLAB-PLAN","FLOW-R-TEAM-APPROVAL","FLOW-S-TRADE-FIELD-CONFLICT",
                "FLOW-T-GPT-EXPORT","FLOW-U-GPT-IMPORT","FLOW-V-CRASH-RECOVERY","FLOW-W-MIGRATION",
                "FLOW-X-THEME-SWITCH","FLOW-Y-THEME-ROLLBACK","FLOW-Z-PLUGIN-PERMISSION",
                "FLOW-AA-ONBOARDING","FLOW-AB-AUTH-SECURITY","FLOW-AC-ARCHIVE-SOFT-DELETE",
                "FLOW-AD-LIFECYCLE-REPORTS","FLOW-AE-READONLY-CONNECTOR",
                "FLOW-AF-COMMAND-PALETTE-DIRECT-ACTION",
            ],
            ["surfaces"] =
            [
                "UX-HOME-001","UX-HOME-002","UX-ONB-001","UX-ONB-002","UX-ONB-003",
                "UX-RISK-001","UX-RISK-002","UX-RISK-003","UX-RISK-004","UX-RISK-005",
                "UX-PLAN-001","UX-PLAN-002","UX-PLAN-003","UX-PLAN-004","UX-PLAN-005",
                "UX-POS-001","UX-POS-002","UX-POS-003","UX-CLOSE-001","UX-CLOSE-002","UX-CLOSE-003",
                "UX-ORDER-001","UX-CLOSE-004","UX-REVIEW-001","UX-REVIEW-002","UX-REVIEW-003",
                "UX-REVIEW-004","UX-REVIEW-005","UX-REVIEW-006","UX-MILE-001","UX-CAL-001","UX-CAL-002",
                "UX-CAL-003","UX-ANALYTICS-001","UX-ANALYTICS-002","UX-TEAM-001","UX-TEAM-002",
                "UX-TEAM-003","UX-TEAM-004","UX-ROOM-001","UX-ROOM-002","UX-COLLAB-001",
                "UX-AI-001","UX-AI-002","UX-AI-003","UX-AI-004","UX-DATA-001","UX-ARCH-001",
                "UX-DATA-002","UX-REC-001","UX-MIG-001","UX-MIG-002","UX-THEME-001","UX-THEME-002",
                "UX-THEME-003","UX-SET-001","UX-SET-002","UX-SET-003","UX-SET-004","UX-SET-005",
                "UX-SET-006","UX-AUTH-001","UX-AUTH-002","UX-AUTH-003","UX-EXT-001","UX-EXT-002",
                "UX-CONN-001","UX-NOTIF-001","UX-CMD-001","UX-FLOAT-001","UX-SAFE-001",
            ],
        };

    [Fact]
    public void StableUxContractContainsOnlyApprovedIds()
    {
        using JsonDocument contract = ReadUxContract();

        foreach ((string registry, string[] expectedIds) in ExpectedIds)
        {
            string[] actualIds = contract.RootElement
                .GetProperty(registry)
                .EnumerateArray()
                .Select(item => item.GetProperty("id").GetString()!)
                .Order(StringComparer.Ordinal)
                .ToArray();

            Assert.Equal(expectedIds.Order(StringComparer.Ordinal), actualIds);
        }
    }

    [Fact]
    public void EveryStableUxIdHasProvenanceAndSemantics()
    {
        using JsonDocument contract = ReadUxContract();

        foreach (string registry in ExpectedIds.Keys.Where(key => key != "surfaces"))
        {
            foreach (JsonElement entry in contract.RootElement.GetProperty(registry).EnumerateArray())
            {
                Assert.False(string.IsNullOrWhiteSpace(entry.GetProperty("semantics").GetString()));
                Assert.Contains("APPROVED", entry.GetProperty("source_reference").GetString(), StringComparison.Ordinal);
            }
        }

        foreach (JsonElement surface in contract.RootElement.GetProperty("surfaces").EnumerateArray())
        {
            Assert.NotEmpty(surface.GetProperty("safety_constraints").EnumerateArray());
            Assert.NotEmpty(surface.GetProperty("source_references").EnumerateArray());
            Assert.All(
                surface.GetProperty("source_references").EnumerateArray(),
                source => Assert.Contains("APPROVED", source.GetString(), StringComparison.Ordinal));
        }
    }

    [Fact]
    public void DuplicateUxIdsAreRejectedByActualGeneratorAndTemporaryFixtureIsRemoved()
    {
        string fixtureDirectory = Path.Combine(Path.GetTempPath(), $"tcc-phase2-ux-{Guid.NewGuid():N}");
        string fixturePath = Path.Combine(fixtureDirectory, "Generate-ThemeContracts.Duplicate.ps1");

        try
        {
            Directory.CreateDirectory(fixtureDirectory);
            string generatorPath = Path.Combine(RepositoryPaths.Root, "tools", "phase2", "Generate-ThemeContracts.ps1");
            string generator = File.ReadAllText(generatorPath)
                .Replace(
                    "$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\\..')).Path",
                    $"$repositoryRoot = '{EscapePowerShellLiteral(RepositoryPaths.Root)}'",
                    StringComparison.Ordinal)
                .Replace(
                    "$contractRoot = Join-Path $repositoryRoot 'contracts\\theme'",
                    $"$contractRoot = '{EscapePowerShellLiteral(Path.Combine(fixtureDirectory, "contracts", "theme"))}'",
                    StringComparison.Ordinal)
                .Replace(
                    "D-HOME|Command Center Home|Integrated safety, status, daily priorities, positions, and alerts",
                    "D-HOME|Command Center Home|Integrated safety, status, daily priorities, positions, and alerts\nD-HOME|Duplicate Home|Duplicate",
                    StringComparison.Ordinal);
            File.WriteAllText(fixturePath, generator);

            ProcessStartInfo startInfo = new("pwsh.exe")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            startInfo.ArgumentList.Add("-NoProfile");
            startInfo.ArgumentList.Add("-NonInteractive");
            startInfo.ArgumentList.Add("-File");
            startInfo.ArgumentList.Add(fixturePath);

            using Process process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("Could not start PowerShell for generator negative test.");
            string standardOutput = process.StandardOutput.ReadToEnd();
            string standardError = process.StandardError.ReadToEnd();
            process.WaitForExit();

            Assert.NotEqual(0, process.ExitCode);
            Assert.Contains(
                "Duplicate stable UX IDs: D-HOME",
                standardOutput + standardError,
                StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(fixtureDirectory))
            {
                Directory.Delete(fixtureDirectory, recursive: true);
            }
        }

        Assert.False(Directory.Exists(fixtureDirectory));
    }

    [Fact]
    public void EveryGeneratedPageUsesExactFrozenStateMapping()
    {
        foreach (string pageId in ExpectedIds["surfaces"])
        {
            using JsonDocument contract = JsonDocument.Parse(File.ReadAllText(Path.Combine(
                RepositoryPaths.ThemeContracts,
                "page-contracts",
                $"{pageId}.json")));
            string[] actual = contract.RootElement.GetProperty("supported_states")
                .EnumerateArray()
                .Select(state => state.GetString()!)
                .Order(StringComparer.Ordinal)
                .ToArray();

            Assert.Equal(GetFrozenExpectedStates(pageId).Order(StringComparer.Ordinal), actual);
        }
    }

    private static string EscapePowerShellLiteral(string value) => value.Replace("'", "''", StringComparison.Ordinal);

    private static string[] GetFrozenExpectedStates(string pageId)
    {
        string[] baseStates =
        [
            "STATE-NORMAL", "STATE-LOADING", "STATE-EMPTY", "STATE-SUCCESS",
            "STATE-WARNING", "STATE-BLOCKED", "STATE-ERROR", "STATE-OFFLINE",
        ];
        string[] additional = pageId switch
        {
            "UX-HOME-001" =>
            [
                "STATE-STALE-DATA", "STATE-SYNCING", "STATE-CONFLICT", "STATE-PERMISSION-DENIED",
                "STATE-APPROVAL-PENDING", "STATE-APPROVAL-REJECTED", "STATE-RECOVERY-AVAILABLE",
                "STATE-SAFE-MODE", "STATE-DEGRADED-PERFORMANCE", "STATE-REDUCED-MOTION",
                "STATE-REDUCED-TRANSPARENCY",
            ],
            "UX-REVIEW-006" =>
            [
                "STATE-STALE-DATA", "STATE-SYNCING", "STATE-CONFLICT", "STATE-PERMISSION-DENIED",
                "STATE-APPROVAL-PENDING", "STATE-RECOVERY-AVAILABLE",
            ],
            _ when pageId.StartsWith("UX-HOME-", StringComparison.Ordinal)
                   || pageId.StartsWith("UX-ONB-", StringComparison.Ordinal) =>
            [
                "STATE-STALE-DATA", "STATE-SYNCING", "STATE-PERMISSION-DENIED", "STATE-RECOVERY-AVAILABLE",
                "STATE-DEGRADED-PERFORMANCE", "STATE-REDUCED-MOTION", "STATE-REDUCED-TRANSPARENCY",
            ],
            _ when pageId.StartsWith("UX-RISK-", StringComparison.Ordinal) =>
            [
                "STATE-STALE-DATA", "STATE-SYNCING", "STATE-CONFLICT", "STATE-PERMISSION-DENIED",
                "STATE-APPROVAL-PENDING", "STATE-APPROVAL-REJECTED", "STATE-RECOVERY-AVAILABLE",
            ],
            _ when pageId.StartsWith("UX-PLAN-", StringComparison.Ordinal)
                   || pageId.StartsWith("UX-POS-", StringComparison.Ordinal)
                   || pageId == "UX-ORDER-001"
                   || pageId.StartsWith("UX-CLOSE-", StringComparison.Ordinal) =>
            [
                "STATE-STALE-DATA", "STATE-SYNCING", "STATE-CONFLICT", "STATE-PERMISSION-DENIED",
                "STATE-APPROVAL-PENDING", "STATE-RECOVERY-AVAILABLE",
            ],
            _ when pageId.StartsWith("UX-REVIEW-", StringComparison.Ordinal)
                   || pageId == "UX-MILE-001" =>
            [
                "STATE-STALE-DATA", "STATE-SYNCING", "STATE-CONFLICT", "STATE-PERMISSION-DENIED",
                "STATE-RECOVERY-AVAILABLE",
            ],
            _ when pageId.StartsWith("UX-CAL-", StringComparison.Ordinal)
                   || pageId.StartsWith("UX-ANALYTICS-", StringComparison.Ordinal) =>
            [
                "STATE-STALE-DATA", "STATE-SYNCING", "STATE-PERMISSION-DENIED", "STATE-RECOVERY-AVAILABLE",
                "STATE-DEGRADED-PERFORMANCE",
            ],
            _ when pageId.StartsWith("UX-TEAM-", StringComparison.Ordinal)
                   || pageId.StartsWith("UX-ROOM-", StringComparison.Ordinal)
                   || pageId == "UX-COLLAB-001" =>
            [
                "STATE-STALE-DATA", "STATE-SYNCING", "STATE-CONFLICT", "STATE-PERMISSION-DENIED",
                "STATE-APPROVAL-PENDING", "STATE-APPROVAL-REJECTED", "STATE-RECOVERY-AVAILABLE",
            ],
            _ when pageId.StartsWith("UX-AI-", StringComparison.Ordinal) =>
            ["STATE-STALE-DATA", "STATE-PERMISSION-DENIED", "STATE-RECOVERY-AVAILABLE", "STATE-DEGRADED-PERFORMANCE"],
            _ when pageId.StartsWith("UX-DATA-", StringComparison.Ordinal)
                   || pageId == "UX-ARCH-001"
                   || pageId == "UX-REC-001"
                   || pageId.StartsWith("UX-MIG-", StringComparison.Ordinal) =>
            [
                "STATE-STALE-DATA", "STATE-SYNCING", "STATE-CONFLICT", "STATE-PERMISSION-DENIED",
                "STATE-RECOVERY-AVAILABLE", "STATE-SAFE-MODE",
            ],
            _ when pageId.StartsWith("UX-THEME-", StringComparison.Ordinal)
                   || pageId.StartsWith("UX-SET-", StringComparison.Ordinal)
                   || pageId.StartsWith("UX-AUTH-", StringComparison.Ordinal) =>
            [
                "STATE-PERMISSION-DENIED", "STATE-RECOVERY-AVAILABLE", "STATE-SAFE-MODE",
                "STATE-DEGRADED-PERFORMANCE", "STATE-REDUCED-MOTION", "STATE-REDUCED-TRANSPARENCY",
            ],
            _ when pageId.StartsWith("UX-EXT-", StringComparison.Ordinal)
                   || pageId == "UX-CONN-001" =>
            [
                "STATE-STALE-DATA", "STATE-SYNCING", "STATE-CONFLICT", "STATE-PERMISSION-DENIED",
                "STATE-RECOVERY-AVAILABLE", "STATE-SAFE-MODE",
            ],
            _ =>
            [
                "STATE-STALE-DATA", "STATE-SYNCING", "STATE-CONFLICT", "STATE-PERMISSION-DENIED",
                "STATE-RECOVERY-AVAILABLE", "STATE-SAFE-MODE", "STATE-DEGRADED-PERFORMANCE",
                "STATE-REDUCED-MOTION", "STATE-REDUCED-TRANSPARENCY",
            ],
        };

        return baseStates.Concat(additional).Distinct(StringComparer.Ordinal).ToArray();
    }

    private static JsonDocument ReadUxContract() => JsonDocument.Parse(
        File.ReadAllText(Path.Combine(RepositoryPaths.ThemeContracts, "ux-contract.v1.1.json")));
}
