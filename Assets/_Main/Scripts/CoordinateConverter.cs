using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class CoordinateConverter {
	const string CONV_URL = "http://epsg.io/trans?";

	public Coordinates result;
	public Coordinates[] results;
	public bool isDone = false;

	public IEnumerator ConvertCoordinate(Coordinates toConvert, string targetEPSGCode) {
		isDone = false;
		string requestURL = CONV_URL + "x=" + toConvert.x + "&y=" + toConvert.y + "&z=" + toConvert.z +
			"&s_srs=" + toConvert.GetGCSType() + "&t_srs=" + targetEPSGCode;
		
		using (UnityWebRequest webReq = UnityWebRequest.Get(requestURL)) {
			yield return webReq.SendWebRequest();

			if (webReq.isNetworkError) {
				Debug.LogError(webReq.error);
			}
			else {
				string json = webReq.downloadHandler.text;
				result = JsonUtility.FromJson<Coordinates>(json);
				result.gcs_type = targetEPSGCode;
			}
		}
		isDone = true;
	}

	public IEnumerator ConvertCoordinate(Coordinates[] toConvert, string targetEPSGCode) {
		isDone = false;
		string requestURL = CONV_URL+"data=";

		for (int i = 0; i < toConvert.Length; i++) {
			Coordinates c = toConvert[i];
			string point = c.x + "," + c.y + "," + c.z;
			if (i != toConvert.Length - 1)
				point = point + ";";
			requestURL = requestURL + point;
		}

		requestURL = requestURL + "&s_srs=" + toConvert[0].GetGCSType() + "&t_srs=" + targetEPSGCode;
		using (UnityWebRequest webReq = UnityWebRequest.Get(requestURL)) {
			yield return webReq.SendWebRequest();

			if (webReq.isNetworkError) {
				Debug.LogError(webReq.error);
			}
			else {
				string json = webReq.downloadHandler.text;
				json = "{\"data\":" + json.Trim() + "}";
				try {
					ConvertedCoordinateData ccd = JsonUtility.FromJson<ConvertedCoordinateData>(json);

					results = ccd.data;
					foreach (var r in results) {
						r.gcs_type = targetEPSGCode;
					}
				} catch(System.Exception e) {
					Debug.LogError(e.StackTrace);
					Debug.Log("Request URL: " + requestURL);
					Debug.LogError("Received JSON: " + json);
				}
			}
		}

		isDone = true;
	}
}

[System.Serializable]
public class ConvertedCoordinateData {
	public Coordinates[] data;
}

[System.Serializable]
public class Coordinates {
	/// <summary>
	/// longitude
	/// </summary>
	public double x;
	/// <summary>
	/// latitude
	/// </summary>
	public double y;
	/// <summary>
	/// elevation
	/// </summary>
	public double z;
	public string gcs_type;

	public Coordinates(double x, double y, double elevation, string gcs_type) {
		this.x = x;
		this.y = y;
		this.z = elevation;
		this.gcs_type = gcs_type;
	}

	public override System.String ToString() {
		return gcs_type + ": " + x + "," + y + "," + z;
	}

	public System.String GetGCSType() {
		if (gcs_type.Contains("EPSG")) {
			var lastColonIdx = gcs_type.LastIndexOf(':');
			return gcs_type.Substring(lastColonIdx + 1);
		}
		else {
			return gcs_type;
		}
	}
}