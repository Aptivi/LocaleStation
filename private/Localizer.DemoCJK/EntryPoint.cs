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

using Localizer.DemoCJK.Localized;
using Terminaux.Writer.ConsoleWriters;

namespace Localizer.DemoCJK
{
    internal class EntryPoint
    {
        internal static void Main()
        {
            TextWriterColor.Write("Localized strings will appear here.\n");

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
                    string translated = LocalStrings.Translate(loc, lang);
                    ListEntryWriterColor.WriteListEntry($"{lang} -> {loc}", translated, 1);
                }
            }

            // Do the invalid translation tests
            TextWriterColor.Write("\nInvalid Translations:");
            string invalid1 = LocalStrings.Translate(LocalStrings.Localizations[0], "brz");
            ListEntryWriterColor.WriteListEntry($"brz -> {LocalStrings.Localizations[0]}", invalid1, 1);
            string invalid2 = LocalStrings.Translate("LOCAL_STRING", "brz");
            ListEntryWriterColor.WriteListEntry($"brz -> LOCAL_STRING", invalid2, 1);
            string invalid3 = LocalStrings.Translate("LOCAL_STRING", LocalStrings.Languages[0]);
            ListEntryWriterColor.WriteListEntry($"{LocalStrings.Languages[0]} -> LOCAL_STRING", invalid3, 1);
        }
    }
}
