using System.Collections;
using UnityEngine;
using XGI.Tools;

public class Info : MonoBehaviour
{
	GameObject activeObject;
	GameObject focusedPanel;

	GameObject focusedObject
	{
		get
		{
			return activeObject;
		}
		set
		{
			if (activeObject != value) // If focused panel is changed
			{
				if(focusedPanel != null) // If previous focus was not empty
				{
					GameObject hidePosition = ChildrenSearch.SearchAmongChildren(focusedPanel.transform.parent.gameObject, "Hide Position");
					Tweener hideTweener = focusedPanel.AddComponent<Tweener>();
					hideTweener.MoveObject(hidePosition.GetComponent<RectTransform>(), 0.25f);
					StartCoroutine(ScaleTransition(1, hideTweener));
					focusedPanel = null;
				}

				if (value != null) // If current focus is not empty
				{
					GameObject infoPanel = ChildrenSearch.SearchAmongLineage(value, "Info Panel");
					focusedPanel = infoPanel;
					
					GameObject showPosition = ChildrenSearch.SearchAmongLineage(value, "Show Position");
					Tweener showTweener = infoPanel.AddComponent<Tweener>();
					showTweener.MoveObject(showPosition.GetComponent<RectTransform>(), 0.25f);
					StartCoroutine(ScaleTransition(0, showTweener));
				}
			}

			activeObject = value;
		}
	}

	IEnumerator ScaleTransition(int subtractFac, Tweener tweener)
	{
		do
		{
			tweener.gameObject.transform.localScale = Mathf.Abs(subtractFac - tweener.progress / 100) * Vector3.one * 0.003f;
			yield return new WaitForEndOfFrame();
		} while (tweener.progress < 100);
	}

	private void OnEnable()
	{
		GameObject[] panels = GameObject.FindGameObjectsWithTag("Info Panel");

		// Hiding all the information panels
		foreach(GameObject panel in panels)
		{
			GameObject hidePosition = ChildrenSearch.SearchAmongChildren(panel.transform.parent.gameObject, "Hide Position");
			panel.transform.localScale = Vector3.zero;
			panel.transform.position = hidePosition.transform.position;
		}
	}

	private void Update()
	{
		RaycastHit hit;
		if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit)) focusedObject = hit.collider.gameObject;
		else focusedObject = null;
		if(focusedPanel != null) focusedPanel.transform.forward = Camera.main.transform.forward; // If a panel is active, align it to camera's direction
	}
}