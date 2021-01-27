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
						if (rayHit.collider.tag == "PlayArea Plane") {
							Debug.Log($"[ToolBehaviour] Hit: {rayHit.collider.gameObject.name}");
							var pointInCity = CityManager.Instance.transform.InverseTransformPoint(rayHit.point);
							CmdSpawnTool(ctrlManager.SelectedTool, pointInCity);
						}
					}
				}

				hasTapBegin = false;
				return;
			}
		}
    }

	[Command]
	private void CmdSpawnTool(ARTool selectedTool, Vector3 point, NetworkConnectionToClient conn=null) {
		switch (selectedTool) {
			case ARTool.InfoPin:
				SpawnInfoPin(point, conn);
				break;
			case ARTool.Ping:
				SpawnPing(point, conn);
				break;
		}
	}

	private void SpawnInfoPin(Vector3 point, NetworkConnection conn) {
		InfoPin infoPin = Instantiate(InfoPinPrefab).GetComponent<InfoPin>();
		infoPin.PinToCity(point);
		infoPin.SetUsername(GetComponentInParent<ARUser>().username);

		NetworkServer.Spawn(infoPin.gameObject, conn);
	}

	private void SpawnPing(Vector3 point, NetworkConnection conn) {
		PingObject ping = Instantiate(PingPrefab).GetComponent<PingObject>();
		ping.PinToCity(point);
		ping.username = GetComponentInParent<ARUser>().username;
		// TODO: set color

		NetworkServer.Spawn(ping.gameObject, conn);

		if (pingReference != null) {
			pingReference.GetComponent<PingObject>().UnPinToCity();
		}
		pingReference = ping.gameObject;
	}
}
