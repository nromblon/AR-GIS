using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

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

		public static List<Vector3> GetBoundCenters(GameObject obj) {
			int childCount = obj.transform.childCount;
			List<Vector3> boundCenters = new List<Vector3>();

			for (int i = 0; i < childCount; i++) {
				var renderer = obj.transform.GetChild(i).GetComponent<Renderer>();
				boundCenters.Add(renderer.bounds.center);
			}

			return boundCenters;
		}

		public static Vector3 GetBoundCenter(GameObject obj) {
			Renderer renderer = obj.GetComponent<Renderer>();
			List<Vector3> boundCenters = new List<Vector3>();
		
			Vector3 center = renderer.bounds.center;

			return center;
		}

		public static double ComputeDistance(Coordinates c1, Coordinates c2) {
			return System.Math.Sqrt(System.Math.Pow(c1.x - c2.x,2) + System.Math.Pow(c1.y - c2.y,2));
		}

		public static double ComputeDistance(Vector3 v1, Vector3 v2) {
			return System.Math.Sqrt(System.Math.Pow(v1.x - v2.x, 2) + System.Math.Pow(v1.z - v2.z, 2));
		}

		public static Vector3 ConvertLatLongToUnits(Coordinates c) {
			double x = c.x * CityManager.Instance.UnitPerLatLongRatio;
			double z = c.y * CityManager.Instance.UnitPerLatLongRatio;
			return new Vector3((float)x, 0, (float)z);
		}

		/// <summary>
		/// Fits a box collider to its children's renderer components.
		/// Code snippet by Unity Answers user dbanfield:
		/// https://answers.unity.com/questions/22019/auto-sizing-primitive-collider-based-on-child-mesh.html
		/// </summary>
		/// <param name="parentObject"></param>
		public static void FitColliderToChildren(GameObject parentObject) {
			BoxCollider bc = parentObject.GetComponent<BoxCollider>();

			if (bc == null) {
				bc = parentObject.AddComponent<BoxCollider>();
			}
			
			bool hasBounds = false;
			Renderer[] renderers = parentObject.GetComponentsInChildren<Renderer>();
			Bounds bounds = new Bounds();
			foreach (Renderer render in renderers) {
				if (render.bounds.size == Vector3.zero)
					continue;

				if (hasBounds) {
					bounds.Encapsulate(render.bounds);
				}
				else {
					bounds = render.bounds;
					hasBounds = true;
				}
			}

			if (hasBounds) {
				bc.center = bounds.center - parentObject.transform.position;
				bc.size = bounds.size;
			}
			else {
				bc.center = Vector3.zero;
				bc.size = Vector3.zero;
			}
		}


		/// <summary>
		/// Checks if a point is within the boxcollider's bounds. This method is
		/// the Oriented Bounding Box (OBB)'s analogue to Bounds.Contain, which is
		/// Axis-Aligned Bounding Box (AABB).
		/// Code Snippet by Unity Answers' user MikeEnoch:
		/// https://answers.unity.com/questions/53989/test-to-see-if-a-vector3-point-is-within-a-boxcoll.html
		/// </summary>
		/// <param name="point"></param>
		/// <param name="box"></param>
		/// <returns></returns>
		public static bool PointInOABB(Vector3 point, BoxCollider box) {
			point = box.transform.InverseTransformPoint(point) - box.center;

			float halfX = (box.size.x * 0.5f);
			float halfY = (box.size.y * 0.5f);
			float halfZ = (box.size.z * 0.5f);
			if (point.x < halfX && point.x > -halfX &&
			   point.y < halfY && point.y > -halfY &&
			   point.z < halfZ && point.z > -halfZ)
				return true;
			else
				return false;
		}

		public static bool IsPointWithinBoxCollider(Vector3 point, BoxCollider box) {
			Vector3 offset = box.bounds.center - point;
			Ray inputRay = new Ray(point, offset.normalized);
			RaycastHit rHit;
			var is_inside = false;

			if (!box.Raycast(inputRay, out rHit, offset.magnitude * 1.1f)) {
				is_inside = true;
				Debug.DrawRay(point, offset, Color.blue, 2f);
			}
			else {
				Debug.DrawRay(point, offset, Color.red, 2f);
			}

			return is_inside;
		}

		public static void VisualizeBoxColliderBounds(BoxCollider box) {
			Gizmos.matrix = box.transform.localToWorldMatrix;
			Gizmos.color = Color.white;
			Gizmos.DrawCube(box.center, box.size);
		}

	}
}
