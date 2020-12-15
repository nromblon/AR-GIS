using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixCityAR;

public class ServerList : MonoBehaviour {
	public GameObject itemPrefab;

	[SerializeField] private Transform contentTf;

	private List<ServerList> serverList;

	private Dictionary<long, ServerListItem> serverDict;

	public ServerListItem selectedItem;

	public event System.Action ItemSelected = delegate { };

    // Start is called before the first frame update
    void Start()
    {
		serverList = new List<ServerList>();
		serverDict = new Dictionary<long, ServerListItem>();
    }

	public void AddItem(DiscoveryResponse info) {
		if (serverDict.ContainsKey(info.serverId)) {
			serverDict[info.serverId].SetServerInfo(info);
		}
		else {
			ServerListItem item = Instantiate(itemPrefab).GetComponent<ServerListItem>();
			item.transform.SetParent(contentTf);
			item.SetServerInfo(info);

			serverDict[info.serverId] = item;
		}
		
	}

	public void OnItemSelected(long itemIdx) {
		var newSelected = serverDict[itemIdx];

		if (newSelected == selectedItem) {
			selectedItem.SetSelected(false);
			selectedItem = null;
		}
		else {
			selectedItem = newSelected;
			selectedItem.SetSelected(true);
		}
		ItemSelected.Invoke();
	}

	public void ClearList() {
		foreach(KeyValuePair<long, ServerListItem> entry in serverDict) {
			Destroy(entry.Value.gameObject);
		}

		serverList.Clear();
	}
}
