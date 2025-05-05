//
// Localizer  Copyright (C) 2025  Aptivi
//
// This file is part of Localizer
//
// Localizer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Localizer is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using Localizer.Instances;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace Localizer.Generator
{
    [Generator]
    public class LocalizationGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            Debugger.Launch();

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
                    return pair.Right.GetOptions(pair.Left).TryGetValue("build_metadata.AdditionalFiles.AptLocIsLanguagePath", out var languagePathIndicator) ?
                        (languagePathIndicator.Equals("true", StringComparison.OrdinalIgnoreCase), pair.Left, rootNamespace) :
                        (false, pair.Left, rootNamespace);
                });

            // Register source output according to the configured language files and their metadata
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
                    return;
                }
                var localizationTextStr = localizationText.ToString();
                var localizationJson = JsonConvert.DeserializeObject<LanguageInfo>(localizationTextStr);

                // !!! UNFINISHED IMPLEMENTATION !!!
            });
        }
    }
}
