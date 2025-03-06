using TMPro;
using UnityEngine;
using XGI.Tools;
using XGI.Tools.Window;
public class QueueManager : MonoBehaviour
{
	int count;
	int front, rear;
	GameObject[] elements;
	[SerializeField] Transform elementPosition, spawnPosition, despawnPosition;

	public TextMeshProUGUI elementCount;
	private void Awake()
	{
		count = 0;
		elements = new GameObject[8];
		elementCount.text = "0";
	}

	public void OnAdd()
	{
		if(front < 8)
		{
			if (front != 0) elements[front - 1].GetComponent<Renderer>().material.color = new Color(1, 0.749f, 0); // Coloring previous element to Amber Color
			
			// Instantiating new element
			elements[front] = Instantiate(Resources.Load<GameObject>("Cube"));
			elements[front].GetComponent<Renderer>().material.color = new Color(0.502f, 0, 0); // Coloring current element to Maroon Color

			// Positioning new element
			elements[front].transform.position = spawnPosition.position;
			elements[front].transform.rotation = spawnPosition.rotation;
			elementPosition.localPosition = Vector3.up * 0.2f * front;
			Lerper lerper = elements[front].AddComponent<Lerper>();
			lerper.MoveObject(elementPosition, 7.5f);

			front++;
			count++;
		}
		else
		{
			QuickMessage quickMessage = new QuickMessage("Queue Overflow", Color.red, 40);
		}

		elementCount.text = count.ToString();
	}

	public void OnRemove()
	{
		if(count > 0)
		{
			count--;

			Lerper lerper = elements[rear].AddComponent<Lerper>();
			lerper.MoveObject(despawnPosition, 7.5f, true);
			rear++;

			if (count != 0) elements[rear - 1].GetComponent<Renderer>().material.color = new Color(0.502f, 0, 0); // Coloring previous element to Maroon Color
			else front = rear = 0;
		}
		else
		{
			QuickMessage quickMessage = new QuickMessage("Queue Underflow", Color.red, 40);
		}

		elementCount.text = count.ToString();
	}
}