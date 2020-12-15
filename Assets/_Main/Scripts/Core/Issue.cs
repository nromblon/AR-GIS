using System;

[Serializable]
public class Issue {

	const string GCS_TYPE = "4326";

	public int id;
	public string title, description;
	public double latitude, longitude;
	public int upvotes, downvotes;
	public IssueStatus status;
	public string image_path, link;
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

	public Coordinates GetCoordinate() {
		return new Coordinates(longitude, latitude, 0, GCS_TYPE);
	}

	public void UpdateStatus(IssueStatusType status) {
		this.status.UpdateStatus(status);
		Updated_At = DateTime.UtcNow;
	}
}

[Serializable]
public class IssueData {
	public Issue[] data;
}