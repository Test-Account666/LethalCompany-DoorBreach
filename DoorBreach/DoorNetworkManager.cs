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


using System.Collections.Generic;
using DoorBreach.Functional;
using Unity.Netcode;

namespace DoorBreach;

public class DoorNetworkManager : NetworkBehaviour {
    public static readonly Dictionary<NetworkObject, DoorHealth> DoorHealthCache = new();
    public static readonly Dictionary<NetworkObject, DoorLocker> DoorLockerCache = new();

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();

        DoorBreach.DoorNetworkManager = this;

        DoorBreach.Logger.LogFatal("Network Spawned!");
    }

    public override void OnNetworkDespawn() {
        base.OnNetworkDespawn();

        DoorBreach.DoorNetworkManager = null!;
        DoorHealthCache.Clear();
        DoorLockerCache.Clear();

        DoorBreach.Logger.LogFatal("Network Despawned!");
    }

    #region DoorHealth

    [ServerRpc(RequireOwnership = false)]
    public void HitDoorServerRpc(NetworkObjectReference doorHit, int playerWhoHit, int damage) {
        DoorBreach.Logger.LogFatal($"Calling HitDoorServerRpc! {doorHit}");

        var hasDoorHealth = TryGetDoorHealth(doorHit, out var doorHealth);
        if (!hasDoorHealth) return;

        DoorBreach.Logger.LogFatal("Found door health!");

        doorHealth.HitDoorServer(playerWhoHit, damage);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestHealthServerRpc(NetworkObjectReference doorHit) {
        var hasDoorHealth = TryGetDoorHealth(doorHit, out var doorHealth);
        if (!hasDoorHealth) return;

        doorHealth.RequestHealthServer();
    }

    [ClientRpc]
    public void BreakDoorClientRpc(NetworkObjectReference doorHit, int playerWhoTriggered) {
        var hasDoorHealth = TryGetDoorHealth(doorHit, out var doorHealth);
        if (!hasDoorHealth) return;

        doorHealth.BreakDoorClient(playerWhoTriggered);
    }

    [ClientRpc]
    public void SetHealthClientRpc(NetworkObjectReference doorHit, int health) {
        var hasDoorHealth = TryGetDoorHealth(doorHit, out var doorHealth);
        if (!hasDoorHealth) return;

        doorHealth.SetHealthClient(health);
    }

    private static bool TryGetDoorHealth(NetworkObjectReference networkObjectReference, out DoorHealth doorHealth) {
        doorHealth = null!;

        var hasNetworkObject = networkObjectReference.TryGet(out var networkObject);
        if (!hasNetworkObject) return false;

        var foundInCache = DoorHealthCache.TryGetValue(networkObject, out doorHealth);
        if (foundInCache) return true;

        doorHealth = networkObject.GetComponentInChildren<DoorHealth>();
        if (!doorHealth) return false;

        DoorHealthCache.Add(networkObject, doorHealth);
        return true;
    }

    #endregion DoorHealth

    #region DoorLocker

    [ServerRpc(RequireOwnership = false)]
    public void LockDoorServerRpc(NetworkObjectReference door) {
        var hasDoorLocker = TryGetDoorLocker(door, out var doorLocker);
        if (!hasDoorLocker) return;

        doorLocker.LockDoorServer();
    }

    [ClientRpc]
    public void LockDoorClientRpc(NetworkObjectReference door) {
        var hasDoorLocker = TryGetDoorLocker(door, out var doorLocker);
        if (!hasDoorLocker) return;

        doorLocker.LockDoorClient();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetDoorOpenServerRpc(NetworkObjectReference door, int playerWhoTriggered, bool open) {
        var hasDoorLocker = TryGetDoorLocker(door, out var doorLocker);
        if (!hasDoorLocker) return;

        doorLocker.SetDoorOpenServer(playerWhoTriggered, open);
    }

    [ClientRpc]
    public void SetDoorOpenClientRpc(NetworkObjectReference door, int playerWhoTriggered, bool open) {
        var hasDoorLocker = TryGetDoorLocker(door, out var doorLocker);
        if (!hasDoorLocker) return;

        doorLocker.SetDoorOpenClient(playerWhoTriggered, open);
    }

    private static bool TryGetDoorLocker(NetworkObjectReference networkObjectReference, out DoorLocker doorLocker) {
        doorLocker = null!;

        var networkObject = (NetworkObject) networkObjectReference;
        if (!networkObject) return false;

        var foundInCache = DoorLockerCache.TryGetValue(networkObject, out doorLocker);
        if (foundInCache) return true;

        doorLocker = networkObject.GetComponentInChildren<DoorLocker>();
        if (!doorLocker) return false;

        DoorLockerCache.Add(networkObject, doorLocker);
        return true;
    }

    #endregion DoorLocker
}