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

using Newtonsoft.Json;

namespace Localizer.Instances
{
    public class LanguageInfo
    {
        [JsonProperty(nameof(lang))]
        private string lang = "";
        [JsonProperty(nameof(name))]
        private string name = "";
        [JsonProperty(nameof(cultures))]
        private string[] cultures = [];
        [JsonProperty(nameof(locs))]
        private LocalizationInfo[] locs = [];

        [JsonIgnore]
        public string Language =>
            lang;

        [JsonIgnore]
        public string Name =>
            name;

        [JsonIgnore]
        public string[] Cultures =>
            cultures;

        [JsonIgnore]
        public LocalizationInfo[] Localizations =>
            locs;

        [JsonConstructor]
        internal LanguageInfo()
        { }
    }
}
