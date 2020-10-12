using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.CityGML2GO;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	public CityGml2GO CGML2GO; 
	public TextMeshProUGUI GMLSelected;
	public Button StartBtn;
	public Button LoadGMLBtn;

	// Performance Testing
	public TMP_InputField EvalNumInputField;

	private string gmlPath;

	private void Start() {
		StartBtn.GetComponentInChildren<TextMeshProUGUI>().SetText("City Not Loaded");
		if(CGML2GO == null) {
			CGML2GO = GameObject.FindGameObjectWithTag("CityGML").GetComponent<CityGml2GO>();
		}

		if (PerformanceTesting.IsEvaluating)
			DebugOverlay.Instance.MainMenuTime = Time.time;
	}

	public void Proceed() {
		if (PerformanceTesting.IsEvaluating) {
			DebugOverlay.Instance.SetCityLoadedIdleFps();
		}
		StartCoroutine(LoadMainScene());
	}

	public void LoadGML() {
		//FileBrowser.ShowLoadDialog((path) => SelectGMLFolder(path),
		//	() => { Debug.Log("Cancelled"); },
		//		true,
		//		false,
		//		Application.persistentDataPath,
		//		"Select Folder that contains GML Files");

		PerformanceTesting.EvalSet = System.Int32.Parse(EvalNumInputField.text);

		string gmlText;

		if (PerformanceTesting.IsEvaluating) {
			gmlText = "Eval Set: " + PerformanceTesting.EvalSet;
		}
		else {
			gmlText = "GML path: " + CGML2GO.FilePath;
		}

		GMLSelected.SetText(gmlText);
		
		CGML2GO.InstantiateCity();
		StartCoroutine(WaitForInstantiateFinish());

	}

	private void SelectGMLFolder(string[] path) {
		GMLSelected.SetText(path[0]);
		StartBtn.interactable = true;

		CGML2GO.SetFilePath(path[0]);
	}

	IEnumerator WaitForInstantiateFinish() {
		TextMeshProUGUI tm = StartBtn.GetComponentInChildren<TextMeshProUGUI>();
		tm.SetText("Initializing City");

		int dotCount = 0;
		string dots = "";
		float timeElapsed = 0;
		float timeDotInterval = .6f;
		while (!CGML2GO.HasInstantiatedCity) {
			if (timeElapsed >= timeDotInterval) {
				timeElapsed = 0;
				tm.SetText("Initializing City" + dots);
				if (dotCount > 2) {
					dotCount = 0;
					dots = "";
				}
				else {
					dots = dots + ".";
					dotCount = dotCount + 1;
				}
			}

			yield return null;
			timeElapsed += Time.deltaTime;
		}

		StartBtn.GetComponentInChildren<TextMeshProUGUI>().SetText("Proceed");
		StartBtn.interactable = true;
		CityManager.Instance.InitializeCity();

	}

	IEnumerator LoadMainScene() {
		Scene currentScene = SceneManager.GetActiveScene();

		//AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("ARScene", LoadSceneMode.Single);

		//while (!asyncLoad.isDone) {
		//	yield return null;
		//}

		CGML2GO.gameObject.SetActive(false);
		//SceneManager.MoveGameObjectToScene(CGML2GO.gameObject, SceneManager.GetSceneByName("ARScene"));
		SceneManager.LoadScene("ARScene");
		yield return null;
		//SceneManager.UnloadSceneAsync(currentScene);
	}
}
