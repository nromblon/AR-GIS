using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FixCityAR {
	public static class Utilities
	{
		/// <summary>
		/// Gets the bounds of a an object, including its children, using renderer component.
		/// Code snippet by Unity Forums user choobyman:
		/// https://forum.unity.com/threads/getting-the-bounds-of-the-group-of-objects.70979/
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static Bounds GetBounds(GameObject obj) {
			Bounds bounds = new Bounds();
			Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
			if (renderers.Length > 0) {
				//Find first enabled renderer to start encapsulate from it
				foreach (Renderer renderer in renderers) {
					if (renderer.enabled) {
						bounds = renderer.bounds;
						break;
					}
				}
				//Encapsulate for all renderers
				foreach (Renderer renderer in renderers) {
					if (renderer.enabled) {
						bounds.Encapsulate(renderer.bounds);
					}
				}
			}
			return bounds;
		}
	}
}
