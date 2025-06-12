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
using com.github.zehsteam.ToilHead.MonoBehaviours;
using DoorBreach.Functional;
using HarmonyLib;
using UnityEngine;
using Plugin = global::DoorBreach.DoorBreach;

namespace DoorBreach.Patches.DoorBreach.Mods.ToilHead;

[HarmonyPatch(typeof(ToilHeadTurretBehaviour))]
public class ToilHeadTurretPatch {
    [HarmonyPatch(nameof(ToilHeadTurretBehaviour.TurretModeLogic))]
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    private static void FireAwayThosePeskyDoors(ToilHeadTurretBehaviour __instance) {
        if (__instance.turretMode is not TurretMode.Firing and not TurretMode.Berserk) return;

        if (__instance is {
                _enteringBerserkMode: true, _berserkTimer: > 0,
            }) return;

        if (__instance._turretInterval < __instance._damageRate) return;

        var shootRay = new Ray(__instance.aimPoint.position, __instance.aimPoint.forward);
        var hitDoor = Physics.Raycast(shootRay, out var doorLock, 23f, 1 << 9, QueryTriggerInteraction.Collide);

        if (!hitDoor) return;

        var hasHealth = doorLock.collider.TryGetComponent(out DoorHealth doorHealth);

        if (!hasHealth) {
            doorHealth = doorLock.collider.transform.parent.parent.GetComponentInChildren<DoorHealth>();
            if (!doorHealth) return;
        }

        var distance = doorLock.distance;

        const int baseDamage = 9;

        var logFactor = Math.Max(Math.Log(distance + 1, 5), 1);

        var adjustedDamage = (int) (baseDamage / logFactor);

        Plugin.DoorNetworkManager.HitDoorServerRpc(doorHealth.DoorLock.NetworkObject, ActionSource.Source.TOIL_HEAD.ToInt(), adjustedDamage);
    }
}