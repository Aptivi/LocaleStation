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

using LocaleStation.DemoCJK.Localized;
using LocaleStation.Tools;
using Terminaux.Writer.ConsoleWriters;

namespace LocaleStation.DemoCJK
{
    internal class EntryPoint
    {
        internal static void Main()
        {
            TextWriterColor.Write("Localized strings will appear here.\n");
            LanguageCommon.AddCustomAction("Demonstration", new(() => LocalStrings.Languages, () => LocalStrings.Localizations, LocalStrings.Translate, LocalStrings.CheckCulture, LocalStrings.ListLanguagesCulture, LocalStrings.Exists));

            // Print the list of localizations and languages
            TextWriterColor.Write("Localizations:");
            for (int i = 0; i < LocalStrings.Localizations.Length; i++)
            {
                string loc = LocalStrings.Localizations[i];
                ListEntryWriterColor.WriteListEntry($"{i + 1}", loc, 1);
            }
            TextWriterColor.Write("\nLanguages:");
            for (int i = 0; i < LocalStrings.Languages.Length; i++)
            {
                string loc = LocalStrings.Languages[i];
                ListEntryWriterColor.WriteListEntry($"{i + 1}", loc, 1);
            }

            // Do the translation tests
            TextWriterColor.Write("\nTranslations:");
            for (int i = 0; i < LocalStrings.Languages.Length; i++)
            {
                for (int j = 0; j < LocalStrings.Localizations.Length; j++)
                {
                    string lang = LocalStrings.Languages[i];
                    string loc = LocalStrings.Localizations[j];
                    string translated = LanguageCommon.Translate(loc, "Demonstration", lang);
                    bool exists = LocalStrings.Exists(loc, lang);
                    ListEntryWriterColor.WriteListEntry($"{lang} -> {loc} [{exists}]", translated, 1);
                }
            }

            // Do the invalid translation tests
            TextWriterColor.Write("\nInvalid Translations:");
            string invalid1 = LanguageCommon.Translate(LocalStrings.Localizations[0], "Demonstration", "brz");
            bool exists1 = LocalStrings.Exists(LocalStrings.Localizations[0], "brz");
            ListEntryWriterColor.WriteListEntry($"brz -> {LocalStrings.Localizations[0]} [{exists1}]", invalid1, 1);
            string invalid2 = LanguageCommon.Translate("LOCAL_STRING", "Demonstration", "brz");
            bool exists2 = LocalStrings.Exists("LOCAL_STRING", "brz");
            ListEntryWriterColor.WriteListEntry($"brz -> LOCAL_STRING [{exists2}]", invalid2, 1);
            string invalid3 = LanguageCommon.Translate("LOCAL_STRING", "Demonstration", LocalStrings.Languages[0]);
            bool exists3 = LocalStrings.Exists("LOCAL_STRING", LocalStrings.Languages[0]);
            ListEntryWriterColor.WriteListEntry($"{LocalStrings.Languages[0]} -> LOCAL_STRING [{exists3}]", invalid3, 1);

            // Perform culture tests
            TextWriterColor.Write("\nCulture tests:");
            bool cultureExists1 = LocalStrings.CheckCulture("en-US", LocalStrings.Languages[0]);
            ListEntryWriterColor.WriteListEntry($"Culture exists for en-US in {LocalStrings.Languages[0]}", $"{cultureExists1}", 1);
            bool cultureExists2 = LocalStrings.CheckCulture("pt-BR", LocalStrings.Languages[1]);
            ListEntryWriterColor.WriteListEntry($"Culture exists for pt-BR in {LocalStrings.Languages[1]}", $"{cultureExists2}", 1);
            bool cultureExists3 = LocalStrings.CheckCulture("pt-BR", "brz");
            ListEntryWriterColor.WriteListEntry("Culture exists for pt-BR in brz", $"{cultureExists3}", 1);
            bool cultureExists4 = LocalStrings.CheckCulture("en-US", "brz");
            ListEntryWriterColor.WriteListEntry("Culture exists for en-US in brz", $"{cultureExists4}", 1);

            // Language listing tests
            TextWriterColor.Write("\nCultures and Languages:");
            string[] languages1 = LocalStrings.ListLanguagesCulture("es-AR");
            TextWriterColor.Write("  List of languages that contain es-AR:");
            for (int langIdx = 0; langIdx < languages1.Length; langIdx++)
            {
                string language = languages1[langIdx];
                ListEntryWriterColor.WriteListEntry($"{langIdx + 1}", language, 2);
            }
            string[] languages2 = LocalStrings.ListLanguagesCulture("pt-BR");
            TextWriterColor.Write("  List of languages that contain pt-BR:");
            for (int langIdx = 0; langIdx < languages2.Length; langIdx++)
            {
                string language = languages2[langIdx];
                ListEntryWriterColor.WriteListEntry($"{langIdx + 1}", language, 2);
            }
        }
    }
}
