using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixCityAR;
using Mirror;

public class SystemMessagesPanel : NetworkBehaviour
{
	[Header("Setup")]
	[SerializeField] private GameObject messagePrefab;
	[SerializeField] private Transform content;
	[Header("Config")]
	[SerializeField] private int maxMessages;
	[SerializeField] private float messageDuration = 5;

	private static SystemMessagesPanel intance;
	public static SystemMessagesPanel Instance;

	private Queue<SystemMessage> messageQ;
	private Coroutine activeTimer;

    // Start is called before the first frame update
    void Start()
    {
		messageQ = new Queue<SystemMessage>();
		content.gameObject.SetActive(false);
		Instance = this;
    }

	[ClientRpc]
    public void RpcAddMessage(string username, MessageType type) {
		content.gameObject.SetActive(true);

		SystemMessage toAdd = Instantiate(messagePrefab, content).GetComponent<SystemMessage>();
			
		if(messageQ.Count < maxMessages) {
			messageQ.Enqueue(toAdd);
		}
		else {
			SystemMessage toRemove = messageQ.Dequeue();
			Destroy(toRemove.gameObject);
			messageQ.Enqueue(toAdd);
		}

		if (activeTimer != null)
			StopCoroutine(activeTimer);
		activeTimer = StartCoroutine(TimerToHide());
	}

	IEnumerator TimerToHide() {
		float t = 0;
		while (t < messageDuration) {
			yield return null;
			t += Time.deltaTime;
		}

		content.gameObject.SetActive(false);
		activeTimer = null;
	}
}
