using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ToolBehaviour : NetworkBehaviour
{
	[SerializeField] private GameObject PingPrefab;
	[SerializeField] private GameObject InfoPinPrefab;

	private ControlsManager ctrlManager;
	/// <summary>
	/// If touch is held longer than .1 second, do not consider it as a 'tap'
	/// </summary>
	private float tapReleaseThreshold = .3f;
	private float timePressed = 0;

	private bool hasTapBegin = false;
	private LayerMask mask;

	[SyncVar]
	public GameObject pingReference = null;

	// Start is called before the first frame update
	void Start()
    {
		ctrlManager = ControlsManager.Instance;
		mask = LayerMask.GetMask("CityPlane","Pinnables","Issue");
	}

    // Update is called once per frame
    void Update()
    {
		if (!hasAuthority)
			return;

		if (Input.touchCount != 1)
			return;

		if (ctrlManager.SelectedTool == ARTool.Hand)
			return;

		Touch t = Input.GetTouch(0); ;
		if (!hasTapBegin) {
			// Tap Begin.
			hasTapBegin = true;
			timePressed = 0;
		}
		else {
			// Tap Continue.
			timePressed += Time.deltaTime;
			if (t.phase == TouchPhase.Moved) {
				// Tap Moved. Cancel tracking
				hasTapBegin = false;
				return;
			}

			if (t.phase == TouchPhase.Ended) {
				// Tap End.
				if ( timePressed <= tapReleaseThreshold) {
					// Raycast.
					Ray r = Camera.main.ScreenPointToRay(t.position);
					RaycastHit rayHit;
					if (Physics.Raycast(r, out rayHit, Mathf.Infinity, mask.value)) {
						// Pin Object.
						if (rayHit.collider.tag == "CityGML") {
							Debug.Log($"[ToolBehaviour] Hit: {rayHit.collider.gameObject.name}");
							CmdSpawnTool(rayHit.point);
						}
					}
				}

				hasTapBegin = false;
				return;
			}
		}
    }

	[Command]
	private void CmdSpawnTool(Vector3 point) {
		switch (ctrlManager.SelectedTool) {
			case ARTool.InfoPin:
				SpawnInfoPin(point);
				break;
			case ARTool.Ping:
				SpawnPing(point);
				break;
		}
	}

	private void SpawnInfoPin(Vector3 point) {
		InfoPin infoPin = Instantiate(InfoPinPrefab).GetComponent<InfoPin>();
		infoPin.PinToCity(point);
		infoPin.SetUsername(GetComponentInParent<ARUser>().username);

		NetworkServer.Spawn(infoPin.gameObject);
		infoPin.netIdentity.AssignClientAuthority(netIdentity.connectionToClient);
	}

	private void SpawnPing(Vector3 point) {
		PingObject ping = Instantiate(PingPrefab).GetComponent<PingObject>();
		ping.PinToCity(point);
		NetworkServer.Spawn(ping.gameObject);

		ping.netIdentity.AssignClientAuthority(netIdentity.connectionToClient);
		ping.username = GetComponentInParent<ARUser>().username;
		// TODO: set color


		if (pingReference != null) {
			pingReference.GetComponent<PingObject>().UnPinToCity();
		}
		pingReference = ping.gameObject;
	}
}
