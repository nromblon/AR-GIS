using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Mirror;

public class ChatBehaviour : NetworkBehaviour
{
	[SerializeField] private ARUser user;
	[SerializeField] private GameObject chatUI;
	[SerializeField] private TextMeshProUGUI chatText;

	private static int MaxMessages = 5;
	private static float messageDuration = 4f;
	private static event Action<string> OnMessage;

	private int messageCount = 0;

	private Coroutine activeTimer;

	public override void OnStartAuthority() {
		OnMessage += HandleMessage;
		// Notify the server that a client has connected
		Send($"{user.username} has joined the server.");
		Debug.Log("[ChatBehaviour] OnStartAuthority() called");
		// should only be called once.
	}

	[ClientCallback]
	private void OnApplicationQuit() {
		if (!hasAuthority)
			return;

		OnMessage -= HandleMessage;
	}

	private void HandleMessage(string message) {
		Debug.Log("[ChatBehaviour] Handling message");
		if (messageCount >= MaxMessages) {
			Debug.Log($"[ChatBehaviour] {messageCount} >={MaxMessages}, Removing Lines");
			chatText.text = RemoveLines(chatText.text);
			messageCount -= 1;
		}

		if (messageCount > 0)
			message = $"\n{message}";

		chatText.text += message;
		messageCount += 1;

		if (!chatUI.activeSelf)
			chatUI.SetActive(true);

		activeTimer = StartCoroutine(TimerToHide());
	}

	/// <summary>
	/// Removes the first x [numLines] lines from a text.
	/// </summary>
	/// <param name="text"></param>
	/// <param name="numLines"></param>
	/// <returns></returns>
	private string RemoveLines(string text, int numLines = 1) {
		var lines = text.Split(new[] { $"\n" }, StringSplitOptions.None);
		string newText = "";
		for (int i = numLines; i < lines.Length; i++) {
			newText += lines[i];
		}
		return newText;
	}

	[Client]
	public void Send(string message) {
		CmdSendMessage(message);
	}

	[Command]
	private void CmdSendMessage(string message) {
		RpcHandleMessage(message);
	}

	[ClientRpc]
	private void RpcHandleMessage(string message) {
		OnMessage?.Invoke(message);
	}

	IEnumerator TimerToHide() {
		float t = 0;
		while (t < messageDuration) {
			yield return null;
			t += Time.deltaTime;
		}

		chatUI.SetActive(false);
		activeTimer = null;
	}
}
