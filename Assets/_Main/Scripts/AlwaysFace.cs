using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlwaysFace : MonoBehaviour
{

	[SerializeField] public Transform FaceTo;

	private void Start() {
		if (FaceTo == null)
			FaceTo = Camera.main.transform;
	}

	// Update is called once per frame
	void Update()
    {
		transform.LookAt(2 * transform.position - FaceTo.position);
    }
}
