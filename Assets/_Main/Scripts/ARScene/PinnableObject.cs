using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class PinnableObject : NetworkBehaviour
{
	[SerializeField] private bool keepScale = true;
	private Transform cityParentTf;

	private float fixedScale;
	private bool isPinned = false;

    // Start is called before the first frame update
    protected void Start()
    {
		cityParentTf = CityManager.Instance.transform;

		//var canvas = GetComponentInChildren<Canvas>();
		//canvas.worldCamera = GameObject.FindGameObjectWithTag("UI Camera").GetComponent<Camera>();
	}

	// Update is called once per frame
	protected void Update()
    {
		if (!keepScale)
			return;

		if (!isPinned)
			return;

		if (cityParentTf.hasChanged) {
			//var parentScale = cityParentTf.localScale;
			//transform.localScale = new Vector3(fixedScale / parentScale.x, fixedScale / parentScale.y, fixedScale / parentScale.z);
			//transform.localScale = transform.localScale;
			cityParentTf.hasChanged = false;
		}
    }

	public abstract void ShowVisuals(bool val);

	public void PinToCity(Vector3 worldPos) {
		if (cityParentTf == null)
			cityParentTf = CityManager.Instance.transform;
		transform.SetParent(cityParentTf, false);
		//transform.localScale = Vector3.one;
		fixedScale = transform.localScale.x;
		transform.position = worldPos;
		
		// TODO: set proper initial scale

		Debug.Log($"[PinnableObject] Pin Parent: {transform.parent.name}");
		isPinned = true;
	}

	public void UnPinToCity() {

		isPinned = false;
		CmdUnpinToCity();
	}

	[Command]
	private void CmdUnpinToCity() {
		NetworkServer.Destroy(gameObject);
	}
}
