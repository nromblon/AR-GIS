using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.CityGML2GO.Scripts;

public class PlayAreaBoundsHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnTriggerExit(Collider other) {
		var building = other.GetComponentInParent<BuildingController>();
		if (building != null) {
			building.EnableRenderers(false);
		}
	}

	private void OnTriggerEnter(Collider other) {
		var building = other.GetComponentInParent<BuildingController>();
		if (building != null) {
			building.EnableRenderers(true);
		}
	}
}
