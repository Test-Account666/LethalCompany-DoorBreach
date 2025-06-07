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


using DoorBreach.Functional;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Plugin = global::DoorBreach.DoorBreach;

namespace DoorBreach.Patches.DoorBreach;

[HarmonyPatch]
public static class MeleeWeaponPatch {
    [HarmonyPatch(typeof(Shovel), nameof(Shovel.HitShovel))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void HitDoor(Shovel __instance, bool cancel) {
        Plugin.Logger.LogDebug($"Hit door {__instance} Cancel? {cancel}");
        if (cancel) return;

        var audioSource = __instance.shovelAudio;

        audioSource.clip = Plugin.doorHitShovelSfx;

        HitDoor(__instance, __instance.shovelHitForce, 1.5f, 0.8f, -0.35f, audioSource);
    }

    [HarmonyPatch(typeof(KnifeItem), nameof(KnifeItem.HitKnife))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void HitDoor(KnifeItem __instance, bool cancel) {
        if (cancel) return;

        var audioSource = __instance.knifeAudio;

        audioSource.clip = Plugin.doorHitKnifeSfx;

        HitDoor(__instance, __instance.knifeHitForce, 0.75f, 0.3F, 0.1f, audioSource);
    }

    private static void HitDoor(GrabbableObject grabbableObject, int damage, float maxDistance, float radius, float rightMultiplier,
                                AudioSource? hitSoundSource = null) {
        if (!grabbableObject.isHeld) return;

        var playerHeldBy = grabbableObject.playerHeldBy;

        var gameplayCameraTransform = playerHeldBy?.gameplayCamera?.transform;

        if (gameplayCameraTransform is null) return;

        var results = new RaycastHit[12];

        var size = Physics.SphereCastNonAlloc(gameplayCameraTransform.position + gameplayCameraTransform.right * rightMultiplier, radius,
                                              gameplayCameraTransform.forward, results, maxDistance, 1 << 9 | StartOfRound.Instance.collidersAndRoomMaskAndDefault,
                                              QueryTriggerInteraction.Collide);

        var playedSound = hitSoundSource == null;

        for (var index = 0; index < size; index++) {
            var result = results[index];

            var hasHealth = result.collider.TryGetComponent(out DoorHealth doorHealth);

            if (!hasHealth) continue;

            if (!doorHealth.IsBroken() && !doorHealth.IsDoorOpen()) {
                if (!playedSound && hitSoundSource is not null) {
                    playedSound = true;
                    hitSoundSource.Play();
                }
            }

            Debug.Assert(playerHeldBy != null, nameof(playerHeldBy) + " != null");

            Plugin.DoorNetworkManager.HitDoorServerRpc(doorHealth.DoorLock.NetworkObject, ActionSource.Source.PLAYER.ToInt() + (int) playerHeldBy.playerClientId,
                                                       damage);
        }
    }
}