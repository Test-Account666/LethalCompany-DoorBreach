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

using Unity.Netcode;

namespace DoorBreach.Functional;

public class DoorLocker : NetworkBehaviour {
    private DoorLock _doorLock = null!;

    private void Awake() => _doorLock = GetComponent<DoorLock>();

    public override void OnDestroy() => DoorNetworkManager.DoorLockerCache.Remove(_doorLock.NetworkObject);

    public void LockDoorServer() => DoorBreach.DoorNetworkManager.LockDoorClientRpc(_doorLock.NetworkObject);

    public void SetDoorOpenServer(int playerWhoTriggered, bool open) {
        DoorBreach.DoorNetworkManager.SetDoorOpenClientRpc(_doorLock.NetworkObject, playerWhoTriggered, open);
    }

    public void LockDoorClient() => _doorLock.LockDoor();

    public void SetDoorOpenClient(int playerWhoTriggered, bool open) {
        var component = _doorLock.gameObject.GetComponent<AnimatedObjectTrigger>();

        component.boolValue = open;

        component.triggerAnimator.SetBool(component.animationString, component.boolValue);
        component.onTriggerBool.Invoke(component.boolValue);

        _doorLock.isDoorOpened = component.boolValue;
        _doorLock.navMeshObstacle.enabled = !component.boolValue;
    }
}