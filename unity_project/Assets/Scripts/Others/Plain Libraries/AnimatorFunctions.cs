using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class AnimatorFunctions {
    public static IEnumerator LinearFade<T>(bool fade_out, T graphic, float time, float max_alpha = 1) where T : Graphic {
        // fade from opaque to transparent
        if (fade_out) {
            // loop backwards
            for (float i = 1; i >= 0; i -= Time.deltaTime / time) {
                // set color with i as alpha
                graphic.color = new Color(graphic.color.r, graphic.color.b, graphic.color.g, i * max_alpha);
                yield return null;
            }
            graphic.color = new Color(graphic.color.r, graphic.color.b, graphic.color.g, 0);
        }
        // fade from transparent to opaque
        else {
            // loop
            for (float i = 0; i <= 1; i += Time.deltaTime / time) {
                // set color with i as alpha
                graphic.color = new Color(graphic.color.r, graphic.color.b, graphic.color.g, i * max_alpha);
                yield return null;
            }
            graphic.color = new Color(graphic.color.r, graphic.color.b, graphic.color.g, max_alpha);
        }
    }

    public static IEnumerator LinearFade(bool fade_out, Material mat, float time, float max_alpha = 1) {
        // fade from opaque to transparent
        if (fade_out) {
            // loop backwards
            for (float i = 1; i >= 0; i -= Time.deltaTime / time) {
                // set color with i as alpha
                mat.color = new Color(mat.color.r, mat.color.b, mat.color.g, i * max_alpha);
                yield return null;
            }
            mat.color = new Color(mat.color.r, mat.color.b, mat.color.g, 0);
        }
        // fade from transparent to opaque
        else {
            // loop
            for (float i = 0; i <= 1; i += Time.deltaTime / time) {
                // set color with i as alpha
                mat.color = new Color(mat.color.r, mat.color.b, mat.color.g, i * max_alpha);
                yield return null;
            }
            mat.color = new Color(mat.color.r, mat.color.b, mat.color.g, max_alpha);
        }
    }
}
