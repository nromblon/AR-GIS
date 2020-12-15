using Assets.Scripts.NetworkMessages;
using Assets.Scripts.Utility.Serialisation;
using System.Collections.Generic;
using UnityEngine;
using Mirror.Discovery;
using kcp2k;
using FixCityAR;

public class DiscoveryHUDController : MonoBehaviour
{
	Dictionary<string, DiscoveryResponse> discoveredServers = new Dictionary<string, DiscoveryResponse>();
	string[] m_headerNames = new string[] { "IP", "Host" };
	Vector2 m_scrollViewPost = Vector2.zero;

	public NewNetworkDiscovery networkDiscovery;
	[SerializeField] private NetworkMenu networkMenu;

	public void FindServers() {
		discoveredServers.Clear();
		networkDiscovery.StartDiscovery();
	}

	public void StopFinding() {
		networkDiscovery.StopDiscovery();
		discoveredServers.Clear();
	}

	public void StartHost(string hostname, string username) {
		discoveredServers.Clear();

		((NewNetworkManager)NewNetworkManager.singleton).username = username;
		((NewNetworkManager)NewNetworkManager.singleton).hostName = hostname;

		NewNetworkManager.singleton.StartHost();
		networkDiscovery.AdvertiseServer();
	}

	public void Connect(DiscoveryResponse info) {
		NewNetworkManager.singleton.StartClient(info.uri);
	}

	public void OnDiscoveredServer(DiscoveryResponse info) {
		Debug.Log("Discovered Server: " + info.hostName + "by" + info.hostUsername + "(IP: "+ info.EndPoint.Address.ToString());
		networkMenu.AddServer(info);
	}
}
 