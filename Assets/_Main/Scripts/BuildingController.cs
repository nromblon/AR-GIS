using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingController : MonoBehaviour
{
	private Renderer[] renderers;

    // Start is called before the first frame update
    void Start()
    {
		renderers = GetComponentsInChildren<Renderer>();    
    }

    public void EnableRenderers(bool val) {
		foreach (var r in renderers) {
			r.enabled = val;
		}
	}
}
