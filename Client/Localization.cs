using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace MoreCheckmarks
{
    /// <summary>
    /// The mod's own display strings, read from Locales/{language}.json next to the DLL.
    ///
    /// Text that belongs to the game (item names, quest names, "STASH", "Barter", ...) is not
    /// handled here and keeps going through the game's own Localized() extension. This only covers
    /// the text MoreCheckmarks writes itself.
    ///
    /// No EFT/Unity dependencies so this can be unit-tested in the Tests project.
    /// </summary>
    public static class Localization
    {
        /// <summary>Language that is always fully populated and used whenever a key is missing.</summary>
        public const string defaultLanguage = "en";

        /// <summary>Value of the language setting meaning "use whatever the game is running in".</summary>
        public const string automaticLanguage = "Auto";

        // Entries whose key starts with this are file metadata (plural rule, translator notes),
        // never display strings
        private const char metadataPrefix = '@';
        private const string pluralRuleKey = "@plural";
        private const string defaultPluralRule = "english";

        private const string localesFolderName = "Locales";
        private const string embeddedEnglishName = "MoreCheckmarks.Locales.en.json";

        private static Dictionary<string, string> strings = new Dictionary<string, string>();
        private static Dictionary<string, string> englishStrings = new Dictionary<string, string>();
        private static string pluralRule = defaultPluralRule;
        private static Action<string> logError;

        // Language selection is deferred: the game's locale manager does not exist yet when the
        // plugin starts, so asking it then answers nothing. Instead English is loaded immediately
        // and the real language is resolved on first use, or when a profile is selected.
        private static Func<string> languageResolver;
        private static bool languageResolved;

        /// <summary>Raised once the selected language has been loaded, so labels bound earlier can refresh.</summary>
        public static Action onLanguageLoaded;

        /// <summary>The language currently in use. English until the wanted one has been resolved.</summary>
        public static string loadedLanguage { get; private set; } = defaultLanguage;

        private static string localesPath = "";

        // Keys already reported, so one missing entry doesn't spam the log on every mouse over
        private static readonly HashSet<string> reportedMissingKeys = new HashSet<string>();

        /// <summary>
        /// Reads English from the files sitting next to the DLL, and takes a resolver that will be
        /// asked for the wanted language later on. English is always kept loaded, so a partial
        /// translation falls back to it for the keys it is missing. Never throws.
        /// </summary>
        public static void Initialize(string modPath, Func<string> resolver, Action<string> onError = null)
        {
            logError = onError;
            languageResolver = resolver;
            languageResolved = false;
            localesPath = Path.Combine(modPath, localesFolderName);

            // English comes out of the assembly first so it is there even if the file was deleted or
            // corrupted, then the file on disk is layered on top so it can be corrected without a rebuild
            Dictionary<string, string> english = ReadEmbeddedEnglish();
            Overlay(english, ReadFile(Path.Combine(localesPath, defaultLanguage + ".json")));

            strings = english;
            englishStrings = english;
            pluralRule = defaultPluralRule;
            loadedLanguage = defaultLanguage;
            reportedMissingKeys.Clear();
        }

        /// <summary>
        /// Asks the resolver for the language and loads it if it has an answer. Safe to call as often
        /// as wanted: it does nothing once a language has been loaded, and nothing while the resolver
        /// still has no answer to give.
        /// </summary>
        public static void RefreshLanguage()
        {
            if (languageResolved || languageResolver == null)
            {
                return;
            }

            string language;
            try
            {
                language = languageResolver();
            }
            catch (Exception ex)
            {
                logError?.Invoke("Failed to resolve the language: " + ex.Message);
                languageResolved = true;
                return;
            }

            // No answer yet, the game is not far enough along. Leave English in place and try again
            // on the next lookup rather than settling for the wrong language.
            if (string.IsNullOrEmpty(language))
            {
                return;
            }

            languageResolved = true;

            if (string.Equals(language, defaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                onLanguageLoaded?.Invoke();
                return;
            }

            Dictionary<string, string> translation = ReadFile(Path.Combine(localesPath, language + ".json"));
            if (translation == null)
            {
                logError?.Invoke("No language file for '" + language + "', using " + defaultLanguage);
            }
            else
            {
                Load(translation, englishStrings);
                loadedLanguage = language;
            }

            onLanguageLoaded?.Invoke();
        }

        /// <summary>
        /// Installs an already parsed pair of dictionaries. Kept separate from Initialize so lookups
        /// and formatting can be tested without touching the file system.
        /// </summary>
        public static void Load(Dictionary<string, string> selected, Dictionary<string, string> english)
        {
            strings = selected ?? new Dictionary<string, string>();
            englishStrings = english ?? new Dictionary<string, string>();
            reportedMissingKeys.Clear();

            // Installing a language directly settles the question, nothing left to resolve
            languageResolved = true;

            pluralRule = strings.TryGetValue(pluralRuleKey, out string rule) && !string.IsNullOrEmpty(rule)
                ? rule
                : defaultPluralRule;
        }

        /// <summary>
        /// The string for <paramref name="key"/> in the selected language, falling back to English
        /// and then to the key itself. Never returns null or an empty string.
        /// </summary>
        public static string Get(string key)
        {
            // Catches the case where nothing triggered the resolution earlier
            RefreshLanguage();

            if (strings.TryGetValue(key, out string value) && !string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (englishStrings.TryGetValue(key, out value) && !string.IsNullOrEmpty(value))
            {
                return value;
            }

            // Showing the key is ugly, but it beats an empty tooltip and points straight at what to fix
            if (reportedMissingKeys.Add(key))
            {
                logError?.Invoke("Missing localization key: " + key);
            }

            return key;
        }

        /// <summary>
        /// The English string for <paramref name="key"/>, whatever language is selected. Used for the
        /// text written to the BepInEx .cfg file, which stays English so existing files keep reading
        /// the same way.
        /// </summary>
        public static string GetEnglish(string key)
        {
            if (englishStrings.TryGetValue(key, out string value) && !string.IsNullOrEmpty(value))
            {
                return value;
            }

            return Get(key);
        }

        /// <summary>
        /// The string for <paramref name="key"/> with its {named} placeholders filled in. A
        /// placeholder with no matching argument is left as written rather than throwing.
        /// </summary>
        public static string Format(string key, params (string name, object value)[] arguments)
        {
            return Fill(Get(key), arguments);
        }

        /// <summary>
        /// Like Format, but picks between the "{key}.one" and "{key}.other" entries following the
        /// plural rule the language file declares. A language that does not split where English does
        /// can therefore use one wording for both, or word the count differently.
        /// </summary>
        public static string Plural(string key, int count, params (string name, object value)[] arguments)
        {
            return Fill(Get(key + "." + PluralSuffix(count)), arguments);
        }

        /// <summary>Whether a key is file metadata rather than a display string.</summary>
        public static bool IsMetadata(string key)
        {
            return !string.IsNullOrEmpty(key) && key[0] == metadataPrefix;
        }

        /// <summary>
        /// Parses a language file into key/value pairs. Anything that is not a string value is
        /// skipped, so a malformed entry costs one key rather than the whole file.
        /// </summary>
        public static Dictionary<string, string> Parse(string json)
        {
            var result = new Dictionary<string, string>();

            foreach (KeyValuePair<string, JToken> entry in JObject.Parse(json))
            {
                if (entry.Value != null && entry.Value.Type == JTokenType.String)
                {
                    result[entry.Key] = entry.Value.ToString();
                }
            }

            return result;
        }

        /// <summary>
        /// The language codes that have a file in the Locales folder, English first. Used to build
        /// the list the config menu offers.
        /// </summary>
        public static List<string> AvailableLanguages(string modPath)
        {
            var result = new List<string> { defaultLanguage };

            try
            {
                string localesPath = Path.Combine(modPath, localesFolderName);
                if (Directory.Exists(localesPath))
                {
                    foreach (string file in Directory.GetFiles(localesPath, "*.json"))
                    {
                        string code = Path.GetFileNameWithoutExtension(file);
                        if (!result.Contains(code))
                        {
                            result.Add(code);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logError?.Invoke("Failed to list language files: " + ex.Message);
            }

            return result;
        }

        private static string PluralSuffix(int count)
        {
            switch (pluralRule)
            {
                // French keeps the singular for zero as well as for one
                case "french":
                    return count < 2 ? "one" : "other";
                default:
                    return count == 1 ? "one" : "other";
            }
        }

        /// <summary>
        /// Replaces the {name} placeholders in <paramref name="template"/>. "{{" and "}}" write a
        /// literal "{" and "}".
        /// </summary>
        private static string Fill(string template, (string name, object value)[] arguments)
        {
            if (string.IsNullOrEmpty(template) || template.IndexOf('{') < 0)
            {
                return template;
            }

            var result = new StringBuilder(template.Length + 16);

            for (int i = 0; i < template.Length; ++i)
            {
                if (template[i] == '}')
                {
                    // A doubled closing brace is an escape, a lone one is just text
                    if (i + 1 < template.Length && template[i + 1] == '}')
                    {
                        ++i;
                    }

                    result.Append('}');
                    continue;
                }

                if (template[i] != '{')
                {
                    result.Append(template[i]);
                    continue;
                }

                if (i + 1 < template.Length && template[i + 1] == '{')
                {
                    result.Append('{');
                    ++i;
                    continue;
                }

                int end = template.IndexOf('}', i + 1);
                if (end < 0)
                {
                    // Unbalanced brace, keep the rest of the string as written
                    result.Append(template, i, template.Length - i);
                    break;
                }

                string name = template.Substring(i + 1, end - i - 1);
                if (TryGetArgument(arguments, name, out string value))
                {
                    result.Append(value);
                }
                else
                {
                    // Leave an unknown placeholder visible so a bad translation can be spotted
                    result.Append(template, i, end - i + 1);
                }

                i = end;
            }

            return result.ToString();
        }

        private static bool TryGetArgument((string name, object value)[] arguments, string name, out string value)
        {
            if (arguments != null)
            {
                for (int i = 0; i < arguments.Length; ++i)
                {
                    if (string.Equals(arguments[i].name, name, StringComparison.Ordinal))
                    {
                        // Invariant culture so numbers read the same whatever the machine is set to
                        value = Convert.ToString(arguments[i].value, CultureInfo.InvariantCulture) ?? "";
                        return true;
                    }
                }
            }

            value = null;
            return false;
        }

        private static Dictionary<string, string> ReadFile(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return null;
                }

                return Parse(File.ReadAllText(path, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                logError?.Invoke("Failed to read language file " + path + ": " + ex.Message);
                return null;
            }
        }

        private static Dictionary<string, string> ReadEmbeddedEnglish()
        {
            try
            {
                using (Stream stream = Assembly.GetExecutingAssembly()
                           .GetManifestResourceStream(embeddedEnglishName))
                {
                    if (stream == null)
                    {
                        return new Dictionary<string, string>();
                    }

                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        return Parse(reader.ReadToEnd());
                    }
                }
            }
            catch (Exception ex)
            {
                logError?.Invoke("Failed to read the embedded English strings: " + ex.Message);
                return new Dictionary<string, string>();
            }
        }

        private static void Overlay(Dictionary<string, string> target, Dictionary<string, string> source)
        {
            if (source == null)
            {
                return;
            }

            foreach (KeyValuePair<string, string> entry in source)
            {
                target[entry.Key] = entry.Value;
            }
        }
    }
}
