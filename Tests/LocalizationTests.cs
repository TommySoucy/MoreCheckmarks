using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace MoreCheckmarks.Tests
{
    /// <summary>
    /// Localization keeps static state, so everything that selects a language shares one xunit
    /// collection and therefore runs serially.
    /// </summary>
    [CollectionDefinition(LocaleFiles.Collection)]
    public class LocalizationCollection
    {
    }

    /// <summary>
    /// Reads the language files exactly as they are shipped, so the tests check the real content
    /// rather than a copy that could drift.
    /// </summary>
    internal static class LocaleFiles
    {
        internal const string Collection = "Localization";

        private static readonly string folder = Path.Combine(AppContext.BaseDirectory, "Locales");

        internal static Dictionary<string, string> Read(string language)
        {
            return Localization.Parse(File.ReadAllText(Path.Combine(folder, language + ".json")));
        }

        internal static IEnumerable<string> Languages()
        {
            return Directory.GetFiles(folder, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .OrderBy(x => x);
        }

        /// <summary>Selects English, which the golden output tests assert against.</summary>
        internal static void UseEnglish()
        {
            Dictionary<string, string> english = Read(Localization.defaultLanguage);
            Localization.Load(english, english);
        }
    }

    /// <summary>
    /// Guards the promise that installing this change leaves an English player's tooltips byte for
    /// byte identical. Every expected string here is the literal the code produced before the text
    /// was moved into en.json; none of them may be "fixed up" without that being a deliberate,
    /// separate decision.
    /// </summary>
    [Collection(LocaleFiles.Collection)]
    public class EnglishOutputTests
    {
        public EnglishOutputTests()
        {
            LocaleFiles.UseEnglish();
        }

        [Fact]
        public void CountLines_MatchOriginal()
        {
            Assert.Equal("ON YOU", Localization.Get("tooltip.counts.onYou"));
            Assert.Equal(
                "STASH: <color=#dd831a>1</color> found in raid / 2 total",
                Localization.Format("tooltip.counts.line",
                    ("label", "STASH"), ("fir", 1), ("total", 2)));
        }

        [Fact]
        public void QuestItemLabel_MatchesOriginal()
        {
            Assert.Equal("QUEST ITEM", Localization.Get("label.questItem"));
        }

        [Fact]
        public void QuestNeededLines_MatchOriginal()
        {
            Assert.Equal(
                "\nNeeded (2/5) to start quest:\n  QUESTS",
                Localization.Plural("tooltip.quests.needed.start", 1,
                    ("possessed", 2), ("required", 5), ("quests", "QUESTS")));

            Assert.Equal(
                "\nNeeded (2/5) to start quests:\n  QUESTS",
                Localization.Plural("tooltip.quests.needed.start", 3,
                    ("possessed", 2), ("required", 5), ("quests", "QUESTS")));

            Assert.Equal(
                "\nNeeded (2/5) to complete quest:\n  QUESTS",
                Localization.Plural("tooltip.quests.needed.complete", 1,
                    ("possessed", 2), ("required", 5), ("quests", "QUESTS")));

            Assert.Equal(
                "\nNeeded (2/5) to complete quests:\n  QUESTS",
                Localization.Plural("tooltip.quests.needed.complete", 3,
                    ("possessed", 2), ("required", 5), ("quests", "QUESTS")));
        }

        [Fact]
        public void QuestListPieces_MatchOriginal()
        {
            Assert.Equal(",\n  ", Localization.Get("tooltip.quests.separator"));
            Assert.Equal("Unknown Quest", Localization.Get("tooltip.quests.unknownName"));
        }

        [Fact]
        public void PrerequisiteCounts_MatchOriginal()
        {
            Assert.Equal(
                " <color=#00ff00>(0 prereqs)</color>",
                Localization.Get("tooltip.quests.prereqs.none"));

            Assert.Equal(
                " <color=#ffff00>(1 prereq)</color>",
                Localization.Plural("tooltip.quests.prereqs.few", 1, ("count", 1)));

            Assert.Equal(
                " <color=#ffff00>(5 prereqs)</color>",
                Localization.Plural("tooltip.quests.prereqs.few", 5, ("count", 5)));

            Assert.Equal(
                " <color=#888888>(12 prereqs)</color>",
                Localization.Format("tooltip.quests.prereqs.many", ("count", 12)));
        }

        [Fact]
        public void HideoutLines_MatchOriginal()
        {
            Assert.Equal(
                "\nNeeded (1/4) for area:AREAS",
                Localization.Plural("tooltip.hideout.needed", 1,
                    ("possessed", 1), ("required", 4), ("areas", "AREAS")));

            Assert.Equal(
                "\nNeeded (1/4) for areas:AREAS",
                Localization.Plural("tooltip.hideout.needed", 2,
                    ("possessed", 1), ("required", 4), ("areas", "AREAS")));

            Assert.Equal(
                "\n  ENTRY",
                Localization.Format("tooltip.hideout.areaEntry", ("area", "ENTRY")));

            Assert.Equal(
                "<color=#4dff47>Generator lvl2</color>",
                Localization.Format("tooltip.hideout.area",
                    ("color", "4dff47"), ("area", "Generator"), ("level", 2)));
        }

        [Fact]
        public void WishlistLine_MatchesOriginal()
        {
            Assert.Equal(
                "\nOn <color=#3befff>Wish List</color>",
                Localization.Format("tooltip.wishlist.line", ("color", "3befff")));
        }

        [Fact]
        public void CraftLines_MatchOriginal()
        {
            Assert.Equal(
                "\nNeeded for crafting:RECIPES",
                Localization.Format("tooltip.craft.header", ("recipes", "RECIPES")));

            Assert.Equal(
                "\n  GENERATOR",
                Localization.Format("tooltip.craft.area", ("area", "GENERATOR")));

            Assert.Equal(
                "\n    <color=#00ffff>Physical Bitcoin lvl3</color> (2)",
                Localization.Format("tooltip.craft.recipe",
                    ("color", "00ffff"), ("item", "Physical Bitcoin"), ("level", 3), ("amount", 2)));
        }

        [Fact]
        public void BarterLines_MatchOriginal()
        {
            Assert.Equal(
                "\nBarter:",
                Localization.Format("tooltip.barter.header", ("barter", "Barter")));

            Assert.Equal(
                "\n With Prapor:",
                Localization.Format("tooltip.barter.trader", ("trader", "Prapor")));

            Assert.Equal(
                "Custom Trader 3",
                Localization.Format("tooltip.barter.customTrader", ("index", 3)));

            Assert.Equal(
                "\n  <color=#ff00ff>Salewa first aid kit</color> (4)",
                Localization.Format("tooltip.barter.offer",
                    ("color", "ff00ff"), ("item", "Salewa first aid kit"), ("amount", 4)));
        }
    }

    /// <summary>
    /// Structural checks every language file has to pass, so an incomplete or mistyped translation
    /// fails the build rather than showing up as broken text in game.
    /// </summary>
    [Collection(LocaleFiles.Collection)]
    public class LanguageFileTests
    {
        private static readonly Regex placeholderPattern = new Regex(@"(?<!\{)\{([A-Za-z][A-Za-z0-9]*)\}");

        // Looped rather than a [Theory] per language so the suite still passes when English is the
        // only file present, which is the state this mechanism landed in
        private static IEnumerable<string> Translations()
        {
            return LocaleFiles.Languages().Where(language => language != Localization.defaultLanguage);
        }

        [Fact]
        public void English_DeclaresItsPluralRule()
        {
            Assert.Equal("english", LocaleFiles.Read(Localization.defaultLanguage)["@plural"]);
        }

        [Fact]
        public void EveryTranslation_DeclaresAPluralRule()
        {
            foreach (string language in Translations())
            {
                Assert.True(LocaleFiles.Read(language).ContainsKey("@plural"),
                    language + ".json is missing the \"@plural\" entry");
            }
        }

        [Fact]
        public void EveryTranslation_HasExactlyTheEnglishKeys()
        {
            HashSet<string> english = DisplayKeys(LocaleFiles.Read(Localization.defaultLanguage));

            foreach (string language in Translations())
            {
                HashSet<string> translated = DisplayKeys(LocaleFiles.Read(language));

                Assert.True(english.SetEquals(translated),
                    language + ".json is missing [" + string.Join(", ", english.Except(translated).OrderBy(x => x)) +
                    "] and has unknown keys [" + string.Join(", ", translated.Except(english).OrderBy(x => x)) + "]");
            }
        }

        [Fact]
        public void EveryTranslation_KeepsEveryPlaceholder()
        {
            Dictionary<string, string> english = LocaleFiles.Read(Localization.defaultLanguage);

            foreach (string language in Translations())
            {
                Dictionary<string, string> translated = LocaleFiles.Read(language);

                foreach (KeyValuePair<string, string> entry in english)
                {
                    if (Localization.IsMetadata(entry.Key) ||
                        !translated.TryGetValue(entry.Key, out string value))
                    {
                        continue;
                    }

                    Assert.True(Placeholders(entry.Value).SequenceEqual(Placeholders(value)),
                        language + ".json has the wrong placeholders for " + entry.Key +
                        ": expected [" + string.Join(", ", Placeholders(entry.Value)) +
                        "] but found [" + string.Join(", ", Placeholders(value)) + "]");
                }
            }
        }

        [Fact]
        public void EveryTranslation_HasNoEmptyValue()
        {
            foreach (string language in Translations())
            {
                foreach (KeyValuePair<string, string> entry in LocaleFiles.Read(language))
                {
                    Assert.False(string.IsNullOrEmpty(entry.Value),
                        language + ".json has an empty value for " + entry.Key);
                }
            }
        }

        private static HashSet<string> DisplayKeys(Dictionary<string, string> strings)
        {
            return new HashSet<string>(strings.Keys.Where(key => !Localization.IsMetadata(key)));
        }

        private static string[] Placeholders(string value)
        {
            return placeholderPattern.Matches(value)
                .Cast<Match>()
                .Select(match => match.Groups[1].Value)
                .OrderBy(name => name)
                .ToArray();
        }
    }

    /// <summary>
    /// Deferred language selection. The game cannot say which language it runs in while the plugin
    /// is starting, so the resolver is asked again until it has an answer; until then English is
    /// what gets displayed. These drive the real files through Initialize.
    /// </summary>
    [Collection(LocaleFiles.Collection)]
    public class LanguageResolutionTests : IDisposable
    {
        // Written on the fly rather than using the shipped files, so these hold whatever translations
        // happen to exist in the repository
        private const string english = "{ \"greeting\": \"Hello\" }";
        private const string other = "{ \"@plural\": \"french\", \"greeting\": \"Bonjour\" }";

        private readonly List<string> folders = new List<string>();

        private string Folder()
        {
            string root = Path.Combine(Path.GetTempPath(), "MoreCheckmarksTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(root, "Locales"));
            File.WriteAllText(Path.Combine(root, "Locales", "en.json"), english);
            File.WriteAllText(Path.Combine(root, "Locales", "xx.json"), other);
            folders.Add(root);
            return root;
        }

        public void Dispose()
        {
            foreach (string folder in folders)
            {
                try
                {
                    Directory.Delete(folder, true);
                }
                catch (IOException)
                {
                    // A leftover temp folder is not worth failing a test over
                }
            }
        }

        [Fact]
        public void NoAnswerYet_KeepsEnglishAndAsksAgainLater()
        {
            string answer = null;
            int asked = 0;
            Localization.onLanguageLoaded = null;
            Localization.Initialize(Folder(), () => { asked++; return answer; });

            // The game is not ready: English, and the question stays open
            Assert.Equal("Hello", Localization.Get("greeting"));
            Assert.True(asked > 0);

            // Once it can answer, the next lookup picks the translation up
            answer = "xx";
            Assert.Equal("Bonjour", Localization.Get("greeting"));
        }

        [Fact]
        public void ResolvedLanguage_LoadsOnceAndNotifies()
        {
            int notified = 0;
            int asked = 0;
            Localization.onLanguageLoaded = () => notified++;
            Localization.Initialize(Folder(), () => { asked++; return "xx"; });

            Assert.Equal("Bonjour", Localization.Get("greeting"));
            Assert.Equal(1, notified);

            Localization.Get("greeting");
            Assert.Equal(1, notified);
            Assert.Equal(1, asked);
        }

        [Fact]
        public void RefreshLanguage_CanBeCalledBeforeAnyLookup()
        {
            Localization.onLanguageLoaded = null;
            Localization.Initialize(Folder(), () => "xx");
            Localization.RefreshLanguage();

            Assert.Equal("Bonjour", Localization.Get("greeting"));
            Assert.Equal("xx", Localization.loadedLanguage);
        }

        [Fact]
        public void UnknownLanguage_FallsBackToEnglish()
        {
            Localization.onLanguageLoaded = null;
            Localization.Initialize(Folder(), () => "zz");

            Assert.Equal("Hello", Localization.Get("greeting"));
            Assert.Equal(Localization.defaultLanguage, Localization.loadedLanguage);
        }

        [Fact]
        public void ThrowingResolver_FallsBackToEnglishAndStopsAsking()
        {
            int asked = 0;
            Localization.onLanguageLoaded = null;
            Localization.Initialize(Folder(),
                () => { asked++; throw new InvalidOperationException("locale manager not there"); });

            Assert.Equal("Hello", Localization.Get("greeting"));
            Localization.Get("greeting");
            Assert.Equal(1, asked);
        }

        [Fact]
        public void English_NotifiesSoLabelsStillRefresh()
        {
            int notified = 0;
            Localization.onLanguageLoaded = () => notified++;
            Localization.Initialize(Folder(), () => "en");

            Localization.Get("greeting");
            Assert.Equal(1, notified);
        }
    }

    /// <summary>
    /// The lookup and formatting rules themselves, driven by hand built dictionaries so they do not
    /// depend on the shipped files.
    /// </summary>
    [Collection(LocaleFiles.Collection)]
    public class LocalizationLookupTests
    {
        private static void Load(Dictionary<string, string> selected, Dictionary<string, string> english)
        {
            Localization.Load(selected, english);
        }

        [Fact]
        public void MissingKey_FallsBackToEnglish()
        {
            Load(new Dictionary<string, string>(),
                new Dictionary<string, string> { ["greeting"] = "Hello" });

            Assert.Equal("Hello", Localization.Get("greeting"));
        }

        [Fact]
        public void MissingEverywhere_ReturnsTheKeyRatherThanNothing()
        {
            Load(new Dictionary<string, string>(), new Dictionary<string, string>());

            Assert.Equal("nope", Localization.Get("nope"));
        }

        [Fact]
        public void EmptyTranslation_FallsBackToEnglish()
        {
            Load(new Dictionary<string, string> { ["greeting"] = "" },
                new Dictionary<string, string> { ["greeting"] = "Hello" });

            Assert.Equal("Hello", Localization.Get("greeting"));
        }

        [Fact]
        public void GetEnglish_IgnoresTheSelectedLanguage()
        {
            Load(new Dictionary<string, string> { ["greeting"] = "Bonjour" },
                new Dictionary<string, string> { ["greeting"] = "Hello" });

            Assert.Equal("Bonjour", Localization.Get("greeting"));
            Assert.Equal("Hello", Localization.GetEnglish("greeting"));
        }

        [Fact]
        public void Placeholders_AreReplacedByName()
        {
            Load(new Dictionary<string, string> { ["line"] = "{a} then {b}" },
                new Dictionary<string, string>());

            Assert.Equal("first then second",
                Localization.Format("line", ("b", "second"), ("a", "first")));
        }

        [Fact]
        public void Placeholders_CanBeReordered()
        {
            var english = new Dictionary<string, string> { ["line"] = "{count} of {total}" };
            var french = new Dictionary<string, string> { ["line"] = "sur {total}, {count}" };

            Load(french, english);

            Assert.Equal("sur 7, 3", Localization.Format("line", ("count", 3), ("total", 7)));
        }

        [Fact]
        public void UnknownPlaceholder_IsLeftVisible()
        {
            Load(new Dictionary<string, string> { ["line"] = "{known} and {unknown}" },
                new Dictionary<string, string>());

            Assert.Equal("here and {unknown}", Localization.Format("line", ("known", "here")));
        }

        [Fact]
        public void DoubleBrace_WritesALiteralBrace()
        {
            Load(new Dictionary<string, string> { ["line"] = "{{not a placeholder}} {real}" },
                new Dictionary<string, string>());

            Assert.Equal("{not a placeholder} value", Localization.Format("line", ("real", "value")));
        }

        [Fact]
        public void UnbalancedBrace_IsKeptAsWritten()
        {
            Load(new Dictionary<string, string> { ["line"] = "broken {oops" },
                new Dictionary<string, string>());

            Assert.Equal("broken {oops", Localization.Format("line"));
        }

        [Theory]
        [InlineData(0, "other")]
        [InlineData(1, "one")]
        [InlineData(2, "other")]
        public void EnglishPluralRule_SplitsAtOne(int count, string expected)
        {
            Load(new Dictionary<string, string>
            {
                ["@plural"] = "english",
                ["n.one"] = "one",
                ["n.other"] = "other"
            }, new Dictionary<string, string>());

            Assert.Equal(expected, Localization.Plural("n", count));
        }

        [Theory]
        [InlineData(0, "one")]
        [InlineData(1, "one")]
        [InlineData(2, "other")]
        public void FrenchPluralRule_KeepsTheSingularForZero(int count, string expected)
        {
            Load(new Dictionary<string, string>
            {
                ["@plural"] = "french",
                ["n.one"] = "one",
                ["n.other"] = "other"
            }, new Dictionary<string, string>());

            Assert.Equal(expected, Localization.Plural("n", count));
        }

        [Fact]
        public void UnknownPluralRule_BehavesLikeEnglish()
        {
            Load(new Dictionary<string, string>
            {
                ["@plural"] = "klingon",
                ["n.one"] = "one",
                ["n.other"] = "other"
            }, new Dictionary<string, string>());

            Assert.Equal("one", Localization.Plural("n", 1));
            Assert.Equal("other", Localization.Plural("n", 4));
        }

        [Fact]
        public void Parse_KeepsMetadataAndSkipsNonStrings()
        {
            Dictionary<string, string> parsed = Localization.Parse(
                "{ \"@plural\": \"french\", \"a\": \"text\", \"b\": 12, \"c\": null }");

            Assert.Equal("french", parsed["@plural"]);
            Assert.Equal("text", parsed["a"]);
            Assert.False(parsed.ContainsKey("b"));
            Assert.False(parsed.ContainsKey("c"));
        }

        [Fact]
        public void IsMetadata_OnlyMatchesTheAtPrefix()
        {
            Assert.True(Localization.IsMetadata("@plural"));
            Assert.False(Localization.IsMetadata("tooltip.counts.onYou"));
        }
    }
}
