using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public struct TfValues { 
	public Vector3 localPosition;
	public Quaternion localRotation;
	/// <summary>
	/// For CityManager, this value is multiplied because the localscale values can be so small that it is rounded to zero when serialized.
	/// </summary>
	public Vector3 localScale;
}

public class PlayAreaTfMessage : NetworkMessage {
	public NetworkIdentity sender;

	/// <summary>
	/// If true, then both playAreaTf and cityTf are empty, and the message is used to signify that the
	/// Play Area is removed.
	/// </summary>
	public bool isRemove = false;

	/// <summary>
	/// The current state of the Play Area Object's transform. Only changes when
	/// PlayArea has been removed and re-set.
	/// </summary>
	public TfValues playAreaTf;

	/// <summary>
	/// The scale for the container object of the city. 
	/// </summary>
	public Vector3 cityContainerScale;
		
	/// <summary>
	/// Current state of City Module's Transform. Useful for when a client joins 
	/// the server wherein the City has already been moved around.
	/// </summary>
	public TfValues cityTf;

}

public class CityTfMessage : NetworkMessage {
	public TfValues cityTf;
	public NetworkIdentity sender;
}

public class CityTransformBehaviour : NetworkBehaviour {
	//public const float scaleMultipler = 1000;

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
			var cityTfVal = GetCityTfValues();
			Debug.Log($"[{this.GetType().Name}] City Instance transform has been manipulated. Sending update");
			Debug.Log($"[{this.GetType().Name}] {TfValuesString(cityTfVal)}");
			CmdUpdateCityTf(netIdentity, GetCityTfValues(true));
			CityInstance.HasManipulationPerformed = false;
		}
	}

	#region Play Area Placement and Removal Functions

	/// <summary>
	/// Currently should only be called by the Host Device.
	/// </summary>
	public void OnPlayAreaConfirm() {
		Debug.Log($"[{this.GetType().Name}] OnPlayAreaConfirm() called.");
		PlayAreaInstance = FindObjectOfType<PlayAreaController>();
		CityInstance = PlayAreaInstance.GetComponentInChildren<CityManager>();

		NetworkServer.SendToAll(new PlayAreaTfMessage {
			playAreaTf = GetPlayAreaTfValues(),
			cityContainerScale = PlayAreaInstance.CityContainer.transform.localScale,
			cityTf = GetCityTfValues(),
			sender = netIdentity
		});
	}
	
	public void OnPlayAreaRemoved() {
		Debug.Log($"[{this.GetType().Name}] OnPlayAreaRemoved() called.");
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
			var ctContainerScale = tfMsg.cityContainerScale;
			PlayAreaManager.Instance.ClientCreatePlayArea(paTf, cTf, ctContainerScale);
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
		ARSceneController.Instance._ShowAndroidToastMessage("Requesting Play Area from Host");
		CmdRequestPlayAreaTf();
	}

	/// <summary>
	/// Called when a newly joined client requests for the current play area state.
	/// </summary>
	/// <param name="sender"></param>
	[Command]
	public void CmdRequestPlayAreaTf() {
		Debug.Log("[CityTransformBehaviour] PlayAreaTf Request received from client.");
		// If PlayArea object has been removed. Wait until Play Area Instance is back.		Debug.Log($"[{this.GetType().Name}] Checking if can send play area values...");

		Debug.Log($"[{this.GetType().Name}] hasPlayAreaInitialized: {PlayAreaManager.Instance.hasConfirmedPlayArea}");
		if (!PlayAreaManager.Instance.hasConfirmedPlayArea) {
			TargetRetryPlayAreaTfRequest();
			return;
		}

		var paTf = GetPlayAreaTfValues();
		var cityTf = GetCityTfValues();
		var ctContainerScale = PlayAreaInstance.CityContainer.transform.localScale;
		Debug.Log("[CityTransformBehaviour] Sending TargetSendPlayAreaTf...");
		Debug.Log($"[{this.GetType().Name}] paTf Values: {TfValuesString(paTf)}");
		Debug.Log($"[{this.GetType().Name}] cityTf Values: {TfValuesString(cityTf)}");
		TargetSendPlayAreaTf(paTf, cityTf, ctContainerScale);
	}

	/// <summary>
	/// ---- still thinking if this will work.
	/// </summary>
	/// <param name="requester"></param>
	[TargetRpc]
	public void TargetSendPlayAreaTf(TfValues paTf, TfValues cityTf, Vector3 cityContainerScale) {

		Debug.Log("[CityTransformBehaviour] Play Area Request Success, Setting playArea values...");
		Debug.Log($"[{this.GetType().Name}] paTf Values: {TfValuesString(paTf)}");
		Debug.Log($"[{this.GetType().Name}] cityTf Values: {TfValuesString(cityTf)}");

		// Create and place play area
		PlayAreaManager.Instance.ClientCreatePlayArea(paTf, cityTf, cityContainerScale);

		PlayAreaInstance = FindObjectOfType<PlayAreaController>();
		CityInstance = PlayAreaInstance.GetComponentInChildren<CityManager>();

		Debug.Log("[CityTransformBehaviour] Play Area values set.");
	}

	[TargetRpc]
	public void TargetRetryPlayAreaTfRequest() {
		Debug.Log("[CityTransformBehaviour] Play Area Request Failed. Retrying again.");
		ARSceneController.Instance._ShowAndroidToastMessage("Play Area request failed. Trying again");
		StartCoroutine(WaitAndTryRequestPlayArea());
	}
	#endregion

	#region City Module Transform Syncing Functions
	[Command]
	public void CmdUpdateCityTf(NetworkIdentity sender, TfValues cityTf) {
		var user = GetComponent<ARUser>().username;
		Debug.Log($"[{this.GetType().Name}] {user} called CmdUpdateCityTf(). Sending tfData");
		Debug.Log($"[{this.GetType().Name}] {TfValuesString(cityTf)}");
		NetworkServer.SendToAll(new CityTfMessage {
			cityTf = cityTf,
			sender = sender
		});
	}

	public void OnCityTfUpdate(NetworkConnection conn, CityTfMessage CTfMsg) {
		var user = GetComponent<ARUser>().username;
		Debug.Log($"[{this.GetType().Name}] {user} called OnCityUpdateTf()");

		// Ignore sender
		if (CTfMsg.sender == netIdentity) {
			Debug.Log($"[{this.GetType().Name}] {user} sender is same as current. ignore message.");
			// client calls might've lead to this.
			return;
		}


		if (!PlayAreaManager.Instance.hasConfirmedPlayArea)
			return;

		Debug.Log($"[{this.GetType().Name}] {user} || cityTf values: {TfValuesString(CTfMsg.cityTf)}");
		// Set CityInstance values
		//CTfMsg.cityTf.localScale = CTfMsg.cityTf.localScale / scaleMultipler;
		//Debug.Log($"[{this.GetType().Name}] {user} || After removing scaleMultiplier: {TfValuesString(CTfMsg.cityTf)}");
		CityInstance.ClientUpdateTransform(CTfMsg.cityTf);
	}
	#endregion

	#region Utility Functions
	private TfValues GetPlayAreaTfValues() {
		if (PlayAreaInstance == null) {
			PlayAreaInstance = FindObjectOfType<PlayAreaController>();
			CityInstance = PlayAreaInstance.GetComponentInChildren<CityManager>();
		}

		return new TfValues {
			localPosition = PlayAreaInstance.transform.localPosition,
			localRotation = PlayAreaInstance.transform.localRotation,
			localScale = PlayAreaInstance.transform.localScale
		};
	}

	private string TfValuesString(TfValues tf) {
		string scaleTiny = $"( {tf.localScale.x}, {tf.localScale.y}, {tf.localScale.z})";
		return $"localPosition: {tf.localPosition} | localRotation: {tf.localRotation} | localScale: {scaleTiny}";
	}
	
	private TfValues GetCityTfValues(bool multiplyScale = false) {
		if (CityInstance == null) {
			PlayAreaInstance = FindObjectOfType<PlayAreaController>();
			CityInstance = PlayAreaInstance.GetComponentInChildren<CityManager>();
		}

		if (multiplyScale) {
			return new TfValues {
				localPosition = CityInstance.transform.localPosition,
				localRotation = CityInstance.transform.localRotation,
				localScale = CityInstance.transform.localScale
			};
		}
		else {
			return new TfValues {
				localPosition = CityInstance.transform.localPosition,
				localRotation = CityInstance.transform.localRotation,
				localScale = CityInstance.transform.localScale
			};
		}
	}

	private IEnumerator WaitAndTryRequestPlayArea () {
		Debug.Log($"[CityTransformBehaviour] Waiting for {waitInterval} seconds before requesting again.");
		yield return new WaitForSeconds(waitInterval);
		Debug.Log("[CityTransformBehaviour] Wait Complete.");

		RequestPlayAreaTf();
	}
	#endregion
}
