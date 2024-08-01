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
using UnityEngine;

namespace DoorBreach;

public static class ActionSource {
    public enum Source {
        UNKNOWN = -666,
        MALFUNCTION = -665,
        TOIL_HEAD = -5,
        LANDMINE = -4,
        TURRET = -3,
        SHOTGUN_ACCIDENT = -2,
        SHOTGUN_ENEMY = -1,

        [Tooltip("Player is actually anything above -1, but this is an enum, so...")]
        PLAYER = 0,
    }

    [Flags]
    public enum SelectableSource {
        TOIL_HEAD = -5,
        LANDMINE = -4,
        TURRET = -3,
        SHOTGUN_ACCIDENT = -2,
        SHOTGUN_ENEMY = -1,

        [Tooltip("Player is actually anything above -1, but this is an enum, so...")]
        PLAYER = 0,

        ALL = TOIL_HEAD | LANDMINE | TURRET | SHOTGUN_ACCIDENT | SHOTGUN_ENEMY | PLAYER,
    }

    public static Source? FromInt(this int source) {
        if (source >= 0) return Source.PLAYER;

        return (Source) source;
    }

    public static Source? FromSelectableSource(this SelectableSource selectableSource) {
        var selectableSourceValue = (int) selectableSource;

        return selectableSourceValue.FromInt();
    }

    public static int ToInt(this Source source) => (int) source;
}