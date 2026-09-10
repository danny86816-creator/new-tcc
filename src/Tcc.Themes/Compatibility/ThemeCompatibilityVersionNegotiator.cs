using System.Collections.Immutable;
using System.Globalization;
using System.Numerics;
using Tcc.Presentation.Contracts.Theme;
using MachineVersion = (System.Numerics.BigInteger Major, System.Numerics.BigInteger Minor, System.Numerics.BigInteger Patch);

namespace Tcc.Themes.Compatibility;

internal static class ThemeCompatibilityVersionNegotiator
{
    internal static ThemeCompatibilityVersionNegotiation Negotiate(ThemeCompatibilityRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        string?[] supportedThemeApiVersions = Snapshot(
            request.SupportedThemeApiVersions,
            out bool themeApiCollectionMissing);
        string?[] supportedUxContractVersions = Snapshot(
            request.SupportedUxContractVersions,
            out bool uxContractCollectionMissing);

        if (!string.Equals(request.Manifest?.SchemaVersion, ContractVersions.Schema, StringComparison.Ordinal)
            || !string.Equals(request.Compatibility?.SchemaVersion, ContractVersions.Schema, StringComparison.Ordinal))
        {
            return new ThemeCompatibilityVersionNegotiation(
                false,
                null,
                null,
                ImmutableArray.Create(ThemeCompatibilityNegotiationFailureKind.UnsupportedManifestSchema));
        }

        ThemeManifest manifest = request.Manifest!;
        ThemeCompatibilityManifest compatibility = request.Compatibility!;
        bool coreRequirementUsable = TryResolveRequirement(
            manifest.Compatibility?.RequiredCoreVersion,
            compatibility.Core?.Required,
            out (MachineVersion LowerInclusive, MachineVersion? UpperExclusive) coreRequirement,
            out bool coreInvalid,
            out bool coreUnsatisfiable,
            out bool coreConflict);
        bool themeApiRequirementUsable = TryResolveRequirement(
            manifest.Compatibility?.RequiredThemeApiVersion,
            compatibility.ThemeApi?.Required,
            out (MachineVersion LowerInclusive, MachineVersion? UpperExclusive) themeApiRequirement,
            out bool themeApiInvalid,
            out bool themeApiUnsatisfiable,
            out bool themeApiConflict);
        bool uxContractRequirementUsable = TryResolveRequirement(
            manifest.Compatibility?.RequiredUxContractVersion,
            compatibility.UxContract?.Required,
            out (MachineVersion LowerInclusive, MachineVersion? UpperExclusive) uxContractRequirement,
            out bool uxContractInvalid,
            out bool uxContractUnsatisfiable,
            out bool uxContractConflict);

        bool coreVersionValid = TryParseVersion(request.CoreVersion, out MachineVersion coreVersion);
        coreInvalid |= !coreVersionValid;

        bool themeApiVersionsValid = TryParseSupportedVersions(
            supportedThemeApiVersions,
            themeApiCollectionMissing,
            out MachineVersion[] parsedThemeApiVersions);
        themeApiInvalid |= !themeApiVersionsValid;

        bool uxContractVersionsValid = TryParseSupportedVersions(
            supportedUxContractVersions,
            uxContractCollectionMissing,
            out MachineVersion[] parsedUxContractVersions);
        uxContractInvalid |= !uxContractVersionsValid;

        ImmutableArray<ThemeCompatibilityNegotiationFailureKind>.Builder failures =
            ImmutableArray.CreateBuilder<ThemeCompatibilityNegotiationFailureKind>();
        AppendRequirementFailures(failures, coreInvalid, coreUnsatisfiable, coreConflict);
        AppendRequirementFailures(failures, themeApiInvalid, themeApiUnsatisfiable, themeApiConflict);
        AppendRequirementFailures(failures, uxContractInvalid, uxContractUnsatisfiable, uxContractConflict);

        if (coreRequirementUsable && coreVersionValid && !Contains(coreRequirement, coreVersion))
        {
            failures.Add(ThemeCompatibilityNegotiationFailureKind.CoreVersionIncompatible);
        }

        string? selectedThemeApiVersion = null;
        if (themeApiRequirementUsable && themeApiVersionsValid)
        {
            selectedThemeApiVersion = SelectHighestAuthorizedVersion(
                parsedThemeApiVersions,
                themeApiRequirement,
                new string[] { ContractVersions.ThemeApi });
            if (selectedThemeApiVersion is null)
            {
                failures.Add(ThemeCompatibilityNegotiationFailureKind.ThemeApiNoCompatibleVersion);
            }
        }

        string? selectedUxContractVersion = null;
        if (uxContractRequirementUsable && uxContractVersionsValid)
        {
            selectedUxContractVersion = SelectHighestAuthorizedVersion(
                parsedUxContractVersions,
                uxContractRequirement,
                new string[] { ContractVersions.UxContract });
            if (selectedUxContractVersion is null)
            {
                failures.Add(ThemeCompatibilityNegotiationFailureKind.UxContractNoCompatibleVersion);
            }
        }

        ImmutableArray<ThemeCompatibilityNegotiationFailureKind> completedFailures = failures.ToImmutable();
        return new ThemeCompatibilityVersionNegotiation(
            completedFailures.IsEmpty,
            selectedThemeApiVersion,
            selectedUxContractVersion,
            completedFailures);
    }

    private static string?[] Snapshot(IReadOnlySet<string>? values, out bool sourceMissing)
    {
        sourceMissing = values is null;
        if (values is null)
        {
            return Array.Empty<string?>();
        }

        List<string?> snapshot = new(values.Count);
        foreach (string? value in values)
        {
            snapshot.Add(value);
        }

        return snapshot.ToArray();
    }

    private static bool TryResolveRequirement(
        string? themeDeclaration,
        string? compatibilityDeclaration,
        out (MachineVersion LowerInclusive, MachineVersion? UpperExclusive) requirement,
        out bool invalid,
        out bool unsatisfiable,
        out bool conflict)
    {
        bool themeValid = TryParseRange(themeDeclaration, out var themeRange, out var themeFailure);
        bool compatibilityValid = TryParseRange(
            compatibilityDeclaration,
            out var compatibilityRange,
            out var compatibilityFailure);

        invalid = (!themeValid && themeFailure == ThemeCompatibilityNegotiationFailureKind.InvalidVersionInput)
            || (!compatibilityValid && compatibilityFailure == ThemeCompatibilityNegotiationFailureKind.InvalidVersionInput);
        unsatisfiable = (!themeValid && themeFailure == ThemeCompatibilityNegotiationFailureKind.UnsatisfiableVersionRange)
            || (!compatibilityValid && compatibilityFailure == ThemeCompatibilityNegotiationFailureKind.UnsatisfiableVersionRange);
        conflict = themeValid && compatibilityValid && !IntervalsEqual(themeRange, compatibilityRange);
        requirement = themeValid ? themeRange : compatibilityRange;
        return themeValid && compatibilityValid && !conflict;
    }

    private static void AppendRequirementFailures(
        ImmutableArray<ThemeCompatibilityNegotiationFailureKind>.Builder failures,
        bool invalid,
        bool unsatisfiable,
        bool conflict)
    {
        if (invalid)
        {
            failures.Add(ThemeCompatibilityNegotiationFailureKind.InvalidVersionInput);
        }

        if (unsatisfiable)
        {
            failures.Add(ThemeCompatibilityNegotiationFailureKind.UnsatisfiableVersionRange);
        }

        if (conflict)
        {
            failures.Add(ThemeCompatibilityNegotiationFailureKind.ConflictingVersionDeclaration);
        }
    }

    private static bool TryParseSupportedVersions(
        string?[] values,
        bool sourceMissing,
        out MachineVersion[] versions)
    {
        List<MachineVersion> parsed = new(values.Length);
        bool valid = !sourceMissing;
        foreach (string? value in values)
        {
            if (TryParseVersion(value, out MachineVersion version))
            {
                parsed.Add(version);
            }
            else
            {
                valid = false;
            }
        }

        versions = parsed.ToArray();
        return valid;
    }

    private static string? SelectHighestAuthorizedVersion(
        MachineVersion[] callerVersions,
        (MachineVersion LowerInclusive, MachineVersion? UpperExclusive) requirement,
        string[] authorizedVersions)
    {
        bool found = false;
        MachineVersion highest = default;
        string? selected = null;

        foreach (string authorizedText in authorizedVersions)
        {
            if (!TryParseVersion(authorizedText, out MachineVersion authorized)
                || !ContainsVersion(callerVersions, authorized)
                || !Contains(requirement, authorized))
            {
                continue;
            }

            if (!found || Compare(authorized, highest) > 0)
            {
                found = true;
                highest = authorized;
                selected = authorizedText;
            }
        }

        return selected;
    }

    private static bool ContainsVersion(MachineVersion[] versions, MachineVersion candidate)
    {
        foreach (MachineVersion version in versions)
        {
            if (Compare(version, candidate) == 0)
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryParseRange(
        string? value,
        out (MachineVersion LowerInclusive, MachineVersion? UpperExclusive) interval,
        out ThemeCompatibilityNegotiationFailureKind failure)
    {
        MachineVersion lower = (BigInteger.Zero, BigInteger.Zero, BigInteger.Zero);
        MachineVersion? upper = null;
        interval = (lower, upper);
        failure = ThemeCompatibilityNegotiationFailureKind.InvalidVersionInput;

        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        int index = 0;
        while (index < value.Length && value[index] == ' ')
        {
            index++;
        }

        if (index == value.Length)
        {
            return false;
        }

        while (index < value.Length)
        {
            string operation = string.Empty;
            if (value[index] is '>' or '<')
            {
                operation = value[index].ToString();
                index++;
                if (index < value.Length && value[index] == '=')
                {
                    operation += "=";
                    index++;
                }
            }
            else if (value[index] == '=')
            {
                operation = "=";
                index++;
            }

            int versionStart = index;
            while (index < value.Length && value[index] != ' ')
            {
                index++;
            }

            if (!TryParseVersion(value.AsSpan(versionStart, index - versionStart), out MachineVersion version))
            {
                return false;
            }

            switch (operation)
            {
                case "":
                case "=":
                    lower = Max(lower, version);
                    upper = Min(upper, Successor(version));
                    break;
                case ">=":
                    lower = Max(lower, version);
                    break;
                case ">":
                    lower = Max(lower, Successor(version));
                    break;
                case "<=":
                    upper = Min(upper, Successor(version));
                    break;
                case "<":
                    upper = Min(upper, version);
                    break;
                default:
                    return false;
            }

            if (index == value.Length)
            {
                break;
            }

            while (index < value.Length && value[index] == ' ')
            {
                index++;
            }

            if (index == value.Length)
            {
                break;
            }
        }

        interval = (lower, upper);
        if (upper.HasValue && Compare(lower, upper.Value) >= 0)
        {
            failure = ThemeCompatibilityNegotiationFailureKind.UnsatisfiableVersionRange;
            return false;
        }

        return true;
    }

    private static bool TryParseVersion(string? value, out MachineVersion version)
    {
        version = default;
        return value is not null && TryParseVersion(value.AsSpan(), out version);
    }

    private static bool TryParseVersion(ReadOnlySpan<char> value, out MachineVersion version)
    {
        version = default;
        int firstDot = value.IndexOf('.');
        if (firstDot <= 0)
        {
            return false;
        }

        int secondRelativeDot = value[(firstDot + 1)..].IndexOf('.');
        if (secondRelativeDot <= 0)
        {
            return false;
        }

        int secondDot = firstDot + 1 + secondRelativeDot;
        if (secondDot >= value.Length - 1 || value[(secondDot + 1)..].IndexOf('.') >= 0)
        {
            return false;
        }

        if (!TryParseComponent(value[..firstDot], out BigInteger major)
            || !TryParseComponent(value[(firstDot + 1)..secondDot], out BigInteger minor)
            || !TryParseComponent(value[(secondDot + 1)..], out BigInteger patch))
        {
            return false;
        }

        version = (major, minor, patch);
        return true;
    }

    private static bool TryParseComponent(ReadOnlySpan<char> value, out BigInteger component)
    {
        component = default;
        if (value.IsEmpty || (value.Length > 1 && value[0] == '0'))
        {
            return false;
        }

        foreach (char character in value)
        {
            if (character < '0' || character > '9')
            {
                return false;
            }
        }

        return BigInteger.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out component);
    }

    private static bool Contains(
        (MachineVersion LowerInclusive, MachineVersion? UpperExclusive) interval,
        MachineVersion version) =>
        Compare(version, interval.LowerInclusive) >= 0
        && (!interval.UpperExclusive.HasValue || Compare(version, interval.UpperExclusive.Value) < 0);

    private static bool IntervalsEqual(
        (MachineVersion LowerInclusive, MachineVersion? UpperExclusive) left,
        (MachineVersion LowerInclusive, MachineVersion? UpperExclusive) right) =>
        Compare(left.LowerInclusive, right.LowerInclusive) == 0
        && ((!left.UpperExclusive.HasValue && !right.UpperExclusive.HasValue)
            || (left.UpperExclusive.HasValue
                && right.UpperExclusive.HasValue
                && Compare(left.UpperExclusive.Value, right.UpperExclusive.Value) == 0));

    private static MachineVersion Successor(MachineVersion version) =>
        (version.Major, version.Minor, version.Patch + BigInteger.One);

    private static MachineVersion Max(MachineVersion left, MachineVersion right) =>
        Compare(left, right) >= 0 ? left : right;

    private static MachineVersion? Min(MachineVersion? current, MachineVersion candidate) =>
        !current.HasValue || Compare(candidate, current.Value) < 0 ? candidate : current;

    private static int Compare(MachineVersion left, MachineVersion right)
    {
        int major = left.Major.CompareTo(right.Major);
        if (major != 0)
        {
            return major;
        }

        int minor = left.Minor.CompareTo(right.Minor);
        return minor != 0 ? minor : left.Patch.CompareTo(right.Patch);
    }
}

internal sealed record ThemeCompatibilityVersionNegotiation(
    bool CanContinue,
    string? SelectedThemeApiVersion,
    string? SelectedUxContractVersion,
    ImmutableArray<ThemeCompatibilityNegotiationFailureKind> Failures);

internal enum ThemeCompatibilityNegotiationFailureKind
{
    InvalidVersionInput,
    UnsatisfiableVersionRange,
    UnsupportedManifestSchema,
    ConflictingVersionDeclaration,
    CoreVersionIncompatible,
    ThemeApiNoCompatibleVersion,
    UxContractNoCompatibleVersion,
}
