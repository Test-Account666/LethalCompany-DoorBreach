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


using System.Collections;
using GameNetcodeStuff;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DoorBreach.Functional;

public class DoorHealth : MonoBehaviour {
    private bool _broken;
    private bool _hittable = true;
    private DoorLocker _doorLocker = null!;
    private int _health = 8;

    public DoorLock DoorLock { get; private set; } = null!;

    private void Awake() {
        var minimumHealth = DoorBreachConfig.minimumDoorHealth;
        var maximumHealth = minimumHealth + DoorBreachConfig.possibleAdditionalHealth;

        _health = Random.RandomRangeInt(minimumHealth, maximumHealth + 1);
    }

    private void OnDestroy() => DoorNetworkManager.DoorHealthCache.Remove(DoorLock.NetworkObject);

    public bool IsBroken() => _broken;
    public bool IsDoorOpen() => DoorLock.isDoorOpened;

    private void Update() {
        if (!_broken) return;

        if (DoorLock.isDoorOpened) return;

        _doorLocker.SetDoorOpenServer(ActionSource.Source.UNKNOWN.ToInt(), true);
    }

    private void Start() => DoorBreach.DoorNetworkManager.RequestHealthServerRpc(DoorLock.NetworkObject);

    public void RequestHealthServer() => DoorBreach.DoorNetworkManager.SetHealthClientRpc(DoorLock.NetworkObject, _health);

    internal void SetDoorLock(DoorLock doorLock) => DoorLock = doorLock;

    internal void SetDoorLocker(DoorLocker doorLocker) => _doorLocker = doorLocker;

    public void HitDoorServer(int playerWhoHit, int damage) {
        if (damage == 0) return;

        var actionSource = playerWhoHit.FromInt();

        if (actionSource is null) return;

        if (!DoorBreachConfig.AllowedDoorBreachSources.Contains(actionSource.Value)) return;

        if (!_hittable) return;

        _hittable = false;

        StartCoroutine(ResetHittable());

        DoorBreach.Logger.LogDebug("Broken: " + _broken);

        DoorBreach.Logger.LogDebug("Current health: " + _health);
        DoorBreach.Logger.LogDebug("Damage dealt: " + damage);

        DoorBreach.Logger.LogDebug("Source: " + actionSource.Value);

        if (_broken) return;

        DoorBreach.DoorNetworkManager.SetHealthClientRpc(DoorLock.NetworkObject, _health - damage);

        if (_health > 0) return;

        DoorBreach.DoorNetworkManager.BreakDoorClientRpc(DoorLock.NetworkObject, playerWhoHit);
    }

    private IEnumerator ResetHittable() {
        yield return new WaitForSeconds(.05F);
        _hittable = true;
    }

    public void SetHealthClient(int health) => _health = health;

    public void BreakDoorClient(int playerWhoTriggered) {
        PlayAudio(gameObject);

        var actionSource = playerWhoTriggered.FromInt();

        if (actionSource is null) return;

        PlayerControllerB? player = null;

        if (actionSource == ActionSource.Source.PLAYER) player = StartOfRound.Instance.allPlayerScripts[playerWhoTriggered];

        EventHandler.OnDoorBreach(actionSource.Value, DoorLock, player, DoorBreachConfig.doorBreachMode);

        _broken = true;

        if (DoorBreachConfig.doorBreachMode == DoorBreachConfig.DoorBreachMode.DESTROY) {
            var doorLockTransform = DoorLock.transform;
            Landmine.SpawnExplosion(doorLockTransform.position, true);
            Destroy(doorLockTransform.parent.gameObject);
            return;
        }

        _doorLocker.SetDoorOpenServer(playerWhoTriggered, true);

        if (DoorBreachConfig.doorBreachMode == DoorBreachConfig.DoorBreachMode.OPEN) {
            _broken = false;
            _health = 1;
            return;
        }

        DoorLock.doorTrigger.interactable = false;
        DoorLock.doorTrigger.enabled = false;

        DoorLock.doorTrigger.holdTip = "";
        DoorLock.doorTrigger.disabledHoverTip = "";

        DoorLock.doorTrigger.hoverIcon = null;
        DoorLock.doorTrigger.disabledHoverIcon = null;
    }

    public int GetHealth() => _health;

    private static void PlayAudio(GameObject gameObject) {
        if (DoorBreach.doorBreakSfx is null) return;

        var audioSource = gameObject.AddComponent<AudioSource>();

        // Set spatial blend to 1 for full 3D sound
        audioSource.spatialBlend = 1.0f;

        // Set max distance to 100 units
        audioSource.maxDistance = 30.0f;

        // Set rolloff mode to Logarithmic
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;

        audioSource.clip = DoorBreach.doorBreakSfx;
        audioSource.volume = 1F;
        audioSource.Play();

        Destroy(audioSource, DoorBreach.doorBreakSfx.length);
    }
}