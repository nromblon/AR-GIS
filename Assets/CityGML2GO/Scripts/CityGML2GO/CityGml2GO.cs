using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Assets.Scripts.CityGML2GO.GmlHandlers;
using Framework.Variables;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Material = UnityEngine.Material;

namespace Assets.Scripts.CityGML2GO {
	
	/// <summary>
	/// CityGML Generator. Generates gameobjects based from a GML file.
	/// </summary>
	public partial class CityGml2GO : MonoBehaviour {
		[LabelOverride("File-/Directory Name")]

		public string Filename;
		public bool StreamingAssets;
		public string FilePath;

		public Material DefaultMaterial;
		public GameObject Parent;
		[LabelOverride("Apply automatic or manual translation")]
		public bool ApplyTranslation;
		[HideInInspector]
		public Vector3 ActualTranslate;
		[LabelOverride("Manual translation (Set to 0,0,0 for automatic)")]
		public Vector3 Translate = Vector3.zero;
		public bool ShowDebug;
		public float UpdateRate;
		public bool ShowCurves;
		public bool Semantics;
		public float CurveThickness;
		public GameObject LineRendererPrefab;
		public bool GenerateColliders;


		public bool ApplyTextures = false;
		public List<string> SemanticSurfaces = new List<string> { "GroundSurface", "WallSurface", "RoofSurface", "ClosureSurface", "CeilingSurface", "InteriorWallSurface", "FloorSurface", "OuterCeilingSurface", "OuterFloorSurface", "Door", "Window" };
		public List<Poly2Mesh.Polygon> oriPoly = new List<Poly2Mesh.Polygon>();
		public List<GameObject> Polygons = new List<GameObject>();
		public Dictionary<string, List<string>> Materials = new Dictionary<string, List<string>>();
		public List<TextureInformation> Textures = new List<TextureInformation>();
		[HideInInspector]
		public SemanticSurfaceMaterial SemanticSurfMat;

		private bool hasInstantiatedCity = false;
		public bool HasInstantiatedCity {
			get { return hasInstantiatedCity; }
			private set {
				hasInstantiatedCity = value;
			}
		}

		void Start() {
			SemanticSurfMat = GetComponent<SemanticSurfaceMaterial>();
			
		}

		/// <summary>
		/// Instantiates the city. 
		/// Changed by Neil Romblon, August 2020. Moved from Update() 
		/// and added a couple lines to allow persistentDataPath as file location.
		/// </summary>
		public void InstantiateCity() {
			if (hasInstantiatedCity) {
				RefreshMeshes();
				return;
			}

			var fn = "";

			if (Application.platform == RuntimePlatform.Android) {
				StreamingAssets = false;
			}

			if (StreamingAssets) {
				fn = Path.Combine(Application.streamingAssetsPath, Filename);
			}
			else {
				// Change this to browse files
				fn = Path.Combine(Application.persistentDataPath, Filename);
			}

			FilePath = fn;

			Polygons = new List<GameObject>();

			FileAttributes attributes;

			attributes = File.GetAttributes(fn);

			// Checks if File is a Single file or a directory.
			if ((attributes & FileAttributes.Directory) == FileAttributes.Directory) {
				SetTranslate(new DirectoryInfo(fn));
				StartCoroutine("RunDirectory", fn);
			}
			else {
				SetTranslate(new FileInfo(fn));
				StartCoroutine(Run(fn,true));
			}
			
		}

		/// <summary>
		/// As the values of GML are way outside of unitys range, you should apply a global translate vector to it.
		/// SetTranslate tries to calculate that vector.
		/// </summary>
		/// <param name="file"></param>
		void SetTranslate(FileInfo file) {
			if (!ApplyTranslation) {
				ActualTranslate = Vector3.zero;
				return;
			}

			ActualTranslate = Translate == Vector3.zero ? TranslateVector.GetTranslateVectorFromFile(file) : Translate;
		}

		/// <summary>
		/// As the values of GML are way outside of unitys range, you should apply a global translate vector to it.
		/// SetTranslate tries to calculate that vector.
		/// </summary>
		/// <param name="directory"></param>
		void SetTranslate(DirectoryInfo directory) {
			if (!ApplyTranslation) {
				ActualTranslate = Vector3.zero;
				return;
			}

			if (Translate != Vector3.zero) {
				ActualTranslate = Translate;
				return;
			}

			Vector3 translate = Vector3.zero;
			var count = 0;
			foreach (var fileInfo in directory.GetFiles("*.gml", SearchOption.AllDirectories)) {
				if (PerformanceTesting.IsEvaluating) {
					if (!PerformanceTesting.IsInEvalSet(fileInfo.FullName)) {
						continue;
					}
				}

				//Debug.Log("SetTranslate: " + fileInfo.FullName);
				count++;
				translate += TranslateVector.GetTranslateVectorFromFile(fileInfo);
			}
			
			ActualTranslate = translate / count;
		}

		public void RefreshMeshes() {
			foreach (var mf in GetComponentsInChildren<MeshFilter>()) {
				var childMesh = mf.mesh;
				childMesh.RecalculateNormals();
				childMesh.RecalculateBounds();
				childMesh.RecalculateTangents();
			}
		}

		/// <summary>
		/// Proccesses all GML files ina directory
		/// </summary>
		/// <param name="directoryName"></param>
		/// <returns></returns>
		IEnumerator RunDirectory(string directoryName) {
			if (PerformanceTesting.IsEvaluating) {
				DebugOverlay.Instance.SetStopwatch(FrameCounts.meshGenStart);
				DebugOverlay.Instance.SaveFrameCount(FrameCounts.meshGenStart);
				DebugOverlay.Instance.SetAverageFPS(AvgFPS.MainMenu);
			}
			//Debug.Log("Run Directory");
			foreach (var gml in Directory.GetFiles(directoryName, "*.gml", SearchOption.AllDirectories)) {
				if (PerformanceTesting.IsEvaluating) {
					//Debug.Log("Checking gml: " + gml);
					if (!PerformanceTesting.IsInEvalSet(gml)) {
						continue;
					}
					//Debug.Log("GML passed: " + gml);
				}

				string gml_r = gml.Replace("\\", "/");
				//Debug.Log("Running: "+ gml_r);
				Polygons = new List<GameObject>();
				Materials = new Dictionary<string, List<string>>();
				Textures = new List<TextureInformation>();
				yield return Run(gml_r);
			}

			//// Do A recursive search for all subdirectories
			//foreach(var dir in Directory.GetDirectories(directoryName)) {
			//	Debug.Log("Dir: " + dir);
			//	RunDirectory(dir);
			//}

			// Refresh Meshes
			RefreshMeshes();

			HasInstantiatedCity = true;

			if (PerformanceTesting.IsEvaluating) {
				DebugOverlay.Instance.SaveFrameCount(FrameCounts.meshGenEnd);
				DebugOverlay.Instance.SetStopwatch(FrameCounts.meshGenEnd);
			}
		}

		/// <summary>
		/// Processes a single file
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		IEnumerator Run(string fileName, bool isSingle=false) {
			var counter = 0;
			var sw = new Stopwatch();
			sw.Start();

			var lastFrame = sw.ElapsedMilliseconds;
			using (XmlReader reader = XmlReader.Create(fileName, new XmlReaderSettings { IgnoreWhitespace = true })) {
				yield return null;

				while (!reader.EOF) {
					reader.Read();
					if (reader.LocalName == "CityModel") {
						break;
					}
				}

				var version = 0;
				for (int i = 0; i < reader.AttributeCount; i++) {
					var attr = reader.GetAttribute(i);
					if (attr == "http://www.opengis.net/citygml/1.0") {
						version = 1;
						break;
					}
					if (attr == "http://www.opengis.net/citygml/2.0") {
						version = 2;
						break;
					}
				}

				if (version == 0) {
					Debug.LogWarning("Possibly invalid xml. Check for xml:ns citygml version.");
				}

				while (reader.Read()) {
					if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "cityObjectMember") {
						while (reader.Read()) {
							if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "Building") {
								counter++;

								if (UpdateRate > 0 && sw.ElapsedMilliseconds > lastFrame + UpdateRate) {
									lastFrame = sw.ElapsedMilliseconds;
									yield return null;
								}
								BuildingHandler.HandleBuilding(reader, this);
							}
						}
					}
				}
			}
			//CombineMeshes();
			if(ApplyTextures)
				MaterialHandler.ApplyMaterials(this);

			if (isSingle)
				HasInstantiatedCity = true;

			yield return null;
		}

		public void SetFilePath(string path) {
			this.Filename = path;
		}
	}
}
