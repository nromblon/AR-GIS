using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ARUser : NetworkBehaviour {

	[SyncVar]
	public string username;

	// Start is called before the first frame update
	void Start() {
		transform.position = Vector3.zero;
		if (hasAuthority) {
			username = PlayerPrefs.GetString("username");
			Debug.Log($"Setting username: {username}");
		}
	}
}
