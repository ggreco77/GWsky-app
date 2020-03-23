using UnityEngine;

/// <summary>
/// Contains references to prefabs which are not in the scene, so that they may be instantiated whenever needed.
/// </summary>
/// Implemented as a singleton.
class VirtualObjects : MonoBehaviour {
    public GameObject EventButton;

    private static VirtualObjects _instance;

    public static VirtualObjects Instance {
        get {
            if (_instance == null) {
                // FindObjectOfType() returns the first AManager object in the scene.
                _instance = FindObjectOfType(typeof(VirtualObjects)) as VirtualObjects;
            }
            return _instance;
        }
    }
}
