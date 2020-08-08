using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.CityGML2GO;

public class CityGMLManager : MonoBehaviour
{
	private static CityGMLManager sharedInstance;
	public static CityGMLManager Instance {
		get {
			return sharedInstance;
		}
	}

	public CityGml2GO cityGML2GO;

	private void Awake() {
		sharedInstance = this;
	}

	// Start is called before the first frame update
	void Start()
    {
		this.cityGML2GO = GetComponent<CityGml2GO>();
    }
}
