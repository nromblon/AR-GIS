using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

public class DebugOverlay : MonoBehaviour
{
	public static DebugOverlay Instance;
	public Text fpsCounter;
	public Text[] frameCounts;
	public Text[] elapsedCounter;
	public Text[] averageFPS;
	public Text triCountTxt;
	public Text cityLoadedIdleAvgFps;

	private int[] frameCountInts;
	private long[] elapsedMs;
	private Stopwatch[] swatches;

	public float MainMenuTime;
	public float CityInitEndTime;
	public float ARSceneTime;
	public float CityPlacedTime;
	public float CityRemovedTime;
	public int ARSceneFrame;

	// Vars for writing in CSV (PerformanceTesting.WriteDataToCSV)
	private string main_menu_baseline;
	private string city_init;
	private string city_loaded_idle;
	private string AR_baseline;
	private string AR_city_placed;
	private string lat_meshgen;
	private string lat_cityInit;

	// Start is called before the first frame update
	void Awake()
    {
		Instance = this;

		frameCountInts = new int[frameCounts.Length];
		swatches = new Stopwatch[frameCounts.Length / 2];
		elapsedMs = new long[frameCounts.Length / 2];

    }

	private void Start() {
		if (!PerformanceTesting.IsEvaluating)
			Destroy(gameObject);
	}


	private void Update() {
		fpsCounter.text = ((int)(1 / Time.deltaTime)).ToString();
	}

	public void SaveFrameCount(FrameCounts saveFor) {
		var origText = frameCounts[(int)saveFor].text;
		frameCounts[(int)saveFor].text = origText + Time.frameCount.ToString();
		frameCountInts[(int)saveFor] = Time.frameCount;
		
	}

	public void SetStopwatch(FrameCounts saveFor) {
		int swIdx = Mathf.FloorToInt((int)saveFor / 2);
		if ((int)saveFor % 2 == 0) {
			swatches[swIdx] = new Stopwatch();
			swatches[swIdx].Start();
		}
		else {
			swatches[swIdx].Stop();
			var elapsedMs = swatches[swIdx].ElapsedMilliseconds;
			elapsedCounter[swIdx].text = elapsedCounter[swIdx].text + elapsedMs.ToString();

			this.elapsedMs[swIdx] = elapsedMs;

			if (swIdx == 0) {
				lat_meshgen = elapsedMs.ToString();
			}
			else if (swIdx == 1) {
				lat_cityInit = elapsedMs.ToString();
			}
		}
	}

	public void SetCityLoadedIdleFps() {
		var frameCountRange = Time.frameCount - frameCountInts[3];
		var elapsedTime = Time.time - CityInitEndTime;
		cityLoadedIdleAvgFps.text = cityLoadedIdleAvgFps.text + (frameCountRange / elapsedTime).ToString();

		city_loaded_idle = (frameCountRange / elapsedTime).ToString();
	}

	public void SetAverageFPS(AvgFPS setFor) {
		string text = "";
		switch (setFor) {
			case AvgFPS.MainMenu:
				UnityEngine.Debug.Log("MainMenu Time: " + MainMenuTime);
				UnityEngine.Debug.Log("FrameCountInts[0]: " + frameCountInts[0]);
				text = (frameCountInts[0] / (Time.time - MainMenuTime)).ToString();

				main_menu_baseline = text;
				break;

			case AvgFPS.CityInit:
				// Add both meshGen and cityInit elapsed in seconds
				long totalElapsed = (elapsedMs[0] + elapsedMs[1]) / 1000;
				UnityEngine.Debug.Log("totalElapsed: " + totalElapsed);
				// Calculate number of frames between meshGenStart and cityInitEnd
				var frameCountRange = frameCountInts[3] - frameCountInts[0];
				text = (frameCountRange / totalElapsed).ToString();

				city_init = text;
				break;

			case AvgFPS.ARScene:
				frameCountRange = frameCountInts[4] - ARSceneFrame;
				var elapsed = Time.time - ARSceneTime;
				text = (frameCountRange / elapsed).ToString();

				AR_baseline = text;
				break;

			case AvgFPS.CityPlaced:
				frameCountRange = frameCountInts[5] - frameCountInts[4];
				elapsed = CityRemovedTime - CityPlacedTime;
				UnityEngine.Debug.Log("elapsed: " + elapsed);
				text = (frameCountRange / elapsed).ToString();
				UnityEngine.Debug.Log("City Placed avg FPS: "+ text);

				AR_city_placed = text;
				//write results to csv
				//UnityEngine.Debug.Log("[PerformanceTesting] WriteDataToCSV called ");
				PerformanceTesting.WriteDataToCSV(main_menu_baseline, city_init, city_loaded_idle, AR_baseline, AR_city_placed, lat_meshgen, lat_cityInit);
				break;
		}

		averageFPS[(int)setFor].text = averageFPS[(int)setFor].text + text;
	}

	public void SetTriCount(int triCount) {
		triCountTxt.text = triCountTxt.text + (triCount).ToString();
	}
}

public enum FrameCounts {
	meshGenStart = 0, meshGenEnd = 1,
	cityInitStart = 2, cityInitEnd = 3,
	cityPlaced = 4, cityRemoved = 5
}

public enum AvgFPS {
	MainMenu = 0, CityInit = 1, ARScene = 2, CityPlaced = 3
}