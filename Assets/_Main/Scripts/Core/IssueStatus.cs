using System;
using UnityEngine;

public enum IssueStatusType {
	UNACKNOWLEDGED = -1, ACKNOWLEDGED = 0, IN_PROGRESS = 1, FIXED = 2, IRRELEVANT = 3

} 

[Serializable]
public class IssueStatus
{
	public int id;
	public string name;
	public IssueStatusType statusType {
		get {
			if (name == "")
				return IssueStatusType.UNACKNOWLEDGED;
			switch (name) {
				case "acknowledged": return IssueStatusType.ACKNOWLEDGED;
				case "in-progress": return IssueStatusType.IN_PROGRESS;
				case "fixed": return IssueStatusType.FIXED;
				case "irrelevant": return IssueStatusType.IRRELEVANT;
				default: return IssueStatusType.UNACKNOWLEDGED;
			}
		}
		set {
			switch (value) {
				case IssueStatusType.UNACKNOWLEDGED: name = ""; break;
				case IssueStatusType.ACKNOWLEDGED: name = "acknowledged"; break;
				case IssueStatusType.IN_PROGRESS: name = "in-progress"; break;
				case IssueStatusType.FIXED: name = "fixed"; break;
				case IssueStatusType.IRRELEVANT: name = "irrelevant"; break;
			}
		}
	}
	public string color;
	public Color ColorVal {
		get {
			return StatusTypeToColor(statusType);
		}
	}
	public string created_at, updated_at;
	private DateTime? _created_at, _updated_at;
	public DateTime? Created_At {
		get {
			if (_created_at == null) {
				_created_at = Convert.ToDateTime(created_at);
			}
			return _created_at;
		}
		set {
			_created_at = value;
			created_at = value.ToString();
		}
	}

	public DateTime? Updated_At {
		get {
			if (_updated_at == null) {
				_updated_at = Convert.ToDateTime(updated_at);
			}
			return _updated_at;
		}
		set {
			_updated_at = value;
			updated_at = value.ToString();
		}
	}

	public string remarks;


	public void UpdateStatus(IssueStatusType newStatus) {
		statusType = newStatus;
		Updated_At = DateTime.UtcNow;
	}

	public void AddRemarks(string remarks) {
		this.remarks = remarks;
		Updated_At = DateTime.UtcNow;
	}

	public static Color32 StatusTypeToColor(IssueStatusType statusType) {
		switch (statusType) {
			case IssueStatusType.ACKNOWLEDGED: return new Color32(0xAA, 0x00, 0xFF, 0xFF);
			case IssueStatusType.IN_PROGRESS: return new Color32(0xFF, 0xDE, 0x03, 0xFF);
			case IssueStatusType.FIXED: return new Color32(0x76, 0xFF, 0x03, 0xFF);
			case IssueStatusType.IRRELEVANT: return new Color32(0xBD, 0xBD, 0xBD, 0xFF);
			default: return new Color32(0x46, 0x46, 0x46, 0xD6);
		}
	}

	public static Color StatusTypeToTextColor(IssueStatusType statusType) {
		switch (statusType) {
			case IssueStatusType.IN_PROGRESS:
			case IssueStatusType.FIXED:
			case IssueStatusType.IRRELEVANT: return Color.black;
			case IssueStatusType.ACKNOWLEDGED:
			default: return Color.white;
		}
	}
}
