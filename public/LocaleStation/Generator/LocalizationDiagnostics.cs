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

using Microsoft.CodeAnalysis;

namespace LocaleStation.Generator
{
    internal static class LocalizationDiagnostics
    {
        internal static Diagnostic GetLocalizationReadErrorDiagnostic(string localizationFile) =>
            Diagnostic.Create(new DiagnosticDescriptor("APTLOC0001", "Localization file read error", $"There was a read error when trying to read the localization file {localizationFile}", "Localization", DiagnosticSeverity.Error, true), null);
        
        internal static Diagnostic GetLocalizationStructureErrorDiagnostic(string localizationFile) =>
            Diagnostic.Create(new DiagnosticDescriptor("APTLOC0002", "Localization file structure error", $"There was an error when trying to deserialize the localization file {localizationFile}", "Localization", DiagnosticSeverity.Error, true), null);
        
        internal static Diagnostic GetLocalizationCultureNotFoundWarningDiagnostic(string cultureName, string localizationFile) =>
            Diagnostic.Create(new DiagnosticDescriptor("APTLOC0003", $"Specified culture \"{cultureName}\" is not found", $"The localization file {localizationFile} contains an invalid culture: \"{cultureName}\". Ignoring...", "Localization", DiagnosticSeverity.Warning, true), null);
    }
}
