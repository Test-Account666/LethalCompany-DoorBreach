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
using Random = System.Random;

namespace DoorBreach.Patches.DoorBreach;

[HarmonyPatch(typeof(ShotgunItem))]
public static class ShotgunPatch {
    private static readonly Random _Random = new();

    [HarmonyPatch(nameof(ShotgunItem.ShootGun))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void ShootDoor(ShotgunItem __instance, Vector3 shotgunPosition, Vector3 shotgunForward) {
        var playerWhoShot = ActionSource.Source.SHOTGUN_ACCIDENT.ToInt();

        if (__instance.isHeld) playerWhoShot = (int) __instance.playerHeldBy.playerClientId;

        if (__instance.isHeldByEnemy) playerWhoShot = ActionSource.Source.SHOTGUN_ENEMY.ToInt();

        var ray = new Ray(shotgunPosition, shotgunForward);

        var hitDoor = Physics.Raycast(ray, out var doorLock, 8f, 1 << 9,
                                      QueryTriggerInteraction.Collide);


        if (!hitDoor) return;

        var hasHealth = doorLock.collider.TryGetComponent(out DoorHealth doorHealth);

        if (!hasHealth) return;

        var distance = doorLock.distance;

        const int baseDamage = 9;

        var adjustedDamage = 666;

        var instantBreak = distance <= 3? _Random.Next(0, (int) (3 - distance)) : 0;

        if (instantBreak <= 0) {
            var logFactor = Math.Max(Math.Log(distance + 1, 2), 1);

            adjustedDamage = (int) (baseDamage / logFactor);
        }

        doorHealth.HitDoorServerRpc(playerWhoShot, adjustedDamage);
    }
}