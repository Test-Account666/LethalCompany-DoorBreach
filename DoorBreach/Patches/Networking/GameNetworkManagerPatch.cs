using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using Plugin = global::DoorBreach.DoorBreach;

namespace DoorBreach.Patches.Networking;

[HarmonyPatch(typeof(GameNetworkManager))]
public static class GameNetworkManagerPatch {
    [HarmonyPatch(nameof(GameNetworkManager.Start))]
    [HarmonyPostfix]
    public static void RegisterNetworkPrefab() {
        var networkManagerPrefab = Plugin.GetNetworkManagerPrefab();

        if (NetworkManager.Singleton.NetworkConfig.Prefabs.Contains(networkManagerPrefab)) return;

        NetworkManager.Singleton.AddNetworkPrefab(networkManagerPrefab);
    }

    [HarmonyPatch(nameof(GameNetworkManager.Disconnect))]
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    public static void DestroyNetworkManager(GameNetworkManager __instance) {
        if (!__instance.isHostingGame) {
            Plugin.DoorNetworkManager = null!;
            return;
        }

        Plugin.DoorNetworkManager.NetworkObject.Despawn();
    }

    [HarmonyPatch(nameof(GameNetworkManager.SetLobbyJoinable))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void SpawnNetworkManager(GameNetworkManager __instance) {
        if (!__instance.isHostingGame) return;

        if (Plugin.DoorNetworkManager && Plugin.DoorNetworkManager.NetworkObject) {
            Plugin.Logger.LogDebug("Network manager already exists! Destroying...");
            Plugin.DoorNetworkManager.NetworkObject.Despawn();
        }

        var networkManagerObject = Object.Instantiate(Plugin.GetNetworkManagerPrefab());

        var networkObject = networkManagerObject.GetComponent<NetworkObject>();
        networkObject.Spawn();
        Object.DontDestroyOnLoad(networkManagerObject);
    }
}