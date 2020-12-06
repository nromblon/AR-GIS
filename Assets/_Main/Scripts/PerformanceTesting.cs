using System.Collections.Generic;
using Unity.Profiling;

public class PerformanceTesting
{
	public const bool IsEvaluating = false;

	public static int EvalSet = -1;
	static Dictionary<string, ProfilerMarker> profilerMarkers;
	public static ProfilerMarker s_MeshGen = new ProfilerMarker("ARGIS.MeshGen");

	/// <summary>
	/// Create a Profiler Marker and call Begin()
	/// </summary>
	/// <param name="name"></param>
	public static void MarkerBegin(string name) {
		if (profilerMarkers == null)
			profilerMarkers = new Dictionary<string, ProfilerMarker>();


		if (!profilerMarkers.ContainsKey(name)) {
			ProfilerMarker p = new ProfilerMarker(name);
			p.Begin();
			profilerMarkers.Add(name, p);
		}
	}

	/// <summary>
	/// End() a Profiler Marker and delete it from the dictionary.
	/// </summary>
	/// <param name="name"></param>
	public static void MarkerEnd(string name) {
		if (profilerMarkers == null)
			return;

		if (profilerMarkers.ContainsKey(name)) {
			profilerMarkers[name].End();
			profilerMarkers.Remove(name);
		}
	}

	/// <summary>
	/// creates a ProfilerMarker instance
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public static ProfilerMarker Marker(string name) {
		return new ProfilerMarker(name);
	}

	/// <summary>
	/// Checks if the file is a part of the current evaluation set
	/// </summary>
	/// <param name="filename">the file to check</param>
	/// <param name="evalset">Evaluation Set number</param>
	/// <returns></returns>
	public static bool IsInEvalSet(string filename, int evalset) {
		bool inSet = false;

		foreach(int gmlId in TestingConstants.EvalCombinations[evalset]) {
			if (filename.EndsWith(TestingConstants.Filenames[gmlId])) {
				inSet = true;
				break;
			}
		}

		return inSet;
	}

	/// <summary>
	/// Checks if the file is a part of the current evaluation set
	/// </summary>
	/// <param name="filename">the file to check</param>
	/// <param name="evalset">Evaluation Set number</param>
	/// <returns></returns>
	public static bool IsInEvalSet(string filename) {
		bool inSet = false;

		foreach (int gmlId in TestingConstants.EvalCombinations[PerformanceTesting.EvalSet]) {
			var testTo = TestingConstants.Filenames[gmlId];
			if (filename.EndsWith(testTo)) {
				inSet = true;
				break;
			}
			else {
				//also check for forward slash
				testTo = testTo.Replace("\\", "/");
				if (filename.EndsWith(testTo)) {
					inSet = true;
					break;
				}
			}
		}

		return inSet;
	}

	public static string[] GetFilenamesFromEvalSet(int evalSet) {
		int[] setIds = TestingConstants.EvalCombinations[evalSet];
		string[] filenames = new string[setIds.Length];
		for (int i = 0; i < setIds.Length; i++) {
			filenames[i] = TestingConstants.Filenames[setIds[i]];
		}

		return filenames;
	}
}

class TestingConstants {
	// See sheets for more details
	public static int[][] EvalCombinations = new int[][]
	{
		new int[] {5, 6},
		new int[] {5, 6, 7},
		new int[] {5, 6, 7, 8},
		new int[] {0, 1, 2},
		new int[] {5, 6, 7, 8, 9},
		new int[] {5, 6, 7, 8, 9, 10},
		new int[] {5, 6, 7, 8, 9, 10, 0, 1, 2},
		new int[] {0, 1, 2, 3, 4},
		new int[] {5, 6, 7, 8, 9, 10, 11, 0, 1, 2, 3},
		new int[] {5, 6, 7, 8, 9, 10, 11, 12},
		new int[] {5, 6, 7, 8, 9, 10, 0, 1}
	};

	// 1st row: Id# 7, 8, 9, 10, 11
	// 2nd row: Id# 13, 14, 15, 16, 17, 18, 19, 20
	public static string[] Filenames = {
		"Berlin1b\\3920_5820.gml", "Berlin1b\\3900_5820.gml", "Berlin1b\\3910_5820.gml", "Berlin1b\\3920_5819.gml", "Berlin1b\\3900_5819.gml",
		"Berlin2\\3900_5819.gml", "Berlin2\\3910_5819.gml", "Berlin2\\3900_5817.gml", "Berlin2\\3920_5818.gml", "Berlin2\\3910_5817.gml", "Berlin2\\3920_5819.gml", "Berlin2\\3900_5818.gml", "Berlin2\\3910_5818.gml"
	};

	public static string MESH_GENERATE = "ARGIS.MeshGeneration";
	public static string MAIN_MENU = "ARGIS.MainMenu";
	public static string CITY_INIT = "ARGIS.CityInitialization";
	public static string CITY_PLACED = "ARGIS.CityPlaced";
}
