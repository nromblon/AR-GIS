using Mirror;

/// <summary>
/// Sent to the server by the client.
/// The client's username is included in this message.
/// </summary>
public struct ConnectRequest : NetworkMessage {
	public string username;
}

/// <summary>
/// Sent to the client by the server after request was processed
/// Here, the Cloud Anchor Id is sent to the client.
/// </summary>
public struct ConnectResponse : NetworkMessage {
	public string cloudAnchorId;
}