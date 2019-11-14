using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Globals {
    //Contains the program global variables
    public static Transform SPHERE_DESTINATION;
    public static float LERP_ALPHA = 0.1f;
    public static int MAX_CAMERA_DEALIGNMENT = 250;

    static Globals()
    {
        SPHERE_DESTINATION = new GameObject("Sphere Reference").transform;

        Transform sphere = GameObject.Find("Sphere").transform;
        SPHERE_DESTINATION.position = sphere.position;
        SPHERE_DESTINATION.rotation = sphere.rotation;
        SPHERE_DESTINATION.localScale = sphere.localScale;
    }

}
