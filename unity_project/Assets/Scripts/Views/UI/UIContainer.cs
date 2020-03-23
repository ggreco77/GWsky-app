using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class UIContainer : MonoBehaviour {
    public MainMenuUI MainMenuUI { get; private set; }
    public SelectEventUI SelectEventUI { get; private set; }

    public LoadingUI LoadingUI { get; private set; }

    public LookAroundUI LookAroundUI { get; private set; }

    public CreditsUI CreditsUI { get; private set; }
    public OptionsUI OptionsUI { get; private set; }

    public PermissionUI PermissionUI { get; private set; }

    public BackgroundMovingSphere UISphere { get; private set; }

    public Canvas UICanvas { get; private set; }

    public Image Background { get; private set; }

    public Image Cover { get; private set; }

    public void Init(StateMachine state_machine, EventDatabase events_db, Transform photosphere, CameraRig camera, SphereAligner sphere_aligner, SphereText sphere_text) {
        Background = transform.Find("UI Canvas/Background").GetComponent<Image>();
        Cover = transform.Find("UI Canvas/Complete Cover").GetComponent<Image>();
        Cover.enabled = false;

        UISphere = transform.Find("UI Sphere").GetComponent<BackgroundMovingSphere>();
        UISphere.Init(transform.Find("UI Canvas/Video Cover").GetComponent<Image>());
        
        MainMenuUI = transform.Find("UI Canvas/Main Menu UI").GetComponent<MainMenuUI>();
        MainMenuUI.Init(state_machine, Background, this);

        SelectEventUI = transform.Find("UI Canvas/Select Event UI").GetComponent<SelectEventUI>();
        SelectEventUI.Init(state_machine, Background, this, events_db);

        OptionsUI = transform.Find("UI Canvas/Options UI").GetComponent<OptionsUI>();
        OptionsUI.Init(state_machine, Background, this);

        CreditsUI = transform.Find("UI Canvas/Credits UI").GetComponent<CreditsUI>();
        CreditsUI.Init(state_machine, Background, this);

        LoadingUI = transform.Find("UI Canvas/Loading UI").GetComponent<LoadingUI>();
        LoadingUI.Init(state_machine, Background, this, photosphere, events_db, sphere_aligner);

        LookAroundUI = transform.Find("UI Canvas/LookAround UI").GetComponent<LookAroundUI>();
        LookAroundUI.Init(state_machine, Background, this, camera, sphere_aligner, sphere_text, events_db, photosphere);

        PermissionUI = transform.Find("UI Canvas/Permission UI").GetComponent<PermissionUI>();
        PermissionUI.Init(state_machine, Background, this);

        UICanvas = transform.Find("UI Canvas").GetComponent<Canvas>();

        MainMenuUI.DisableInstant();
        SelectEventUI.DisableInstant();
        OptionsUI.DisableInstant();
        CreditsUI.DisableInstant();
        LoadingUI.DisableInstant();
        LookAroundUI.DisableInstant();
        PermissionUI.DisableInstant();
    }
}
