using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

class PermissionUI : UI {
    const float CLOSE_TIME = 7.0f;

    protected override string ReferenceName { get; } = "Permissions";

    public TextMeshProUGUI PermissionDeniedText { get; private set; }

    public new void Init(StateMachine state_machine, Image background, UIContainer UI_container) {
        base.Init(state_machine, background, UI_container);
        
        PermissionDeniedText = transform.Find("Permission Denied Text").GetComponent<TextMeshProUGUI>();
    }

    public override IEnumerator Disable() {
        gameObject.SetActive(false);

        yield return null;
    }

    public override void DisableInstant() {
        _UI_container.Cover.enabled = false;
        gameObject.SetActive(false);
    }

    public IEnumerator PermissionDenied() {
        PermissionDeniedText.gameObject.SetActive(true);

        yield return new WaitForSeconds(CLOSE_TIME);

        Application.Quit();
    }

    public override IEnumerator Enable() {
        _UI_container.Cover.enabled = true;
        PermissionDeniedText.gameObject.SetActive(false);
        yield return null;
    }
}
