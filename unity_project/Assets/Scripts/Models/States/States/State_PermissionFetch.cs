using System.Collections.Generic;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

/// <summary>
/// Barebone state, does nothing.
/// </summary>
/// Used as a null-valued state, and also as a reference to build all other states.
class State_PermissionFetch : State {
    /// <summary>
    /// Constructor.
    /// </summary>
    public State_PermissionFetch(StateMachine state_machine, UIContainer UI_container) : base(state_machine) {
        _UI_container = UI_container;
        _permission_UI = UI_container.PermissionUI;
    }

    readonly PermissionUI _permission_UI;

    bool closing = false;

    readonly List<string> android_permissions = new List<string>() {
        Permission.FineLocation,
        "android.permission.ACCESS_NETWORK_STATE",
        "android.permission.INTERNET"
    };

    readonly UIContainer _UI_container;

    public override void Enter() {
        _permission_UI.gameObject.SetActive(true);
        _permission_UI.StartCoroutine(_permission_UI.Enable());
        
        #if PLATFORM_ANDROID
        for (int i = 0; i < android_permissions.Count && !closing; i++) {
            AndroidRuntimePermissions.Permission permission = AndroidRuntimePermissions.CheckPermission(android_permissions[i]);
            while (permission != AndroidRuntimePermissions.Permission.Granted && !closing) {
                switch (permission) {
                    case AndroidRuntimePermissions.Permission.ShouldAsk:
                        permission = AndroidRuntimePermissions.RequestPermission(android_permissions[i]);
                        break;
                    case AndroidRuntimePermissions.Permission.Denied:
                        _permission_UI.StartCoroutine(_permission_UI.PermissionDenied());
                        closing = true;
                        break;
                }
            }
        }
        #endif
    }

    public override void Update() {
        if (!closing)
            StateMachine.IssueChangeState(new State_MainMenu(StateMachine, _UI_container));
    }

    public override void Exit() {
        _permission_UI.DisableInstant();
    }
}
