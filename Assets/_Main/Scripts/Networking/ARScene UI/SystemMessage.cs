using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum MessageType {
	Join, Leave
}

public class SystemMessage : MonoBehaviour
{
	[Header("Setup")]
	[SerializeField] private TextMeshProUGUI tm;


    public void SetMessage(MessageType type, string content) {
		switch (type) {
			case MessageType.Join:
				tm.text = content + " has joined the session";
				break;
			case MessageType.Leave:
				tm.text = content + " has left the session";
				break;
		}
	}
}
