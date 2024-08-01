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
using DoorBreach.Functional;
using HarmonyLib;
using UnityEngine;

namespace DoorBreach.Patches.DoorBreach;

[HarmonyPatch(typeof(Turret))]
public static class TurretPatch {
    [HarmonyPatch(nameof(Turret.Update))]
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    private static void DecimateDoors(Turret __instance) {
        if (__instance.turretMode is not TurretMode.Firing and not TurretMode.Berserk) return;

        if (__instance is {
                enteringBerserkMode: true, berserkTimer: > 0,
            }) return;

        if (__instance.turretInterval < 0.209) return;

        var shootRay = new Ray(__instance.aimPoint.position, __instance.aimPoint.forward);
        var hitDoor = Physics.Raycast(shootRay, out var doorLock, 23f, 1 << 9, QueryTriggerInteraction.Collide);

        if (!hitDoor) return;

        var hasHealth = doorLock.collider.TryGetComponent(out DoorHealth doorHealth);

        if (!hasHealth) return;

        var distance = doorLock.distance;

        const int baseDamage = 9;

        var logFactor = Math.Max(Math.Log(distance + 1, 5), 1);

        var adjustedDamage = (int) (baseDamage / logFactor);

        doorHealth.HitDoorServerRpc(ActionSource.Source.TURRET.ToInt(), adjustedDamage);
    }
}