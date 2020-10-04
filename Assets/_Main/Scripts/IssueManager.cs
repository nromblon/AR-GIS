using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class IssueManager : MonoBehaviour
{
	private static IssueManager sharedInstance;
	public static IssueManager Instance {
		get { return sharedInstance; }
	}

	const string ISSUE_URI = "fixcity.app/api/issues";
	[Header("Setup")]
	[SerializeField] private GameObject issuePrefab;

	[Header("Debug")]
	public List<Issue> issues;
	public List<IssueObject> issueObjects;

	private void Awake() {
		sharedInstance = this;
	}

	// Start is called before the first frame update
	void Start()
    {
		StartCoroutine(RetrieveIssues());
    }

	public void InitializeIssues() {
		// Remove Issues that are not within City Bounds.
		var numRemoved = issues.RemoveAll((Issue obj) => 
		!IsInsideCoordBounds(obj.GetCoordinate(),CityProperties.wgs_MinPoint, CityProperties.raw_MaxPoint));
		
		// Compute coord_distance to be used in Conversion Ratio for LatLong to Unity Units
		//GPSEncoder.SetLocalOrigin(new Vector2((float)CityProperties.wgs_Center.y, (float)CityProperties.wgs_Center.x));

		issueObjects = new List<IssueObject>();

		// Instantiate all Issues into Issue Objects.
		foreach (var issue in issues) {
			// position the issue in Unity world
			GameObject obj = Instantiate(issuePrefab);
			obj.name = issue.title;
			var pos = GPSEncoder.GPSToUCS(new Vector2((float)issue.GetCoordinate().y, (float)issue.GetCoordinate().x));
			obj.transform.position = pos;
			obj.transform.SetParent(transform);

			// Initialize Issue Object Properties
			var iobj = obj.GetComponent<IssueObject>();
			iobj.InitializeObject(issue);

			//add to list
			issueObjects.Add(iobj);
		}
	}

	IEnumerator RetrieveIssues() {
		using (UnityWebRequest webRequest = UnityWebRequest.Get(ISSUE_URI)) {
			yield return webRequest.SendWebRequest();

			if (webRequest.isNetworkError) {
			}
			else {
				string json = webRequest.downloadHandler.text;
				
				issues = new List<Issue>(ParseJsonArray(json));

				foreach (var i in issues) {
					i.GetCoordinate().gcs_type = "4326";
				}
			}
		}
	}

	private Issue[] ParseJsonArray(string data) {
		IssueData issues;

		issues = JsonUtility.FromJson<IssueData>(data);
		

		return issues.data;
	}

	private bool IsInsideCoordBounds(Coordinates toCheck, Coordinates boundMin, Coordinates boundMax) {
		return toCheck.x >= boundMin.x && toCheck.y >= boundMin.y &&
			toCheck.x <= boundMax.x && toCheck.y <= boundMax.y;
	}
}
