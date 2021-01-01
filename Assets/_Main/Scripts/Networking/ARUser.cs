using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ARUser : NetworkBehaviour {

	[SyncVar]
	public string username;
	[SyncVar]
	public bool isHost;

	// Start is called before the first frame update
	void Start() {
		transform.position = Vector3.zero;
		if (hasAuthority) {
			username = PlayerPrefs.GetString("username");
			Debug.Log($"[ARUser] Setting username: {username}");
		}

		isHost = isServer;
		Debug.Log($"[ARUser] Am I host?: {isHost}");
		if (isHost) {
			ARSceneController.Instance.applicationMode = ApplicationMode.Host;
		}
		else {
			ARSceneController.Instance.applicationMode = ApplicationMode.Client;
		}

		Debug.Log($"[ARUser] ARSceneController application mode : {ARSceneController.Instance.applicationMode.ToString()}");
	}
}
