using DoorBreach.Functional;
using HarmonyLib;

namespace DoorBreach.Patches;

[HarmonyPatch(typeof(DoorLock))]
public static class DoorLockPatch {
    [HarmonyPatch(nameof(DoorLock.Awake))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void AfterAwake(DoorLock __instance) {
        var gameObject = __instance.gameObject;
        var doorLocker = gameObject.AddComponent<DoorLocker>();

        if (!DoorBreachConfig.doorBreachEnabled) return;

        var doorHealth = gameObject.AddComponent<DoorHealth>();

        doorHealth.SetDoorLock(__instance);
        doorHealth.SetDoorLocker(doorLocker);
    }
}