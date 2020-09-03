using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SimpleFileBrowser;
using Assets.Scripts.CityGML2GO;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	public CityGml2GO CGML2GO; 
	public TextMeshProUGUI GMLSelected;
	public Button StartBtn;
	public Button LoadGMLBtn;

	private string gmlPath;

	private void Start() {
		StartBtn.GetComponentInChildren<TextMeshProUGUI>().SetText("City Not Loaded");
		if(CGML2GO == null) {
			CGML2GO = GameObject.FindGameObjectWithTag("CityGML").GetComponent<CityGml2GO>();
		}
	}

	public void Proceed() {
		StartCoroutine(LoadMainScene());
	}

	public void LoadGML() {
		//FileBrowser.ShowLoadDialog((path) => SelectGMLFolder(path),
		//	() => { Debug.Log("Cancelled"); },
		//		true,
		//		false,
		//		Application.persistentDataPath,
		//		"Select Folder that contains GML Files");

		CGML2GO.InstantiateCity();
		StartCoroutine(WaitForInstantiateFinish());

	}

	private void SelectGMLFolder(string[] path) {
		GMLSelected.SetText(path[0]);
		StartBtn.interactable = true;

		CGML2GO.SetFilePath(path[0]);
	}

	IEnumerator WaitForInstantiateFinish() {
		GMLSelected.SetText(CGML2GO.FilePath);
		StartBtn.GetComponentInChildren<TextMeshProUGUI>().SetText("Loading City...");

		while (!CGML2GO.HasInstantiatedCity) {
			yield return null;
		}

		StartBtn.GetComponentInChildren<TextMeshProUGUI>().SetText("Proceed");
		StartBtn.interactable = true;
		CityGMLManager.Instance.InitializeCity();
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
