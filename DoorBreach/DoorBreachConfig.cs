/*
    A Lethal Company Mod
    Copyright (C) 2024  TestAccount666 (Entity303 / Test-Account666)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/


using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;

namespace DoorBreach;

public static class DoorBreachConfig {
    public static readonly List<ActionSource.Source> AllowedDoorBreachSources = [
    ];

    public static bool doorBreachEnabled = true;
    public static DoorBreachMode doorBreachMode = DoorBreachMode.DESTROY;
    public static int minimumDoorHealth = 8;
    public static int possibleAdditionalHealth = 16;

    public enum DoorBreachMode {
        DESTROY,
        UNUSABLE,
        OPEN,
    }

    public static void InitializeConfig(ConfigFile configFile) {
        doorBreachEnabled = configFile.Bind("Door Breach", "1. Door Breach Enabled", true, "If true, will enable door breach").Value;

        doorBreachMode = configFile.Bind("Door Breach", "2. Door Breach Mode", DoorBreachMode.DESTROY,
                                         "What mode should door breach use?"
                                       + " Destroy will destroy the door and unusable will make it unusable (This may cause bugs)").Value;

        minimumDoorHealth = configFile.Bind("Door Breach", "3. Minimum Door Health", 8,
                                            new ConfigDescription("The minimum health a door has", new AcceptableValueRange<int>(1, 16))).Value;

        possibleAdditionalHealth = configFile.Bind("Door Breach", "4. Possible Additional Door Health", 16,
                                                   new ConfigDescription(
                                                       "This value defines how much additional health a door can have "
                                                     + "(On default values, this means a door's health can be between 8 and 24)",
                                                       new AcceptableValueRange<int>(0, 16))).Value;

        var selectableSources = configFile.Bind("Door Breach", "5. Allowed Door Breach Sources", ActionSource.SelectableSource.ALL,
                                                "Defines what can breach doors").Value;

        foreach (var value in EnumUtil.GetValues<ActionSource.SelectableSource>()) {
            var hasFlag = selectableSources.HasFlag(value);

            if (!hasFlag) continue;

            var source = value.FromSelectableSource();
            if (source is null) continue;

            if (AllowedDoorBreachSources.Contains(source.Value)) continue;

            AllowedDoorBreachSources.Add(source.Value);
        }

        AllowedDoorBreachSources.Add(ActionSource.Source.MALFUNCTION);
        AllowedDoorBreachSources.Add(ActionSource.Source.UNKNOWN);
    }
}

public static class EnumUtil {
    public static IEnumerable<T> GetValues<T>() => Enum.GetValues(typeof(T)).Cast<T>();
}