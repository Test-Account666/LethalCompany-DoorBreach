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
using Plugin = global::DoorBreach.DoorBreach;

namespace DoorBreach.Patches.DoorBreach;

[HarmonyPatch(typeof(Landmine))]
public class LandminePatch {
    [HarmonyPatch(nameof(Landmine.Detonate))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void DisintegrateDoors(Landmine __instance) {
        var position = __instance.transform.position;

        var results = new Collider[12];

        var size = Physics.OverlapSphereNonAlloc(position, 6F, results, 1 << 9, QueryTriggerInteraction.Collide);

        if (size <= 0) return;

        for (var index = 0; index < size; index++) {
            var collider = results[index];

            var hasHealth = collider.TryGetComponent(out DoorHealth doorHealth);

            if (!hasHealth) continue;

            var distance = Vector3.Distance(position, collider.transform.position);

            const int baseDamage = 11;

            int adjustedDamage;

            if (distance <= 3.6f) {
                adjustedDamage = 666;
            } else {
                var logFactor = Math.Max(Math.Log(distance + 1, 4), 1);

                adjustedDamage = (int) (baseDamage / logFactor);
            }

            Plugin.DoorNetworkManager.HitDoorServerRpc(doorHealth.DoorLock.NetworkObject, ActionSource.Source.LANDMINE.ToInt(), adjustedDamage);
        }
    }
}