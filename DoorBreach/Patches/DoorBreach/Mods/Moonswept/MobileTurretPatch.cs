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
using Moonswept.Enemies.MovingTurret;
using UnityEngine;
using Plugin = global::DoorBreach.DoorBreach;

namespace DoorBreach.Patches.DoorBreach.Mods.Moonswept;

[HarmonyPatch(typeof(MovingTurret))]
public class MobileTurretPatch {
    [HarmonyPatch(nameof(MovingTurret.Update))]
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    public static void IncinerateDoor(MovingTurret __instance) {
        if (__instance.currentBehaviourStateIndex != (int) MovingTurret.BehaviourState.FIRING) return;

        if (__instance._firingDelay + Time.fixedDeltaTime < MovingTurret._FIRE_DELAY) return;

        var shootRay = new Ray(__instance.eye.position, __instance.eye.forward);
        var hitDoor = Physics.Raycast(shootRay, out var doorLock, MovingTurret._VIEW_DISTANCE, 1 << 9, QueryTriggerInteraction.Collide);

        if (!hitDoor) return;

        var hasHealth = doorLock.collider.TryGetComponent(out DoorHealth doorHealth);

        if (!hasHealth) return;

        var distance = doorLock.distance;

        const int baseDamage = 9;

        var logFactor = Math.Max(Math.Log(distance + 1, 5), 1);

        var adjustedDamage = (int) (baseDamage / logFactor);
        Plugin.DoorNetworkManager.HitDoorServerRpc(doorHealth.DoorLock.NetworkObject, ActionSource.Source.MOBILE_TURRET.ToInt(), adjustedDamage);
    }
}