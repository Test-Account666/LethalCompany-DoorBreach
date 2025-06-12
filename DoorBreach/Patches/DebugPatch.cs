using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace DoorBreach.Patches;

[HarmonyPatch(typeof(PlayerControllerB))]
public static class DebugPatch {
    [HarmonyPatch(nameof(PlayerControllerB.ScrollMouse_performed))]
    [HarmonyPrefix]
    public static void FindTwinDoor() {
        var localPlayer = StartOfRound.Instance.localPlayerController;

        var doorLocks = Object.FindObjectsByType<DoorLock>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        var found = false;

        foreach (var doorLock in doorLocks) {
            if (!doorLock) continue;

            if (!doorLock.doorTriggerB) continue;

            if (doorLock.doorTriggerB == doorLock.doorTrigger) continue;

            localPlayer.TeleportPlayer(doorLock.lockPickerPosition.position);
            found = true;
        }

        HUDManager.Instance.AddChatMessage(found? "Found twin door!" : "No twin door found!");
    }
}