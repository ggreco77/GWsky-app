using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureButton : MonoBehaviour {

    int _tex_n = -1;
    Transform _sphere;

	public void Init(Transform sphere)
    {
        _sphere = sphere;

        Button button = gameObject.GetComponent<Button>() as Button;
        Renderer renderer = _sphere.gameObject.GetComponent<Renderer>() as Renderer;

        renderer.material = Resources.Load<Material>("Materials/world_test 1") as Material;
        _tex_n = 1;

        button.onClick.AddListener(delegate
        {
            _tex_n = (_tex_n + 1) % 3;
            if (_tex_n == 0)
            {
                renderer.material = Resources.Load<Material>("Materials/PANO_IRIS_GW170814_Poly 1") as Material;
            }
            else if (_tex_n == 1)
            {
                renderer.material = Resources.Load<Material>("Materials/world_test 1") as Material;
            }
            else
            {
                renderer.material = Resources.Load<Material>("Materials/PANO_Mellinger_GW170814_Poly") as Material;
            }
        });
    }
}
