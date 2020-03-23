using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckBox : Button {
    public bool IsOn { get; private set; } = false;

    public void Init(bool is_on) {
        IsOn = is_on;
        if (IsOn)
            GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/System/Checkbox On");
        else
            GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/System/Checkbox Off");
    }

    public bool Toggle() {
        if (!IsOn) {
            GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/System/Checkbox On");
            IsOn = true;
        }
        else {
            GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/System/Checkbox Off");
            IsOn = false;
        }

        return IsOn;
    }
}
