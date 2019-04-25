using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureButton : MonoBehaviour {

    int _tex = 0;
    Transform _sphere;

	public void Init(Transform sphere)
    {
        _sphere = sphere;

        Button button = gameObject.GetComponent<Button>() as Button;
        Renderer renderer = _sphere.gameObject.GetComponent<Renderer>() as Renderer;
        button.onClick.AddListener(delegate
        {
            if (_tex == 0)
            {
                _tex = 1;
                renderer.material = Resources.Load<Material>("Materials/PANO_IRIS_GW170814_Poly 1") as Material;
            }
            else
            {
                _tex = 0;
                renderer.material = Resources.Load<Material>("Materials/world_test 1") as Material;
            }
        });
    }
}
