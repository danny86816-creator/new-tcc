using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Tcc.Presentation.Contracts.Theme;
using Tcc.Themes.Integrity;

namespace Tcc.Architecture.Tests;

public sealed class PhaseFourThemePackageInventoryEvaluatorTests
{
    private const string ReservedMessage = "A payload file declaration must not target a reserved integrity metadata path.";

    [Theory]
    [InlineData("assets/icon.png", "hello")]
    [InlineData("themes/default/main.json", "nested")]
    [InlineData("A/B/c.txt", "case")]
    [InlineData("assets/é.txt", "unicode")]
    [InlineData("assets/e\u0301.txt", "decomposed")]
    [InlineData("empty.bin", "")]
    public async Task CorrectRequiredFileProducesCompleteVerifiedEvidence(string path, string content)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        string canonical = path.Normalize(NormalizationForm.FormC);
        ThemeIntegrityFileV2 file = File(path, bytes);
        RecordingReader reader = new([Entry(path, bytes.Length)], new() { [canonical] = bytes });
        ThemePackageInventoryEvaluation result = await Evaluate(Request(file), reader);

        Assert.True(result.CanContinue);
        Assert.Empty(result.Diagnostics);
        Assert.Equal(new ThemeIntegrityFileEvidenceV1(canonical, ThemeIntegrityEntryKind.Payload, true, true,
            bytes.Length, bytes.Length, Hash(bytes), Hash(bytes), ThemeIntegrityEvidenceStatus.Verified),
            Assert.Single(result.FileEvidence));
        Assert.Equal([canonical], reader.ReadPaths);
    }

    [Fact]
    public async Task AllRequiredAndOptionalPresentFilesAreReadOnceInOrdinalOrder()
    {
        byte[] moderate = Enumerable.Range(0, 256 * 1024).Select(index => (byte)(index % 251)).ToArray();
        byte[] theme = Encoding.UTF8.GetBytes("theme raw bytes\r\n");
        ThemeIntegrityFileV2[] files =
        [
            File("z.bin", moderate),
            File("theme.json", theme) with { EntryKind = ThemeIntegrityEntryKind.ThemeManifest },
            File("optional.bin", []) with { Required = false },
        ];
        RecordingReader reader = new(
            [Entry("z.bin", moderate.Length), Entry("theme.json", theme.Length), Entry("optional.bin", 0)],
            new() { ["z.bin"] = moderate, ["theme.json"] = theme, ["optional.bin"] = [] });
        ThemePackageInventoryEvaluation result = await Evaluate(Request(files), reader);

        Assert.True(result.CanContinue);
        Assert.Empty(result.Diagnostics);
        Assert.Equal(["optional.bin", "theme.json", "z.bin"], reader.ReadPaths);
        Assert.All(result.FileEvidence, item => Assert.Equal(ThemeIntegrityEvidenceStatus.Verified, item.Status));
        Assert.Equal(Hash(moderate), result.FileEvidence[2].ActualSha256);
        Assert.Equal(moderate.Length, result.FileEvidence[2].ActualLengthBytes);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AbsentDeclarationHasOnlyTheApplicablePresenceOutcome(bool required)
    {
        ThemeIntegrityFileV2 file = File("absent.bin", [7]) with { Required = required };
        RecordingReader reader = new([], []);
        ThemePackageInventoryEvaluation result = await Evaluate(Request(file), reader);

        Assert.Equal(!required, result.CanContinue);
        Assert.Equal(new ThemeIntegrityFileEvidenceV1("absent.bin", ThemeIntegrityEntryKind.Payload, required,
            false, 1, null, Hash([7]), null, required ? ThemeIntegrityEvidenceStatus.MissingRequired
                : ThemeIntegrityEvidenceStatus.MissingAllowed), Assert.Single(result.FileEvidence));
        AssertCodes(result, required ? ["P4I005"] : []);
        Assert.Empty(reader.ReadPaths);
    }

    public static IEnumerable<object[]> HostilePaths()
    {
        string[] lexical =
        [
            "", "/absolute", "\\absolute", "C:/x", "C:\\x", "\\\\server\\share", "../x", "a/../b",
            "./x", "a//b", "a\\b", "a:b", "file.txt:stream", "a/", "a/./b", "trailing-dot.",
            "trailing-space ", "a./b", "a /b", "high-surrogate-vector", "low-surrogate-vector", "embedded-surrogate-vector",
        ];
        foreach (string path in lexical)
        {
            yield return [path];
        }

        foreach (int control in Enumerable.Range(0, 32).Concat(Enumerable.Range(127, 33)))
        {
            yield return [$"a{(char)control}b"];
        }

        string[] devices = ["CON", "PRN", "AUX", "NUL", "CLOCK$", "CONIN$", "CONOUT$",
            .. Enumerable.Range(1, 9).Select(number => $"COM{number}"),
            .. Enumerable.Range(1, 9).Select(number => $"LPT{number}")];
        foreach (string device in devices)
        {
            yield return [device];
            yield return [$"dir/{device.ToLowerInvariant()}.txt"];
            yield return [$"{device}.TXT.more"];
        }
    }

    [Theory]
    [MemberData(nameof(HostilePaths))]
    public async Task HostileDeclaredAndPresentPathsAreRejectedWithoutReadingForFiftyRuns(string path)
    {
        // Construct ill-formed UTF-16 after xUnit transport, which otherwise replaces it with U+FFFD.
        path = path switch
        {
            "high-surrogate-vector" => "\uD800",
            "low-surrogate-vector" => "\uDC00",
            "embedded-surrogate-vector" => "a\uD800z",
            _ => path,
        };
        ThemePackageInventoryEvaluation? first = null;
        for (int run = 0; run < 50; run++)
        {
            RecordingReader reader = new([Entry(path, 1)], []);
            ThemePackageInventoryEvaluation result = await Evaluate(Request(File(path, [1])), reader);
            Assert.False(result.CanContinue);
            Assert.NotEmpty(result.Diagnostics);
            Assert.All(result.Diagnostics, diagnostic =>
            {
                Assert.Equal("P4I002", diagnostic.Code);
                Assert.Equal(path, diagnostic.CanonicalPath);
                Assert.Equal(ThemeDiagnosticsSeverity.Error, diagnostic.Severity);
            });
            Assert.Empty(result.FileEvidence);
            Assert.Empty(reader.ReadPaths);
            if (first is not null) AssertSame(first, result);
            first ??= result;
        }
    }

    [Theory]
    [InlineData("same.bin", "same.bin", "P4I003", "same.bin")]
    [InlineData("assets/e\u0301.txt", "assets/é.txt", "P4I003", "assets/é.txt")]
    [InlineData("A/file.txt", "a/FILE.txt", "P4I004", "A/file.txt")]
    public async Task BothInventoriesRejectAmbiguousGroupsWithoutFabricatedEvidence(
        string firstPath, string secondPath, string code, string representative)
    {
        foreach (bool declaredCollision in new[] { true, false })
        {
            ThemePackageInventoryEvaluation? first = null;
            for (int run = 0; run < 50; run++)
            {
                ThemeIntegrityFileV2[] files = declaredCollision
                    ? [File(firstPath, [1]), File(secondPath, [1])] : [File(firstPath, [1])];
                ThemePackageContentEntryV1[] entries = declaredCollision
                    ? [Entry(firstPath, 1)] : [Entry(firstPath, 1), Entry(secondPath, 1)];
                if (run % 2 != 0) { Array.Reverse(files); Array.Reverse(entries); }
                RecordingReader reader = new(entries, []);
                ThemePackageInventoryEvaluation result = await Evaluate(Request(files), reader);
                AssertCodes(result, code);
                Assert.Equal(representative, Assert.Single(result.Diagnostics).CanonicalPath);
                Assert.Empty(result.FileEvidence);
                Assert.Empty(reader.ReadPaths);
                if (first is not null) AssertSame(first, result);
                first ??= result;
            }
        }
    }

    [Fact]
    public async Task CaseOnlyCrossInventoryMismatchIsMissingAndUndeclaredNotAnOrdinaryMatch()
    {
        RecordingReader reader = new([Entry("FILE.bin", 1)], []);
        ThemePackageInventoryEvaluation result = await Evaluate(Request(File("file.bin", [1])), reader);
        AssertCodes(result, "P4I005", "P4I006");
        Assert.Equal(["file.bin", "FILE.bin"], result.Diagnostics.Select(item => item.CanonicalPath));
        Assert.Equal(ThemeIntegrityEvidenceStatus.MissingRequired, Assert.Single(result.FileEvidence).Status);
        Assert.Empty(reader.ReadPaths);
    }

    [Theory]
    [InlineData("e\u0301.bin", "é.bin")]
    [InlineData("é.bin", "e\u0301.bin")]
    public async Task NfcEquivalentCrossInventoryPathsMatch(string declared, string present)
    {
        RecordingReader reader = new([Entry(present, 1)], new() { ["é.bin"] = [9] });
        ThemePackageInventoryEvaluation result = await Evaluate(Request(File(declared, [9])), reader);
        Assert.True(result.CanContinue);
        Assert.Equal("é.bin", Assert.Single(reader.ReadPaths));
        Assert.Equal("é.bin", Assert.Single(result.FileEvidence).CanonicalPath);
    }

    [Fact]
    public async Task UndeclaredFileIsNotReadOrGivenInventedEvidence()
    {
        RecordingReader reader = new([Entry("extra.bin", 1)], []);
        ThemePackageInventoryEvaluation result = await Evaluate(Request(), reader);
        AssertCodes(result, "P4I006");
        Assert.Equal("extra.bin", Assert.Single(result.Diagnostics).CanonicalPath);
        Assert.Empty(result.FileEvidence);
        Assert.Empty(reader.ReadPaths);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public async Task InvalidDirectFileSecurityValuesUseP4I029AndAreNeverRead(int vector)
    {
        ThemeIntegrityFileV2 valid = File("payload.bin", [1]);
        ThemeIntegrityFileV2 file = vector switch
        {
            0 => valid with { EntryKind = (ThemeIntegrityEntryKind)999 },
            1 => valid with { LengthBytes = -1 },
            2 => valid with { Sha256 = new string('a', 63) },
            3 => valid with { Sha256 = new string('a', 65) },
            4 => valid with { Sha256 = new string('g', 64) },
            5 => valid with { Sha256 = valid.Sha256.ToUpperInvariant() },
            6 => valid with { Sha256 = "" },
            _ => valid with { Sha256 = null! },
        };
        RecordingReader reader = new([Entry("payload.bin", 1)], []);
        ThemePackageInventoryEvaluation result = await Evaluate(Request(file), reader);
        AssertCodes(result, "P4I029");
        Assert.Null(Assert.Single(result.Diagnostics).CanonicalPath);
        Assert.Empty(result.FileEvidence);
        Assert.Empty(reader.ReadPaths);
    }

    [Theory]
    [InlineData(ThemePackageContentEntryKind.Directory)]
    [InlineData(ThemePackageContentEntryKind.SymbolicLink)]
    [InlineData(ThemePackageContentEntryKind.ReparsePoint)]
    [InlineData(ThemePackageContentEntryKind.Unsupported)]
    [InlineData((ThemePackageContentEntryKind)999)]
    public async Task UnsupportedPhysicalEntriesAreNeverRead(ThemePackageContentEntryKind kind)
    {
        RecordingReader reader = new([Entry("payload.bin", 99) with { EntryKind = kind }], []);
        ThemePackageInventoryEvaluation result = await Evaluate(Request(File("payload.bin", [1])), reader);
        AssertCodes(result, "P4I019");
        ThemeIntegrityFileEvidenceV1 item = Assert.Single(result.FileEvidence);
        Assert.Equal(ThemeIntegrityEvidenceStatus.UnsupportedEntry, item.Status);
        Assert.True(item.IsPresent);
        Assert.Null(item.ActualLengthBytes);
        Assert.Null(item.ActualSha256);
        Assert.Empty(reader.ReadPaths);
    }

    [Theory]
    [InlineData(2, 1, false, "P4I007", ThemeIntegrityEvidenceStatus.LengthMismatch)]
    [InlineData(1, 2, false, "P4I007", ThemeIntegrityEvidenceStatus.LengthMismatch)]
    [InlineData(2, 3, false, "P4I007", ThemeIntegrityEvidenceStatus.LengthMismatch)]
    [InlineData(1, -1, false, "P4I007", ThemeIntegrityEvidenceStatus.LengthMismatch)]
    [InlineData(1, 1, false, "P4I008", ThemeIntegrityEvidenceStatus.HashMismatch)]
    [InlineData(1, 1, true, "P4I010", ThemeIntegrityEvidenceStatus.HashMismatch)]
    [InlineData(2, 1, true, "P4I007", ThemeIntegrityEvidenceStatus.LengthMismatch)]
    public async Task RawByteLengthAndHashFailuresRetainLengthPrecedence(
        long declaredLength, long enumeratedLength, bool themeManifest, string code, ThemeIntegrityEvidenceStatus status)
    {
        ThemeIntegrityFileV2 file = File("payload.bin", [0]) with
        {
            LengthBytes = declaredLength,
            EntryKind = themeManifest ? ThemeIntegrityEntryKind.ThemeManifest : ThemeIntegrityEntryKind.Payload,
        };
        RecordingReader reader = new([Entry("payload.bin", enumeratedLength)], new() { ["payload.bin"] = [1] });
        ThemePackageInventoryEvaluation result = await Evaluate(Request(file), reader);
        AssertCodes(result, code);
        ThemeIntegrityFileEvidenceV1 item = Assert.Single(result.FileEvidence);
        Assert.Equal(status, item.Status);
        Assert.Equal(1, item.ActualLengthBytes);
        Assert.Equal(Hash([1]), item.ActualSha256);
        Assert.Equal(Hash([0]), item.DeclaredSha256);
        Assert.Equal(declaredLength, item.DeclaredLengthBytes);
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    [InlineData(true, 2)]
    [InlineData(true, 3)]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(false, 2)]
    [InlineData(false, 3)]
    public async Task ExpectedReaderFailuresUseFixedDiagnosticsWithoutInfrastructureDetails(bool enumeration, int kind)
    {
        Exception failure = ReaderFailure(kind);
        RecordingReader reader = new([Entry("payload.bin", 123)], [])
        {
            EnumerationFailure = enumeration ? failure : null,
            ContentFailure = enumeration ? null : failure,
        };
        ThemePackageInventoryEvaluation result = await Evaluate(Request(File("payload.bin", [1])), reader);
        AssertCodes(result, "P4I021");
        ThemeIntegrityDiagnosticV1 diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal(enumeration ? null : "payload.bin", diagnostic.CanonicalPath);
        Assert.DoesNotContain("PRIVATE", diagnostic.Message, StringComparison.Ordinal);
        if (enumeration)
        {
            Assert.Empty(result.FileEvidence);
            Assert.Empty(reader.ReadPaths);
        }
        else
        {
            ThemeIntegrityFileEvidenceV1 item = Assert.Single(result.FileEvidence);
            Assert.Equal(ThemeIntegrityEvidenceStatus.Unavailable, item.Status);
            Assert.True(item.IsPresent);
            Assert.Equal(123, item.ActualLengthBytes); // Enumeration claim only; no content read succeeded.
            Assert.Null(item.ActualSha256);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UnrelatedReaderExceptionsPropagateUnchanged(bool enumeration)
    {
        InvalidOperationException failure = new("Programmer fault");
        RecordingReader reader = new([Entry("payload.bin", 1)], [])
        {
            EnumerationFailure = enumeration ? failure : null,
            ContentFailure = enumeration ? null : failure,
        };
        Exception actual = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await ThemePackageInventoryEvaluator.EvaluateAsync(Request(File("payload.bin", [1])), reader));
        Assert.Same(failure, actual);
    }

    [Fact]
    public async Task PreCancelledTokenPropagatesWithoutEnumeration()
    {
        using CancellationTokenSource source = new();
        source.Cancel();
        RecordingReader reader = new([], []);
        OperationCanceledException error = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await ThemePackageInventoryEvaluator.EvaluateAsync(Request(), reader, source.Token));
        Assert.Equal(source.Token, error.CancellationToken);
        Assert.Equal(0, reader.EnumerationCount);
        Assert.Empty(reader.ReadPaths);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CancellationDuringSuspendedReaderOperationPropagatesOriginalToken(bool enumeration)
    {
        using CancellationTokenSource source = new();
        TaskCompletionSource entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        RecordingReader reader = new([Entry("a.bin", 1)], new() { ["a.bin"] = [1] })
        {
            ExpectedToken = source.Token,
            BeforeEnumeration = enumeration ? WaitForCancellation : null,
            BeforeRead = enumeration ? null : _ => WaitForCancellation(),
        };
        async Task WaitForCancellation()
        {
            entered.SetResult();
            await Task.Delay(Timeout.Infinite, source.Token);
        }

        Task<ThemePackageInventoryEvaluation> operation = ThemePackageInventoryEvaluator
            .EvaluateAsync(Request(File("a.bin", [1])), reader, source.Token).AsTask();
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        source.Cancel();
        OperationCanceledException error = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operation);
        Assert.Equal(source.Token, error.CancellationToken);
        Assert.Equal(enumeration ? 0 : 1, reader.ReadPaths.Count);
    }

    [Fact]
    public async Task CancellationBetweenEntriesPreventsTheNextReadEvenIfReaderReturnsBytes()
    {
        using CancellationTokenSource source = new();
        RecordingReader reader = new([Entry("a.bin", 1), Entry("b.bin", 1)],
            new() { ["a.bin"] = [1], ["b.bin"] = [2] })
        {
            ExpectedToken = source.Token,
            AfterRead = _ => source.Cancel(),
        };
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await ThemePackageInventoryEvaluator.EvaluateAsync(
                Request(File("b.bin", [2]), File("a.bin", [1])), reader, source.Token));
        Assert.Equal(["a.bin"], reader.ReadPaths);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CancellationWinsOverConcurrentExpectedReaderFailure(bool enumeration)
    {
        using CancellationTokenSource source = new();
        RecordingReader reader = new([Entry("a.bin", 1)], [])
        {
            ExpectedToken = source.Token,
            EnumerationFailure = enumeration ? new IOException("PRIVATE") : null,
            ContentFailure = enumeration ? null : new IOException("PRIVATE"),
            BeforeEnumeration = enumeration ? Cancel : null,
            BeforeRead = enumeration ? null : _ => Cancel(),
        };
        Task Cancel() { source.Cancel(); return Task.CompletedTask; }
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await ThemePackageInventoryEvaluator.EvaluateAsync(Request(File("a.bin", [1])), reader, source.Token));
    }

    public static IEnumerable<object[]> ReservedDeclarations()
    {
        foreach (bool signature in new[] { false, true })
        foreach (bool required in new[] { false, true })
        foreach (bool present in new[] { false, true })
            yield return [signature, required, present];
    }

    [Theory]
    [MemberData(nameof(ReservedDeclarations))]
    public async Task ExactReservedDeclarationIsInvalidRegardlessOfPresenceOrRequiredFlag(
        bool signature, bool required, bool present)
    {
        string path = signature ? "meta/signature.json" : "meta/integrity.json";
        RecordingReader reader = new(present ? [Entry(path, 1)] : [], []);
        ThemePackageInventoryEvaluation result = await Evaluate(
            Request(File(path, [1]) with { Required = required }), reader);
        AssertReservedRejection(result);
        Assert.Empty(reader.ReadPaths);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task NfcEquivalentReservedDeclarationIsInvalid(bool signature)
    {
        ThemeIntegrityVerificationRequestV2 request = Request(File("meta/e\u0301.json", [1]));
        request = signature
            ? request with { SignatureEnvelopePath = ThemeCanonicalPath.Parse("meta/é.json") }
            : request with { IntegrityManifestPath = ThemeCanonicalPath.Parse("meta/é.json") };
        RecordingReader reader = new([Entry("meta/é.json", 1)], []);
        ThemePackageInventoryEvaluation result = await Evaluate(request, reader);
        AssertReservedRejection(result);
        Assert.Empty(reader.ReadPaths);
    }

    [Fact]
    public async Task ExactPresentMetadataWithoutDeclarationsIsExcludedWithoutReads()
    {
        RecordingReader reader = new([Entry("meta/integrity.json", 50), Entry("meta/signature.json", 70)], []);
        ThemePackageInventoryEvaluation result = await Evaluate(Request(), reader);
        Assert.True(result.CanContinue);
        Assert.Empty(result.Diagnostics);
        Assert.Empty(result.FileEvidence);
        Assert.Empty(reader.ReadPaths);
    }

    [Theory]
    [InlineData("meta/integrity.json.bak")]
    [InlineData("meta/signature.json.bak")]
    [InlineData("META/integrity.json")]
    [InlineData("META/signature.json")]
    public async Task SimilarAndCaseVariantMetadataNamesUseOrdinaryInventoryRules(string path)
    {
        RecordingReader declaredReader = new([Entry(path, 1)], new() { [path] = [1] });
        ThemePackageInventoryEvaluation declared = await Evaluate(Request(File(path, [1])), declaredReader);
        Assert.True(declared.CanContinue);
        Assert.Equal(ThemeIntegrityEvidenceStatus.Verified, Assert.Single(declared.FileEvidence).Status);
        Assert.Equal([path], declaredReader.ReadPaths);

        RecordingReader undeclaredReader = new([Entry(path, 1)], []);
        ThemePackageInventoryEvaluation undeclared = await Evaluate(Request(), undeclaredReader);
        AssertCodes(undeclared, "P4I006");
        Assert.Empty(undeclaredReader.ReadPaths);
    }

    [Fact]
    public async Task NullSignatureIdentityDoesNotReserveAConventionalSignatureFilename()
    {
        ThemeIntegrityVerificationRequestV2 request = Request() with { SignatureEnvelopePath = null };
        ThemePackageInventoryEvaluation result = await Evaluate(request,
            new RecordingReader([Entry("meta/signature.json", 1)], []));
        AssertCodes(result, "P4I006");
    }

    [Theory]
    [InlineData(0, "P4I003")]
    [InlineData(1, "P4I004")]
    [InlineData(2, "P4I019")]
    [InlineData(3, "P4I002")]
    public async Task PresentReservedMetadataCannotBypassPackageEntrySafety(int vector, string code)
    {
        ThemePackageContentEntryV1[] entries = vector switch
        {
            0 => [Entry("meta/integrity.json", 1), Entry("meta/integrity.json", 1)],
            1 => [Entry("meta/integrity.json", 1), Entry("META/integrity.json", 1)],
            2 => [Entry("meta/integrity.json", 1) with { EntryKind = ThemePackageContentEntryKind.SymbolicLink }],
            _ => [Entry("meta/../meta/integrity.json", 1)],
        };
        RecordingReader reader = new(entries, []);
        ThemePackageInventoryEvaluation result = await Evaluate(Request(), reader);
        AssertCodes(result, code);
        Assert.Empty(result.FileEvidence);
        Assert.Empty(reader.ReadPaths);
    }

    [Fact]
    public async Task ReservedCaseCollisionBlocksTheOtherwiseDeclaredVariant()
    {
        RecordingReader reader = new([Entry("meta/integrity.json", 1), Entry("META/integrity.json", 1)], []);
        ThemePackageInventoryEvaluation result = await Evaluate(Request(File("META/integrity.json", [1])), reader);
        AssertCodes(result, "P4I004");
        Assert.Equal("META/integrity.json", Assert.Single(result.Diagnostics).CanonicalPath);
        Assert.Empty(result.FileEvidence);
        Assert.Empty(reader.ReadPaths);
    }

    [Fact]
    public async Task CompleteMixedOutcomeIsIdenticalAcrossCulturesOrdersAndFiftyRepetitions()
    {
        CultureInfo previousCulture = CultureInfo.CurrentCulture;
        CultureInfo previousUiCulture = CultureInfo.CurrentUICulture;
        ThemePackageInventoryEvaluation? first = null;
        try
        {
            foreach (string culture in new[] { "en-US", "tr-TR", "zh-TW" })
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
                for (int run = 0; run < 50; run++)
                {
                    ThemeIntegrityFileV2[] files =
                    [
                        File("meta/integrity.json", [0]), File("absent.bin", [1]),
                        File("optional.bin", [1]) with { Required = false }, File("verified.bin", [2]),
                        File("bad-hash.bin", [0]), File("length.bin", [0]) with { LengthBytes = 7 },
                        File("unavailable.bin", [0]), File("unsupported.bin", [0]),
                        File("theme.json", [0]) with { EntryKind = ThemeIntegrityEntryKind.ThemeManifest },
                        File("I/file.bin", [0]), File("i/FILE.bin", [0]), File("é.bin", [0]), File("e\u0301.bin", [0]),
                    ];
                    ThemePackageContentEntryV1[] entries =
                    [
                        Entry("meta/integrity.json", 0), Entry("verified.bin", 1), Entry("bad-hash.bin", 1),
                        Entry("length.bin", 1), Entry("unavailable.bin", 42), Entry("theme.json", 1),
                        Entry("unsupported.bin", 1) with { EntryKind = ThemePackageContentEntryKind.ReparsePoint },
                        Entry("I/file.bin", 1), Entry("é.bin", 1), Entry("extra.bin", 1), Entry("../hostile", 0),
                    ];
                    if (run % 2 != 0) { Array.Reverse(entries); Array.Reverse(files); }
                    RecordingReader reader = new(entries, new()
                    {
                        ["verified.bin"] = [2], ["bad-hash.bin"] = [1], ["length.bin"] = [1], ["theme.json"] = [1],
                    });
                    ThemePackageInventoryEvaluation result = await Evaluate(Request(files), reader);
                    AssertCodes(result, "P4I002", "P4I003", "P4I004", "P4I005", "P4I006", "P4I007",
                        "P4I008", "P4I010", "P4I019", "P4I021", "P4I029");
                    Assert.Equal(["bad-hash.bin", "length.bin", "theme.json", "unavailable.bin", "verified.bin"], reader.ReadPaths);
                    Assert.Equal(8, result.FileEvidence.Count);
                    Assert.DoesNotContain(result.FileEvidence, item => item.CanonicalPath == "meta/integrity.json");
                    if (first is not null) AssertSame(first, result);
                    first ??= result;
                }
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }

    [Fact]
    public async Task EnumerationMetadataIsSnapshottedBeforeAnyRead()
    {
        List<ThemePackageContentEntryV1> entries = [Entry("a.bin", 1), Entry("b.bin", 1)];
        RecordingReader reader = new(entries, new() { ["a.bin"] = [1], ["b.bin"] = [2] })
        {
            AfterRead = _ => entries.Clear(),
        };
        ThemePackageInventoryEvaluation result = await Evaluate(Request(File("a.bin", [1]), File("b.bin", [2])), reader);
        Assert.True(result.CanContinue);
        Assert.Equal(["a.bin", "b.bin"], reader.ReadPaths);
        Assert.Equal(2, result.FileEvidence.Count);
    }

    [Fact]
    public void CompiledStageSurfaceHasNoPublicVerifierOrPackageVerdict()
    {
        Type evaluator = typeof(ThemePackageInventoryEvaluator);
        Type outcome = typeof(ThemePackageInventoryEvaluation);
        Assert.False(evaluator.IsPublic);
        Assert.True(evaluator.IsAbstract && evaluator.IsSealed);
        Assert.False(outcome.IsPublic);
        Assert.True(outcome.IsSealed);
        Assert.Empty(evaluator.GetInterfaces());
        Assert.Equal(["CanContinue", "Diagnostics", "FileEvidence"],
            outcome.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(property => property.Name)
                .Order(StringComparer.Ordinal));
        PhaseThreeScopeBoundaryTests.AssertExactVerifierImplementations(evaluator.Assembly.GetTypes());
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task P4BIV001OriginalReservedCaseCollisionNeverReadsOrProducesEvidence(bool signature, bool required)
    {
        string reserved = signature ? "meta/signature.json" : "meta/integrity.json";
        string variant = reserved.ToUpperInvariant();
        RecordingReader reader = new([Entry(variant, 1)], new() { [variant] = [1] });
        ThemePackageInventoryEvaluation result = await Evaluate(Request(
            File(reserved, [1]) with { Required = required },
            File(variant, [1]) with { Required = required }), reader);
        AssertCodes(result, "P4I004", "P4I029");
        Assert.Equal(variant, result.Diagnostics[0].CanonicalPath);
        Assert.Null(result.Diagnostics[1].CanonicalPath);
        Assert.Equal(ReservedMessage, result.Diagnostics[1].Message);
        Assert.Empty(reader.ReadPaths);
        Assert.Empty(result.FileEvidence);
    }

    public static IEnumerable<object[]> P4BIV001ReservedAmbiguityVectors()
    {
        foreach (bool signature in new[] { false, true })
        foreach (bool required in new[] { false, true })
        foreach (int collision in new[] { 0, 1, 2 })
        foreach (int presence in new[] { 0, 1, 2, 3 })
            yield return [signature, required, collision, presence];
    }

    [Theory]
    [MemberData(nameof(P4BIV001ReservedAmbiguityVectors))]
    public async Task P4BIV001WholeReservedAmbiguityGroupIsBlockedRegardlessOfPresence(
        bool signature, bool required, int collision, int presence)
    {
        string reserved = collision == 1 ? "meta/é.json"
            : signature ? "meta/signature.json" : "meta/integrity.json";
        string variant = collision switch { 0 => reserved, 1 => "meta/e\u0301.json", _ => reserved.ToUpperInvariant() };
        ThemeIntegrityVerificationRequestV2 request = Request(
            File(reserved, [1]) with { Required = required },
            File(variant, [1]) with { Required = required });
        request = signature
            ? request with { SignatureEnvelopePath = ThemeCanonicalPath.Parse(reserved) }
            : request with { IntegrityManifestPath = ThemeCanonicalPath.Parse(reserved) };
        List<ThemePackageContentEntryV1> entries = [];
        if ((presence & 1) != 0) entries.Add(Entry(variant, 1));
        if ((presence & 2) != 0) entries.Add(Entry(reserved, 1));
        Dictionary<string, byte[]> contents = new(StringComparer.Ordinal) { [reserved] = [1] };
        contents[variant.Normalize(NormalizationForm.FormC)] = [1];
        RecordingReader reader = new(entries, contents);
        ThemePackageInventoryEvaluation result = await Evaluate(request, reader);
        AssertCodes(result, collision == 2 ? "P4I004" : "P4I003", "P4I029");
        Assert.Equal(collision == 2 ? variant : reserved, result.Diagnostics[0].CanonicalPath);
        Assert.Null(result.Diagnostics[1].CanonicalPath);
        Assert.Equal(ReservedMessage, result.Diagnostics[1].Message);
        Assert.Empty(reader.ReadPaths);
        Assert.Empty(result.FileEvidence);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task P4BIV001CaseVariantAloneRetainsOrdinaryPayloadSemantics(bool signature, bool required)
    {
        string variant = signature ? "META/SIGNATURE.JSON" : "META/INTEGRITY.JSON";
        ThemeIntegrityVerificationRequestV2 request = Request(File(variant, [1]) with { Required = required });
        RecordingReader presentReader = new([Entry(variant, 1)], new() { [variant] = [1] });
        ThemePackageInventoryEvaluation present = await Evaluate(request, presentReader);
        AssertCodes(present);
        Assert.Equal(ThemeIntegrityEvidenceStatus.Verified, Assert.Single(present.FileEvidence).Status);
        Assert.Equal([variant], presentReader.ReadPaths);
        RecordingReader absentReader = new([], []);
        ThemePackageInventoryEvaluation absent = await Evaluate(request, absentReader);
        AssertCodes(absent, required ? ["P4I005"] : []);
        Assert.Equal(required ? ThemeIntegrityEvidenceStatus.MissingRequired : ThemeIntegrityEvidenceStatus.MissingAllowed,
            Assert.Single(absent.FileEvidence).Status);
        Assert.Empty(absentReader.ReadPaths);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task P4BIV001MixedCollisionOutcomeIsDeterministicForFiftyRunsPerCultureAndOrder(
        bool signature, bool required)
    {
        CultureInfo previousCulture = CultureInfo.CurrentCulture;
        CultureInfo previousUiCulture = CultureInfo.CurrentUICulture;
        ThemePackageInventoryEvaluation? first = null;
        try
        {
            foreach (string culture in new[] { "en-US", "tr-TR", "zh-TW" })
            foreach (bool reverse in new[] { false, true })
            for (int run = 0; run < 50; run++)
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
                string reserved = signature ? "meta/signature.json" : "meta/integrity.json";
                string variant = reserved.ToUpperInvariant();
                ThemeIntegrityFileV2[] files =
                [
                    File(reserved, [1]) with { Required = required },
                    File(variant, [1]) with { Required = required },
                    File("A/file.txt", [1]), File("a/FILE.txt", [1]),
                    File("é.bin", [1]), File("e\u0301.bin", [1]),
                    File("absent.bin", [1]), File("verified.bin", [1]), File("../invalid", [1]),
                ];
                ThemePackageContentEntryV1[] entries =
                [Entry(variant, 1), Entry("A/file.txt", 1), Entry("extra.bin", 1),
                    Entry("verified.bin", 1), Entry("../invalid", 1)];
                if (reverse) { Array.Reverse(files); Array.Reverse(entries); }
                RecordingReader reader = new(entries, new() { [variant] = [1], ["verified.bin"] = [1] });
                ThemePackageInventoryEvaluation result = await Evaluate(Request(files), reader);
                AssertCodes(result, "P4I002", "P4I002", "P4I003", "P4I004", "P4I004", "P4I005", "P4I006", "P4I029");
                Assert.Equal(["A/file.txt", variant], result.Diagnostics.Where(item => item.Code == "P4I004")
                    .Select(item => item.CanonicalPath));
                Assert.Equal(["verified.bin"], reader.ReadPaths);
                Assert.Equal(["absent.bin", "verified.bin"], result.FileEvidence.Select(item => item.CanonicalPath));
                if (first is not null) AssertSame(first, result);
                first ??= result;
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }

    private static void AssertReservedRejection(ThemePackageInventoryEvaluation result)
    {
        AssertCodes(result, "P4I029");
        ThemeIntegrityDiagnosticV1 diagnostic = Assert.Single(result.Diagnostics);
        Assert.Null(diagnostic.CanonicalPath);
        Assert.Equal(ReservedMessage, diagnostic.Message);
        Assert.Empty(result.FileEvidence);
    }

    private static void AssertCodes(ThemePackageInventoryEvaluation result, params string[] codes)
    {
        Assert.Equal(codes.Length == 0, result.CanContinue);
        Assert.Equal(codes, result.Diagnostics.Select(item => item.Code));
        Assert.All(result.Diagnostics, item => Assert.Equal(ThemeDiagnosticsSeverity.Error, item.Severity));
    }

    private static void AssertSame(ThemePackageInventoryEvaluation expected, ThemePackageInventoryEvaluation actual)
    {
        Assert.Equal(expected.CanContinue, actual.CanContinue);
        Assert.Equal(expected.Diagnostics.ToArray(), actual.Diagnostics.ToArray());
        Assert.Equal(expected.FileEvidence.ToArray(), actual.FileEvidence.ToArray());
    }

    private static async Task<ThemePackageInventoryEvaluation> Evaluate(
        ThemeIntegrityVerificationRequestV2 request, RecordingReader reader)
    {
        reader.ExpectedPackage = request.PackageRef;
        ThemePackageInventoryEvaluation result = await ThemePackageInventoryEvaluator.EvaluateAsync(request, reader);
        Assert.Equal(1, reader.EnumerationCount);
        Assert.Equal(result.Diagnostics.OrderBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.CanonicalPath, StringComparer.Ordinal).ThenBy(item => item.Message, StringComparer.Ordinal),
            result.Diagnostics);
        Assert.Equal(result.FileEvidence.OrderBy(item => item.CanonicalPath, StringComparer.Ordinal), result.FileEvidence);
        return result;
    }

    private static Exception ReaderFailure(int kind) => kind switch
    {
        0 => new IOException("PRIVATE reader detail"),
        1 => new UnauthorizedAccessException("PRIVATE access detail"),
        2 => new FileNotFoundException("PRIVATE disappeared entry"),
        _ => new InvalidDataException("PRIVATE data detail"),
    };

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static ThemeIntegrityFileV2 File(string path, byte[] bytes) =>
        new(path, ThemeIntegrityEntryKind.Payload, Hash(bytes), bytes.LongLength, true);

    private static ThemePackageContentEntryV1 Entry(string path, long length) =>
        new(path, length, ThemePackageContentEntryKind.File, null);

    private static ThemeIntegrityVerificationRequestV2 Request(params ThemeIntegrityFileV2[] files) => new(
        new ThemeId("com.example.theme"), new ThemeVersion("1.2.3"), new ThemePackageRef("opaque:test-package"),
        ThemeCanonicalPath.Parse("theme.json"), ThemeCanonicalPath.Parse("meta/integrity.json"),
        ThemeCanonicalPath.Parse("meta/signature.json"),
        new ThemeIntegrityManifestV2(ContractVersions.ThemeIntegritySchemaV2,
            ThemeIntegrityInventoryMode.ExhaustiveAllowedPayload, "com.example.theme", "1.2.3", "sha256", files,
            new string('a', 64)),
        null,
        new ThemeIntegrityVerificationPolicyV1("beta", "1", "trust", "1", ThemeDistributionChannel.Beta,
            ThemeSignatureRequirement.Optional, false),
        new ThemeTrustSnapshotV1("trust", "1", []));

    private sealed class RecordingReader(
        IReadOnlyList<ThemePackageContentEntryV1> entries,
        Dictionary<string, byte[]> contents) : IThemePackageContentReader
    {
        public int EnumerationCount { get; private set; }
        public List<string> ReadPaths { get; } = [];
        public ThemePackageRef ExpectedPackage { get; set; } = new("opaque:test-package");
        public CancellationToken ExpectedToken { get; init; }
        public Exception? EnumerationFailure { get; init; }
        public Exception? ContentFailure { get; init; }
        public Func<Task>? BeforeEnumeration { get; init; }
        public Func<string, Task>? BeforeRead { get; init; }
        public Action<string>? AfterRead { get; init; }

        public async ValueTask<IReadOnlyList<ThemePackageContentEntryV1>> EnumerateEntriesAsync(
            ThemePackageRef packageRef, CancellationToken cancellationToken = default)
        {
            Assert.Equal(ExpectedPackage, packageRef);
            Assert.Equal(ExpectedToken, cancellationToken);
            Assert.Equal(0, EnumerationCount++);
            if (BeforeEnumeration is not null) await BeforeEnumeration();
            if (EnumerationFailure is not null) throw EnumerationFailure;
            return entries;
        }

        public async ValueTask<ReadOnlyMemory<byte>> ReadContentAsync(
            ThemePackageRef packageRef, string canonicalPath, CancellationToken cancellationToken = default)
        {
            Assert.Equal(ExpectedPackage, packageRef);
            Assert.Equal(ExpectedToken, cancellationToken);
            Assert.DoesNotContain(canonicalPath, ReadPaths); // A second read is a test failure, not a simulated success.
            Assert.Equal(canonicalPath.Normalize(NormalizationForm.FormC), canonicalPath);
            ReadPaths.Add(canonicalPath);
            if (BeforeRead is not null) await BeforeRead(canonicalPath);
            if (ContentFailure is not null) throw ContentFailure;
            if (!contents.TryGetValue(canonicalPath, out byte[]? bytes)) throw new FileNotFoundException("PRIVATE disappearance");
            AfterRead?.Invoke(canonicalPath);
            return bytes;
        }
    }
}
