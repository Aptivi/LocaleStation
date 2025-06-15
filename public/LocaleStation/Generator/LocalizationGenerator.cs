//
// LocaleStation  Copyright (C) 2025  Aptivi
//
// This file is part of LocaleStation
//
// LocaleStation is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// LocaleStation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using LocaleStation.Generator.Tools;
using LocaleStation.Instances;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LocaleStation.Generator
{
    [Generator]
    public class LocalizationGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Read configuration to decide the localization
            var localizationTexts = context.AdditionalTextsProvider;
            var localizationConfigs = context.AnalyzerConfigOptionsProvider;
            var localizationCompilation = context.CompilationProvider;
            var localizationLanguageFiles = localizationTexts
                .Combine(localizationConfigs)
                .Select((pair, ctx) =>
                {
                    var rootNamespace =
                        pair.Right.GlobalOptions.TryGetValue("build_property.RootNamespace", out var nameSpace) ?
                        nameSpace : "";
                    bool noCultureCheck =
                        pair.Right.GlobalOptions.TryGetValue("build_property.AptLocDisableInvalidCultureWarnings", out var indicator) &&
                        indicator.Equals("true", StringComparison.OrdinalIgnoreCase);
                    var fileOptions = pair.Right.GetOptions(pair.Left);
                    bool disabled =
                        fileOptions.TryGetValue("build_metadata.AdditionalFiles.AptLocDisableLocalization", out var disabledIndicator) &&
                        disabledIndicator.Equals("true", StringComparison.OrdinalIgnoreCase);
                    return fileOptions.TryGetValue("build_metadata.AdditionalFiles.AptLocIsLanguagePath", out var languagePathIndicator) ?
                        (languagePathIndicator.Equals("true", StringComparison.OrdinalIgnoreCase) && !disabled, pair.Left, rootNamespace, noCultureCheck) :
                        (false, pair.Left, rootNamespace, noCultureCheck);
                });

            // Register source output according to the configured language files and their metadata
            Dictionary<string, string> totalLanguages = [];
            List<string> totalLocalizationNames = [];
            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures).Select((ci) => ci.Name).ToArray();
            context.RegisterSourceOutput(localizationLanguageFiles.Combine(localizationCompilation), (ctx, pair) =>
            {
                var languageFileInfo = pair.Left;
                bool isLocalization = languageFileInfo.Item1;
                if (!isLocalization)
                    return;

                // Determine the root namespace from either the build property or, in very rare cases, assembly name
                string namespaceName =
                    !string.IsNullOrEmpty(languageFileInfo.rootNamespace) ?
                    languageFileInfo.rootNamespace : pair.Right.AssemblyName ?? "";
                if (string.IsNullOrEmpty(namespaceName))
                    return;

                // Ensure that appropriate files are generated with appropriate IDs
                var localizationText = languageFileInfo.Left.GetText();
                if (localizationText == null)
                {
                    var diag = LocalizationDiagnostics.GetLocalizationReadErrorDiagnostic(languageFileInfo.Left.Path);
                    ctx.ReportDiagnostic(diag);
                    return;
                }
                var localizationTextStr = localizationText.ToString();
                var localizationJson = JsonConvert.DeserializeObject<LanguageInfo?>(localizationTextStr);
                if (localizationJson is null)
                {
                    var diag = LocalizationDiagnostics.GetLocalizationStructureErrorDiagnostic(languageFileInfo.Left.Path);
                    ctx.ReportDiagnostic(diag);
                    return;
                }

                // Ensure that the culture IDs are not malformed
                bool noCheckCulture = languageFileInfo.noCultureCheck;
                List<string> processedCultures = [.. localizationJson.Cultures];
                if (!noCheckCulture)
                {
                    // Check the cultures
                    foreach (var culture in localizationJson.Cultures)
                    {
                        if (!cultures.Contains(culture))
                        {
                            processedCultures.Remove(culture);
                            var diag = LocalizationDiagnostics.GetLocalizationCultureNotFoundWarningDiagnostic(culture, languageFileInfo.Left.Path);
                            ctx.ReportDiagnostic(diag);
                        }
                    }
                }

                // Get the information to build the appropriate class for each language
                string header =
                    $$"""
                    /*
                        This file is automatically generated by LocaleStation for the {{localizationJson.Name}} language
                     */
                
                    // <auto-generated/>

                    namespace {{namespaceName}}.Localized
                    {
                        internal static class LocalStrings_{{GeneratorTools.NeutralizeName(localizationJson.Language)}}
                        {

                    """;
                string footer =
                    $$"""
                        }
                    }
                    """;
                var langClassBuilder = new StringBuilder(header);
                if (!totalLanguages.ContainsKey(localizationJson.Language))
                    totalLanguages.Add(localizationJson.Language, localizationJson.Name);

                // First, convert the individual strings to their compilable representations using constant fields
                var strings = localizationJson.Localizations;
                List<string> processedLocalizationNames = [];
                foreach (var str in strings)
                {
                    string finalLocalizationName = str.Localization.Replace("-", "_").ToUpper();
                    int repetitions = 1;
                    while (processedLocalizationNames.Contains(finalLocalizationName))
                    {
                        repetitions++;
                        finalLocalizationName = str.Localization.ToUpper() + $"_{repetitions}";
                    }
                    processedLocalizationNames.Add(finalLocalizationName);
                    if (!totalLocalizationNames.Contains(finalLocalizationName))
                        totalLocalizationNames.Add(finalLocalizationName);
                    langClassBuilder.AppendLine($"        private const string {finalLocalizationName} = @\"{str.Text.Replace("\"", "\"\"")}\";");
                }
                langClassBuilder.AppendLine();

                // Then, add convenience function to make things easier
                langClassBuilder.AppendLine("        internal static string GetLocalizedString(string loc)");
                langClassBuilder.AppendLine("        {");
                langClassBuilder.AppendLine("            switch (loc)");
                langClassBuilder.AppendLine("            {");
                for (int i = 0; i < processedLocalizationNames.Count; i++)
                {
                    var locName = processedLocalizationNames[i];

                    // Add code that detects the localization
                    langClassBuilder.AppendLine($"                case \"{locName}\":");
                    langClassBuilder.AppendLine($"                    return {locName};");
                }
                langClassBuilder.AppendLine("            }");
                langClassBuilder.AppendLine("            return loc;");
                langClassBuilder.AppendLine("        }");
                langClassBuilder.AppendLine();

                // Add a function that determines whether the given localization is found or not
                langClassBuilder.AppendLine("        internal static bool HasLocalization(string loc)");
                langClassBuilder.AppendLine("        {");
                langClassBuilder.AppendLine("            return");
                if (processedLocalizationNames.Count == 0)
                    langClassBuilder.AppendLine("                false");
                for (int i = 0; i < processedLocalizationNames.Count; i++)
                {
                    var locName = processedLocalizationNames[i];

                    // Add code that detects the localization
                    langClassBuilder.Append($"                loc == \"{locName}\"");
                    if (i == processedLocalizationNames.Count - 1)
                        langClassBuilder.AppendLine();
                    else
                        langClassBuilder.AppendLine(" ||");
                }
                langClassBuilder.AppendLine("            ;");
                langClassBuilder.AppendLine("        }");

                // Add a function that determines whether the culture is for this language or not
                langClassBuilder.AppendLine("        internal static bool CheckCulture(string culture)");
                langClassBuilder.AppendLine("        {");
                langClassBuilder.AppendLine("            return");
                if (processedCultures.Count == 0)
                    langClassBuilder.AppendLine("                false");
                for (int i = 0; i < processedCultures.Count; i++)
                {
                    var locName = processedCultures[i];

                    // Add code that detects the localization
                    langClassBuilder.Append($"                culture == \"{locName}\"");
                    if (i == processedCultures.Count - 1)
                        langClassBuilder.AppendLine();
                    else
                        langClassBuilder.AppendLine(" ||");
                }
                langClassBuilder.AppendLine("            ;");
                langClassBuilder.AppendLine("        }");

                // Finally, register the output
                langClassBuilder.AppendLine(footer);
                ctx.AddSource($"LocalStrings.{localizationJson.Language}.g.cs", SourceText.From(langClassBuilder.ToString(), Encoding.UTF8));
            });

            // Finally, make a static class
            context.RegisterSourceOutput(localizationConfigs, (ctx, options) =>
            {
                // Determine the root namespace from the build property
                if (!options.GlobalOptions.TryGetValue("build_property.RootNamespace", out var namespaceName) || string.IsNullOrEmpty(namespaceName))
                    return;

                // Get the information to build the appropriate class for each language
                string header =
                    $$"""
                    /*
                        This file is automatically generated by LocaleStation
                     */
                
                    // <auto-generated/>

                    using System.Collections.Generic;
                    using System.Linq;

                    namespace {{namespaceName}}.Localized
                    {
                        /// <summary>
                        /// Localized string tools
                        /// </summary>
                        public static class LocalStrings
                        {

                    """;
                string footer =
                    $$"""
                        }
                    }
                    """;
                var langClassBuilder = new StringBuilder(header);

                // Then, add convenience property that allows querying languages
                langClassBuilder.AppendLine("        /// <summary>");
                langClassBuilder.AppendLine("        /// Queries the languages");
                langClassBuilder.AppendLine("        /// </summary>");
                langClassBuilder.AppendLine("        public static Dictionary<string, string> Languages => new Dictionary<string, string>");
                langClassBuilder.AppendLine("        {");
                foreach (var lang in totalLanguages)
                    langClassBuilder.AppendLine($"            {{ \"{lang.Key}\", \"{lang.Value}\" }},");
                langClassBuilder.AppendLine("        };");
                langClassBuilder.AppendLine();

                // Then, add convenience property that allows querying localizations
                langClassBuilder.AppendLine("        /// <summary>");
                langClassBuilder.AppendLine("        /// Queries the localizations");
                langClassBuilder.AppendLine("        /// </summary>");
                langClassBuilder.AppendLine("        public static string[] Localizations => new string[]");
                langClassBuilder.AppendLine("        {");
                foreach (var loc in totalLocalizationNames)
                    langClassBuilder.AppendLine($"            \"{loc}\",");
                langClassBuilder.AppendLine("        };");
                langClassBuilder.AppendLine();

                // Add a function that performs the translation. We don't use reflection here for AOT compatibility
                langClassBuilder.AppendLine("        /// <summary>");
                langClassBuilder.AppendLine("        /// Translates the given string using the string ID in a specific language");
                langClassBuilder.AppendLine("        /// </summary>");
                langClassBuilder.AppendLine("        /// <param name=\"id\">String ID that represents a localization</param>");
                langClassBuilder.AppendLine("        /// <param name=\"lang\">Language to translate to</param>");
                langClassBuilder.AppendLine("        /// <returns>A translated string</returns>");
                langClassBuilder.AppendLine("        public static string Translate(string id, string lang)");
                langClassBuilder.AppendLine("        {");
                langClassBuilder.AppendLine("            switch (lang)");
                langClassBuilder.AppendLine("            {");
                foreach (var locName in totalLanguages)
                {
                    // Add code that performs the localization
                    langClassBuilder.AppendLine($"                case \"{locName.Key}\":");
                    langClassBuilder.AppendLine($"                    return LocalStrings_{GeneratorTools.NeutralizeName(locName.Key)}.GetLocalizedString(id);");
                }
                langClassBuilder.AppendLine("                default:");
                langClassBuilder.AppendLine("                    return $\"{lang}_{id}\";");
                langClassBuilder.AppendLine("            }");
                langClassBuilder.AppendLine("        }");
                langClassBuilder.AppendLine();

                // Add a function that checks the localization. We don't use reflection here for AOT compatibility
                langClassBuilder.AppendLine("        /// <summary>");
                langClassBuilder.AppendLine("        /// Checks to see if the given string using the string ID in a specific language exists");
                langClassBuilder.AppendLine("        /// </summary>");
                langClassBuilder.AppendLine("        /// <param name=\"id\">String ID that represents a localization</param>");
                langClassBuilder.AppendLine("        /// <param name=\"lang\">Language to translate to</param>");
                langClassBuilder.AppendLine("        /// <returns>True if exists; false otherwise</returns>");
                langClassBuilder.AppendLine("        public static bool Exists(string id, string lang)");
                langClassBuilder.AppendLine("        {");
                langClassBuilder.AppendLine("            switch (lang)");
                langClassBuilder.AppendLine("            {");
                foreach (var locName in totalLanguages)
                {
                    // Add code that detects the localization
                    langClassBuilder.AppendLine($"                case \"{locName.Key}\":");
                    langClassBuilder.AppendLine($"                    return LocalStrings_{GeneratorTools.NeutralizeName(locName.Key)}.HasLocalization(id);");
                }
                langClassBuilder.AppendLine("                default:");
                langClassBuilder.AppendLine("                    return false;");
                langClassBuilder.AppendLine("            }");
                langClassBuilder.AppendLine("        }");
                langClassBuilder.AppendLine();

                // Add a function that checks the culture. We don't use reflection here for AOT compatibility
                langClassBuilder.AppendLine("        /// <summary>");
                langClassBuilder.AppendLine("        /// Checks to see if the given culture in a specific language exists");
                langClassBuilder.AppendLine("        /// </summary>");
                langClassBuilder.AppendLine("        /// <param name=\"culture\">Culture ID</param>");
                langClassBuilder.AppendLine("        /// <param name=\"lang\">Language to process</param>");
                langClassBuilder.AppendLine("        /// <returns>True if exists; false otherwise</returns>");
                langClassBuilder.AppendLine("        public static bool CheckCulture(string culture, string lang)");
                langClassBuilder.AppendLine("        {");
                langClassBuilder.AppendLine("            switch (lang)");
                langClassBuilder.AppendLine("            {");
                foreach (var locName in totalLanguages)
                {
                    // Add code that checks for culture
                    langClassBuilder.AppendLine($"                case \"{locName.Key}\":");
                    langClassBuilder.AppendLine($"                    return LocalStrings_{GeneratorTools.NeutralizeName(locName.Key)}.CheckCulture(culture);");
                }
                langClassBuilder.AppendLine("                default:");
                langClassBuilder.AppendLine("                    return false;");
                langClassBuilder.AppendLine("            }");
                langClassBuilder.AppendLine("        }");
                langClassBuilder.AppendLine();

                // Add a function that lists languages by culture. We don't use reflection here for AOT compatibility
                langClassBuilder.AppendLine("        /// <summary>");
                langClassBuilder.AppendLine("        /// Lists languages in a given culture");
                langClassBuilder.AppendLine("        /// </summary>");
                langClassBuilder.AppendLine("        /// <param name=\"culture\">Culture ID</param>");
                langClassBuilder.AppendLine("        /// <returns>A list of languages</returns>");
                langClassBuilder.AppendLine("        public static string[] ListLanguagesCulture(string culture)");
                langClassBuilder.AppendLine("        {");
                langClassBuilder.AppendLine("            List<string> processedLanguages = new List<string>();");
                langClassBuilder.AppendLine("            foreach (var lang in Languages)");
                langClassBuilder.AppendLine("            {");
                langClassBuilder.AppendLine("                if (CheckCulture(culture, lang.Key))");
                langClassBuilder.AppendLine("                    processedLanguages.Add(lang.Key);");
                langClassBuilder.AppendLine("            }");
                langClassBuilder.AppendLine("            return processedLanguages.ToArray();");
                langClassBuilder.AppendLine("        }");

                // Finally, register the output
                langClassBuilder.AppendLine(footer);
                ctx.AddSource($"LocalStrings.g.cs", SourceText.From(langClassBuilder.ToString(), Encoding.UTF8));
            });
        }
    }
}
