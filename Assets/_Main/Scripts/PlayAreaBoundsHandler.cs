using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.CityGML2GO.Scripts;

public class PlayAreaBoundsHandler : MonoBehaviour
{
	private void OnTriggerExit(Collider other) {
		var building = other.GetComponentInParent<BuildingController>();
		if (building != null) {
			building.EnableRenderers(false);
		}
		else {
			var issueObj = other.GetComponentInParent<IssueObject>();
			if (issueObj != null) {
				issueObj.EnableRenderers(false);
			}
		}
	}

	private void OnTriggerEnter(Collider other) {
		var building = other.GetComponentInParent<BuildingController>();
		if (building != null) {
			building.EnableRenderers(true);
		}
		else {
			var issueObj = other.GetComponentInParent<IssueObject>();
			if (issueObj != null) {
				issueObj.EnableRenderers(true);
			}
		}
	}
}
