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
			return;
		}

		var issueObj = other.GetComponentInParent<IssueObject>();
		if (issueObj != null) {
			issueObj.EnableRenderers(false);
			return;
		}

		if (other.tag == "InfoPin") {
			other.GetComponent<InfoPin>().gameObject.SetActive(false);
			return;
		}

		if (other.tag == "Ping") {
			other.GetComponent<InfoPin>().gameObject.SetActive(false);
			return;
		}
	}

	private void OnTriggerEnter(Collider other) {
		var building = other.GetComponentInParent<BuildingController>();
		if (building != null) {
			building.EnableRenderers(true);
			return;
		}

		var issueObj = other.GetComponentInParent<IssueObject>();
		if (issueObj != null) {
			issueObj.EnableRenderers(true);
			return;
		}

		if (other.tag == "InfoPin") {
			other.GetComponent<InfoPin>().gameObject.SetActive(true);
			return;
		}

		if (other.tag == "Ping") {
			other.GetComponent<InfoPin>().gameObject.SetActive(true);
			return;
		}
	}
}
