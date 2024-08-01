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
using System.Collections;
using System.IO;
using System.Reflection;
using System.Threading;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using DoorBreach.Dependencies;
using DoorBreach.Patches;
using DoorBreach.Patches.DoorBreach;
using DoorBreach.Patches.DoorBreach.Mods.PiggyVariety;
using DoorBreach.Patches.DoorBreach.Mods.ToilHead;
using UnityEngine;
using UnityEngine.Networking;
using Debug = System.Diagnostics.Debug;

namespace DoorBreach;

[BepInDependency("com.github.zehsteam.ToilHead", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("Piggy.PiggyVarietyMod", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("BMX.LobbyCompatibility", BepInDependency.DependencyFlags.SoftDependency)]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class DoorBreach : BaseUnityPlugin {
    public static DoorBreach Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony? Harmony { get; set; }

    internal static AudioClip? doorHitKnifeSfx;
    internal static AudioClip? doorHitShovelSfx;
    internal static AudioClip? doorBreakSfx;

    internal static void Patch() {
        Harmony ??= new(MyPluginInfo.PLUGIN_GUID);

        Logger.LogDebug("Patching...");

        Harmony.PatchAll(typeof(DoorLockPatch));

        Harmony.PatchAll(typeof(LandminePatch));
        Harmony.PatchAll(typeof(MeleeWeaponPatch));
        Harmony.PatchAll(typeof(ShotgunPatch));
        Harmony.PatchAll(typeof(TurretPatch));

        if (DependencyChecker.IsPiggyInstalled()) {
            Harmony.PatchAll(typeof(RiflePatch));
            Harmony.PatchAll(typeof(RevolverPatch));
        }

        if (DependencyChecker.IsToilHeadInstalled()) Harmony.PatchAll(typeof(ToilHeadTurretPatch));

        Logger.LogDebug("Finished patching!");
    }

    private void Awake() {
        Logger = base.Logger;
        Instance = this;

        if (DependencyChecker.IsLobbyCompatibilityInstalled()) {
            Logger.LogInfo("Found LobbyCompatibility Mod, initializing support :)");
            LobbyCompatibilitySupport.Initialize();
        }

        DoorBreachConfig.InitializeConfig(Config);

        Patch();

        //Make RCP methods work
        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types) {
            try {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods) {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length <= 0) continue;

                    method.Invoke(null, null);
                }
            } catch (FileNotFoundException) {
            }
        }

        StartCoroutine(LoadAudioClips());

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    private static IEnumerator LoadAudioClips() {
        var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        Logger.LogInfo("Loading Sounds...");

        Debug.Assert(assemblyDirectory != null, nameof(assemblyDirectory) + " != null");
        var audioPath = Path.Combine(assemblyDirectory, "sounds");

        audioPath = Directory.Exists(audioPath)? audioPath : Path.Combine(assemblyDirectory);

        LoadDoorAudioClips(audioPath);

        yield break;
    }

    private static void LoadDoorAudioClips(string audioPath) {
        Logger.LogInfo("Loading Door Sounds...");

        var doorAudioPath = Path.Combine(audioPath, "DoorSfx");

        doorAudioPath = Directory.Exists(doorAudioPath)? doorAudioPath : Path.Combine(audioPath);


        doorHitKnifeSfx = LoadAudioClipFromFile(new(Path.Combine(doorAudioPath, "DoorHitKnife.wav")), "DoorHitKnife");

        Logger.LogInfo(doorHitKnifeSfx is null? "Failed to load clip 'DoorHitKnife'!" : $"Loaded clip '{doorHitKnifeSfx.name}'!");

        doorHitShovelSfx = LoadAudioClipFromFile(new(Path.Combine(doorAudioPath, "DoorHitShovel.wav")), "DoorHitShovel");

        Logger.LogInfo(doorHitShovelSfx is null? "Failed to load clip 'DoorHitShovel'!" : $"Loaded clip '{doorHitShovelSfx.name}'!");


        doorBreakSfx = LoadAudioClipFromFile(new(Path.Combine(doorAudioPath, "DoorBreak.wav")), "DoorBreak");

        Logger.LogInfo(doorBreakSfx is null? "Failed to load clip 'DoorBreak'!" : $"Loaded clip '{doorBreakSfx.name}'!");
    }

    private static AudioClip? LoadAudioClipFromFile(Uri filePath, string name) {
        using var unityWebRequest = UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.WAV);

        var asyncOperation = unityWebRequest.SendWebRequest();

        while (!asyncOperation.isDone) Thread.Sleep(100);

        if (unityWebRequest.result != UnityWebRequest.Result.Success) {
            Logger.LogError("Failed to load AudioClip: " + unityWebRequest.error);
            return null;
        }

        var clip = DownloadHandlerAudioClip.GetContent(unityWebRequest);

        clip.name = name;

        return clip;
    }
}