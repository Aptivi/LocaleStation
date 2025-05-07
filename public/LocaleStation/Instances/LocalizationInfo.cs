﻿//
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

using Newtonsoft.Json;

namespace LocaleStation.Instances
{
    public class LocalizationInfo
    {
        [JsonProperty(nameof(loc))]
        private string loc = "";
        [JsonProperty(nameof(text))]
        private string text = "";

        [JsonIgnore]
        public string Localization =>
            loc;

        [JsonIgnore]
        public string Text =>
            text;

        [JsonConstructor]
        internal LocalizationInfo()
        { }
    }
}
