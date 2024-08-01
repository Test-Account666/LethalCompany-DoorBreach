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
using GameNetcodeStuff;

namespace DoorBreach;

public static class EventHandler {
    public delegate void DoorBreachEvent(DoorBreachEventArguments doorBreachEventArguments);

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public static DoorBreachEvent? doorBreach;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    internal static void OnDoorBreach(ActionSource.Source actionSource, DoorLock doorLock, PlayerControllerB? playerControllerB,
                                      DoorBreachConfig.DoorBreachMode doorBreachMode) {
        doorBreach?.Invoke(new(actionSource, doorLock, playerControllerB, doorBreachMode));
    }

    public class DoorBreachEventArguments(
        ActionSource.Source actionSource,
        DoorLock doorLock,
        PlayerControllerB? playerControllerB,
        DoorBreachConfig.DoorBreachMode doorBreachMode) : EventArgs {
        public readonly ActionSource.Source actionSource = actionSource;
        public readonly DoorLock doorLock = doorLock;
        public readonly PlayerControllerB? playerControllerB = playerControllerB;
        public readonly DoorBreachConfig.DoorBreachMode doorBreachMode = doorBreachMode;
    }
}