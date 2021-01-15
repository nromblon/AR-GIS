using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsAnimatorCallback : MonoBehaviour
{
	private ControlsManager CtrlManager;
    // Start is called before the first frame update
    void Start()
    {
		CtrlManager = ControlsManager.Instance;
    }

    public void OnToolCatalogShown() {
		CtrlManager.OnToolAnimShown();
	}

	public void OnToolCatalogHide() {
		CtrlManager.OnToolAnimHidden();
	}
}
