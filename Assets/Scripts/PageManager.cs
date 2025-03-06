using Unity.VisualScripting;
using UnityEngine;
using XGI.Tools;

public class PageManager : MonoBehaviour
{
	[SerializeField] RectTransform showTransform;
	[SerializeField] RectTransform hideTransform;
	[SerializeField] RectTransform[] pages;

	private void OnEnable()
	{
		foreach(RectTransform page in pages)
		{
			page.gameObject.SetActive(true);
			page.position = hideTransform.position;
		}	
	}

	public void OpenPage(int pageNumber)
	{
		Tweener tweener;
		foreach(RectTransform page in pages)
		{
			tweener = page.AddComponent<Tweener>();
			tweener.MoveObject(hideTransform, 0.5f);
		}
		tweener = pages[pageNumber - 1].AddComponent<Tweener>();
		tweener.MoveObject(showTransform, 0.5f);
	}
}