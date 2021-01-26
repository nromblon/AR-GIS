using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.UI.Extensions;
using TMPro;

public class IssueObject : MonoBehaviour
{
	[Header("Config")]
	[SerializeField] private float collapseWidth = 320f;
	[SerializeField] private float expandWidth = 700f, expandDuration = 1.3f;
	[SerializeField] private float imageMaxWidth = 1200f;
	[SerializeField] private float imageMaxHeight = 1040f;
	public Canvas Canvas;
	public Renderer[] renderers;
	[Header("Setup")]
	[SerializeField] private TextMeshProUGUI titleText;
	[SerializeField] private TextMeshProUGUI createDateText;
	[SerializeField] private TextMeshProUGUI updateDateText;
	[SerializeField] private TextMeshProUGUI descText;
	[SerializeField] private RawImage image;
	[SerializeField] private GameObject rightLayout;
	[SerializeField] private Button expandButton;
	[SerializeField] private Transform expandChevronTf;
	[SerializeField] private float chevronEulerExpand = 90f;
	[SerializeField] private float chevronEulerCollapse = 270f;
	public SegmentedControl segmentControl;
	
	private bool hasCanvasInitialized = false;
	private bool isCanvasShown = false;
	private bool isCanvasExpanded = false;
	public Issue issue {
		get;
		private set;
	}

	private void Awake() {
		if(renderers == null) {
			renderers = GetComponentsInChildren<Renderer>();
		}
	}

	public void InitializeObject(Issue issue) {
		this.issue = issue;

		titleText.text = issue.title;
		descText.text = issue.description;
		createDateText.text = "reported on " + issue.Created_At.Value.ToLocalTime().ToString("MMMM dd, yyyy");
		if (!issue.created_at.Equals(issue.updated_at))
			updateDateText.text = "last updated on " + issue.Updated_At.Value.ToLocalTime().ToString("MMMM dd, yyyy");

		StartCoroutine(DownloadAndSetImage(issue.image_path));

		var status = issue.status.statusType;
		segmentControl.selectedSegmentIndex = (int) status;
	}

	public bool ToggleCanvas() {
		if (!hasCanvasInitialized) {
			//Canvas.worldCamera = GameObject.FindGameObjectWithTag("UI Camera").GetComponent<Camera>();
			hasCanvasInitialized = true;
		}

		if (!isCanvasShown) {
			// Show the UI
			//CancelInvoke();
			Canvas.gameObject.SetActive(true);
			//CanvasAnimator.SetTrigger("Show");
			isCanvasShown = true;
		}
		else {
			DisableCanvas();
		}

		return isCanvasShown;
	}

	public void DisableCanvas() {
		if (!isCanvasShown)
			return;

		//CanvasAnimator.SetTrigger("Hide");

		// Collapse the canvas before disabling.
		if (isCanvasExpanded)
			LerpCanvasWidth(collapseWidth, false, _DisableCanvas);
		else
			_DisableCanvas();
	}

	public void EnableRenderers(bool val) {
		foreach(var r in renderers) {
			r.enabled = val;
		}
		if (!val) {
			DisableCanvas();
		}
	}

	private void _DisableCanvas() {
		Canvas.gameObject.SetActive(false);
		isCanvasShown = false;
	}

	public void ToggleExpandCanvas() {
		if (!isCanvasShown)
			return;

		expandButton.interactable = false;
		var newWidth = isCanvasExpanded ? collapseWidth : expandWidth;

		StartCoroutine(LerpCanvasWidth(newWidth, !isCanvasExpanded));
		isCanvasExpanded = !isCanvasExpanded;
	}

	IEnumerator DownloadAndSetImage(string mediaURL) {
		UnityWebRequest request = UnityWebRequestTexture.GetTexture(mediaURL);
		yield return request.SendWebRequest();
		//Canvas.gameObject.SetActive(true);
		if (request.isNetworkError || request.isHttpError) {
			Debug.LogError(request.error);
			yield break;
		}
		else
			image.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;

		// Resize the image
		image.SetNativeSize();
		var w = image.rectTransform.rect.width;
		var h = image.rectTransform.rect.height;

		var isWidthLarger = w > h;
		float ratio;
		if (isWidthLarger) {
			ratio = imageMaxWidth / w;
			image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, imageMaxWidth);
			image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h * ratio);
		}
		else {
			ratio = imageMaxHeight / h;
			image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, imageMaxHeight);
			image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w * ratio);
		}

		//Canvas.gameObject.SetActive(false);
	}

	IEnumerator LerpCanvasWidth(float width, bool isExpand, System.Action callback = null) {
		var rt = Canvas.GetComponent<RectTransform>();
		float origWidth = rt.rect.width;
		var rotateTo = isExpand ? chevronEulerCollapse : chevronEulerExpand;
		var origEuler = expandChevronTf.localEulerAngles.z;
		var targetRot = origEuler + 180;
		float time = 0, t;

		// Hide RightLayout if canvas is collapsing
		if (!isExpand)
			rightLayout.SetActive(false);

		while (time < expandDuration) {
			t = time / expandDuration;
			rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(origWidth, width, t));
			expandChevronTf.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(origEuler,targetRot,t));
			time += Time.deltaTime;
			yield return null;
		}

		expandChevronTf.localEulerAngles = new Vector3(0, 0, rotateTo);
		rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

		if (isExpand) {
			// Show RightLayout if canvas is expanding
			rightLayout.SetActive(true);
		}

		expandButton.interactable = true;

		callback?.Invoke();
	}

	public void StatusChanged() {
		var index = segmentControl.selectedSegmentIndex;
		var status = (IssueStatusType)index;

		issue.UpdateStatus(status);
		updateDateText.text = "last updated on " + issue.Updated_At.Value.ToLocalTime().ToString("MMMM dd, yyyy");
	}
}
