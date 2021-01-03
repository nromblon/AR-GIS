using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public struct TfValues { 
	public Vector3 localPosition;
	public Quaternion localRotation;
	public Vector3 localScale;
}

public class PlayAreaTfMessage : NetworkMessage {
	public NetworkIdentity sender;

	/// <summary>
	/// If true, then both playAreaTf and cityTf are empty, and the message is used to signify that the
	/// Play Area is removed.
	/// </summary>
	public bool isRemove = false;

	// The current state of the Play Area Object's transform. Only changes when
	// PlayArea has been removed and re-set.
	public TfValues playAreaTf;

	// Current state of City Module's Transform. Useful for when a client joins 
	// the server wherein the City has already been moved around.
	public TfValues cityTf;

}

public class CityTfMessage : NetworkMessage {
	public TfValues cityTf;
	public NetworkIdentity sender;
}

public class CityTransformBehaviour : NetworkBehaviour {
	private float waitInterval = 3f;

	private PlayAreaController PlayAreaInstance;
	private CityManager CityInstance;

	private NewNetworkManager netManager;

	// Start is called before the first frame update
	void Start() {
		netManager = (NewNetworkManager)NewNetworkManager.singleton;

		if (hasAuthority) {
			NetworkClient.RegisterHandler<PlayAreaTfMessage>(OnPlayAreaTfUpdate);
			NetworkClient.RegisterHandler<CityTfMessage>(OnCityTfUpdate);
		}
	}

	// Update is called once per frame
	void Update() {
		if (!hasAuthority)
			return;

		if (PlayAreaInstance == null || CityInstance == null)
			return;

		// Send a command to notify the other clients if 
		if (CityInstance.HasManipulationPerformed) {
			CmdUpdateCityTf(netIdentity, GetCityTfValues());
			CityInstance.HasManipulationPerformed = false;
		}
	}

	#region Play Area Placement and Removal Functions

	/// <summary>
	/// Currently should only be called by the Host Device.
	/// </summary>
	[Server]
	public void OnPlayAreaConfirm() {
		PlayAreaInstance = FindObjectOfType<PlayAreaController>();
		CityInstance = PlayAreaInstance.GetComponentInChildren<CityManager>();

		NetworkServer.SendToAll(new PlayAreaTfMessage {
			playAreaTf = GetPlayAreaTfValues(),
			cityTf = GetCityTfValues(),
			sender = netIdentity
		});
	}

	[Server]
	public void OnPlayAreaRemoved() {
		PlayAreaInstance = null;
		CityInstance = null;

		NetworkServer.SendToAll(new PlayAreaTfMessage {
			isRemove = true,
			sender = netIdentity
		});
	}

	/// <summary>
	/// Handles the PlayAreaTfMessage. 
	/// If isRemove flag is true, removes the current play area manager instance.
	/// If isRemove flag is false, creates a new play area manager. Then this must be called after new anchor is resolved!
	/// </summary>
	/// <param name="conn"></param>
	/// <param name="tfMsg"></param>
	public void OnPlayAreaTfUpdate(NetworkConnection conn, PlayAreaTfMessage tfMsg) {
		if (tfMsg.sender == netIdentity)
			return;

		if (tfMsg.isRemove) {
			// Remove Play Area
			PlayAreaManager.Instance.ClientRemovePlayArea();
			PlayAreaInstance = null;
			CityInstance = null;
		}
		else {
			// Create and place play area
			var paTf = tfMsg.playAreaTf;
			var cTf = tfMsg.cityTf;
			PlayAreaManager.Instance.ClientCreatePlayArea(paTf, cTf);
			PlayAreaInstance = FindObjectOfType<PlayAreaController>();
			CityInstance = PlayAreaInstance.GetComponentInChildren<CityManager>();
		}
	}
	#endregion

	#region Play Area Request and Reply Functions

	/// <summary>
	/// Call this from outside to request the server for a clone of the play area.
	/// Only to be called after Resolving Cloud anchor is success.
	/// </summary>
	public void RequestPlayAreaTf() {

		Debug.Log("[CityTransformBehaviour] Sending PlayAreaTf Request");
		CmdRequestPlayAreaTf();
	}

	/// <summary>
	/// Called when a newly joined client requests for the current play area state.
	/// </summary>
	/// <param name="sender"></param>
	[Command]
	public void CmdRequestPlayAreaTf() {

		Debug.Log("[CityTransformBehaviour] PlayAreaTf Request received from client.");
		// If PlayArea object has been removed. Wait until 
		if (PlayAreaInstance == null || CityInstance == null) {
			TargetRetryPlayAreaTfRequest();
			return;
		}
		
		Debug.Log("[CityTransformBehaviour] Sending TargetSendPlayAreaTf...");
		TargetSendPlayAreaTf(GetPlayAreaTfValues(), GetCityTfValues());
	}

	/// <summary>
	/// ---- still thinking if this will work.
	/// </summary>
	/// <param name="requester"></param>
	[TargetRpc]
	public void TargetSendPlayAreaTf(TfValues paTf, TfValues cityTf) {

		Debug.Log("[CityTransformBehaviour] Play Area Request Success, Setting playArea values...");
		// Create and place play area
		PlayAreaManager.Instance.ClientCreatePlayArea(paTf, cityTf);
		PlayAreaInstance = FindObjectOfType<PlayAreaController>();
		CityInstance = PlayAreaInstance.GetComponentInChildren<CityManager>();
		Debug.Log("[CityTransformBehaviour] Play Area values set.");
	}

	[TargetRpc]
	public void TargetRetryPlayAreaTfRequest() {
		Debug.Log("[CityTransformBehaviour] Play Area Request Failed. Retrying again.");
		RetryPlayAreaTfRequest();
	}

	private void RetryPlayAreaTfRequest() {
		StartCoroutine(WaitAndTryRequestPlayArea());
	}
	#endregion

	#region City Module Transform Syncing Functions
	[Command]
	public void CmdUpdateCityTf(NetworkIdentity sender, TfValues cityTf) {
		NetworkServer.SendToAll(new CityTfMessage {
			cityTf = cityTf,
			sender = sender
		});
	}

	public void OnCityTfUpdate(NetworkConnection conn, CityTfMessage CTfMsg) {
		// Ignore sender
		if (CTfMsg.sender == netIdentity)
			return;

		// Set CityInstance values
		CityInstance.ClientUpdateTransform(CTfMsg.cityTf);
	}
	#endregion

	#region Utility Functions
	private TfValues GetPlayAreaTfValues() {
		return new TfValues {
			localPosition = PlayAreaInstance.transform.localPosition,
			localRotation = PlayAreaInstance.transform.localRotation,
			localScale = PlayAreaInstance.transform.localScale
		};
	}

	private TfValues GetCityTfValues() {
		return new TfValues {
			localPosition = CityInstance.transform.localPosition,
			localRotation = CityInstance.transform.localRotation,
			localScale = CityInstance.transform.localScale
		};
	}

	private IEnumerator WaitAndTryRequestPlayArea () {
		Debug.Log($"[CityTransformBehaviour] Waiting for {waitInterval} seconds before requesting again.");
		yield return new WaitForSeconds(waitInterval);
		Debug.Log("[CityTransformBehaviour] Wait Complete.");

		RequestPlayAreaTf();
	}
	#endregion
}
