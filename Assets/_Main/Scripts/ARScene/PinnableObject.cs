using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PinnableObject : NetworkBehaviour
{
	[SerializeField] private bool keepScale = true;
	private Transform cityParentTf;

	private float fixedScale;
	private bool isPinned = false;

    // Start is called before the first frame update
    protected void Start()
    {
		fixedScale = transform.localScale.x;
		cityParentTf = CityManager.Instance.transform;
    }

    // Update is called once per frame
    protected void Update()
    {
		if (!keepScale)
			return;

		if (!isPinned)
			return;

		if (cityParentTf.hasChanged) {
			var parentScale = cityParentTf.localScale;
			transform.localScale = new Vector3(fixedScale / parentScale.x, fixedScale / parentScale.y, fixedScale / parentScale.z);
			cityParentTf.hasChanged = false;
		}

    }

	public void PinToCity(Vector3 worldPos) {
		transform.position = worldPos;
		if (cityParentTf == null)
			cityParentTf = CityManager.Instance.transform;
		transform.SetParent(cityParentTf, false);
		// set proper initial scale

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
