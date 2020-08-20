using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore.Examples.ObjectManipulation;

public class CityTranslationManipulator : TranslationManipulator
{

    // Update is called once per frame
    protected override void Update()
    {
		base.Update();
		//if (CityGMLManager.Instance.b_IsCityPlaced) {
		//	Collider collider = PlayAreaManager.Instance.PlayArea.MeshBoundary.GetComponent<BoxCollider>();
		//	CityGMLManager.Instance
		//		.CheckWithinBounds(collider.bounds);
		//}
    }
}
