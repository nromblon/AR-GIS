using System;
using System.Net;
using Mirror;
using Mirror.Discovery;
using UnityEngine;
using UnityEngine.Events;

/*
	Discovery Guide: https://mirror-networking.com/docs/Guides/NetworkDiscovery.html
    Documentation: https://mirror-networking.com/docs/Components/NetworkDiscovery.html
    API Reference: https://mirror-networking.com/docs/api/Mirror.Discovery.NetworkDiscovery.html
*/

namespace FixCityAR {

	public class DiscoveryRequest : NetworkMessage {
		// Add properties for whatever information you want sent by clients
		// in their broadcast messages that servers will consume.
	}

	public class DiscoveryResponse : NetworkMessage {
		// Add properties for whatever information you want the server to return to
		// clients for them to display or consume for establishing a connection.
		public IPEndPoint EndPoint { get; set; }

		public Uri uri;

		public long serverId;

		public string anchorId;

		public string serverName;
		public string hostUsername;

		public int numUsers;
		public int maxUsers;

		public string[] loadedFiles;
	}

	[Serializable]
	public class ServerFoundUnityEvent : UnityEvent<DiscoveryResponse> { };

	public class NewNetworkDiscovery : NetworkDiscoveryBase<DiscoveryRequest, DiscoveryResponse> {

		private NewNetworkManager netManager;

		#region Server
		public long ServerId { get; private set; }

		[Tooltip("Transport to be advertised during discovery")]
		public Transport transport;

		[Tooltip("Invoked when a server is found")]
		public ServerFoundUnityEvent OnServerFound;

		public override void Start() {
			ServerId = RandomLong();

			// active transport gets initialized in awake
			// so make sure we set it here in Start()  (after awakes)
			// Or just let the user assign it in the inspector
			if (transport == null)
				transport = Transport.activeTransport;

			base.Start();
			netManager = (NewNetworkManager)NewNetworkManager.singleton;
		}

		/// <summary>
		/// Reply to the client to inform it of this server
		/// </summary>
		/// <remarks> 
		/// Override if you wish to ignore server requests based on
		/// custom criteria such as language, full server game mode or difficulty
		/// </remarks>
		/// <param name="request">Request comming from client</param>
		/// <param name="endpoint">Address of the client that sent the request</param>
		protected override void ProcessClientRequest(DiscoveryRequest request, IPEndPoint endpoint) {
			Debug.Log($"[NewNetworkDiscovery] Request received from client {endpoint.Address}");
			base.ProcessClientRequest(request, endpoint);
		}



		/// <summary>
		/// Process the request from a client
		/// </summary>
		/// <remarks>
		/// Override if you wish to provide more information to the clients
		/// such as the name of the host player
		/// </remarks>
		/// <param name="request">Request comming from client</param>
		/// <param name="endpoint">Address of the client that sent the request</param>
		/// <returns>A message containing information about this server</returns>
		protected override DiscoveryResponse ProcessRequest(DiscoveryRequest request, IPEndPoint endpoint) {
			try {
				return new DiscoveryResponse() {
					serverId = ServerId,
					uri = transport.ServerUri(),
					serverName = netManager.roomName,
					hostUsername = PlayerPrefs.GetString("username"),
					loadedFiles = netManager.loadedFiles.ToArray(),
					numUsers = NetworkServer.connections.Count,
					maxUsers = netManager.maxConnections
					
					// TODO: add numUsers, maxUsers, loadedFiles
					// But first learn whether if placing those properties in network manager is
					// the right way to do it.
					// Read on Network Authority maybe?
				};
			}
			catch (Exception e) {
				Debug.LogError(e.Message);
				throw;
			}
		}

		#endregion

		#region Client

		/// <summary>
		/// Create a message that will be broadcasted on the network to discover servers
		/// </summary>
		/// <remarks>
		/// Override if you wish to include additional data in the discovery message
		/// such as desired game mode, language, difficulty, etc... </remarks>
		/// <returns>An instance of ServerRequest with data to be broadcasted</returns>
		protected override DiscoveryRequest GetRequest() {
			return new DiscoveryRequest();
		}

		/// <summary>
		/// Process the answer from a server
		/// </summary>
		/// <remarks>
		/// A client receives a reply from a server, this method processes the
		/// reply and raises an event
		/// </remarks>
		/// <param name="response">Response that came from the server</param>
		/// <param name="endpoint">Address of the server that replied</param>
		protected override void ProcessResponse(DiscoveryResponse response, IPEndPoint endpoint) {
			response.EndPoint = endpoint;

			// although we got a supposedly valid url, we may not be able to resolve
			// the provided host
			// However we know the real ip address of the server because we just
			// received a packet from it,  so use that as host.
			UriBuilder realUri = new UriBuilder(response.uri) {
				Host = response.EndPoint.Address.ToString()
			};
			response.uri = realUri.Uri;

			OnServerFound.Invoke(response);
		}

		#endregion
	}
}
