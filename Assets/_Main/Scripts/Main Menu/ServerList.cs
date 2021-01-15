using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixCityAR;

public class ServerList : MonoBehaviour {
	public GameObject itemPrefab;

	[Header("Setup")]
	[SerializeField] private Transform contentTf;
	[SerializeField] private ServerDetails detailsPanel;
	[SerializeField] private NetworkMenu networkMenu;

	private List<ServerList> servers;
	private Dictionary<long, ServerListItem> serverDict;

	[Header("Runtime Data")]
	public ServerListItem selectedItem;

	public event System.Action ItemSelected = delegate { };

    // Start is called before the first frame update
    void Start()
    {
		servers = new List<ServerList>();
		serverDict = new Dictionary<long, ServerListItem>();
    }

	public void AddItem(DiscoveryResponse info) {
		if (serverDict.ContainsKey(info.serverId)) {
			serverDict[info.serverId].SetServerInfo(info);
		}
		else {
			ServerListItem item = Instantiate(itemPrefab,contentTf).GetComponent<ServerListItem>();
			item.SetServerInfo(info);

			serverDict[info.serverId] = item;
		}
	}

	public void OnItemSelected(long serverId) {
		var newSelected = serverDict[serverId];
		Debug.Log($"[ServerList] Selected item idx: {newSelected}");
		if (newSelected == selectedItem) {
			Debug.Log($"[ServerList] newSelected == Selected item");
			selectedItem.SetSelected(false);
			selectedItem = null;
			detailsPanel.SetDetails(null);

		}
		else {
			Debug.Log($"[ServerList] newSelected: {newSelected.Info.serverName}");
			selectedItem = newSelected;
			detailsPanel.SetDetails(newSelected.Info);
			selectedItem.SetSelected(true);
		}
		networkMenu.CheckIfButtonsEnable();
	}

	public void ClearList() {
		foreach (Transform child in contentTf) {
			Destroy(child.gameObject);
		}
		detailsPanel.SetDetails(null);

		if(serverDict != null)
			serverDict.Clear();

		if (servers != null)
			servers.Clear();
	}
}
