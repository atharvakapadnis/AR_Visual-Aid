using TMPro;
using UnityEngine;
using XGI.Tools;
using XGI.Tools.Window;
public class StackManager : MonoBehaviour
{
	int count;
	GameObject[] elements;
	[SerializeField] Transform elementPosition, spawnPosition;

	public TextMeshProUGUI elementCount;

	private void Awake()
	{
		count = 0;
		elements = new GameObject[8];
		elementCount.text = "0";
	}

	public void OnAdd()
	{
		if(count < 8)
		{
			if (count != 0) elements[count - 1].GetComponent<Renderer>().material.color = new Color(1, 0.749f, 0); // Coloring previous element to Amber Color
			
			// Instantiating new element
			elements[count] = Instantiate(Resources.Load<GameObject>("Cube"));
			elements[count].GetComponent<Renderer>().material.color = new Color(0.502f, 0, 0); // Coloring current element to Maroon Color

			// Positioning new element
			elements[count].transform.position = spawnPosition.position;
			elementPosition.localPosition = Vector3.up * 0.2f * count;
			Lerper lerper = elements[count].AddComponent<Lerper>();
			lerper.MoveObject(elementPosition, 7.5f);
			
			count++;
		}
		else
		{
			QuickMessage quickMessage = new QuickMessage("Stack Overflow", Color.red, 40);
		}

		elementCount.text = count.ToString();
	}

	public void OnRemove()
	{
		if(count > 0)
		{
			count--;

			Lerper lerper = elements[count].AddComponent<Lerper>();
			lerper.MoveObject(spawnPosition, 7.5f, true);

			if (count != 0) elements[count - 1].GetComponent<Renderer>().material.color = new Color(0.502f, 0, 0); // Coloring previous element to Maroon Color
		}
		else
		{
			QuickMessage quickMessage = new QuickMessage("Stack Underflow", Color.red, 40);
		}

		elementCount.text = count.ToString();
	}
}