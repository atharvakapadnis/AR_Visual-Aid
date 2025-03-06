using UnityEngine;

public class PlanetBehaviour : MonoBehaviour
{
	public Transform sun;
	public float speed = 5;

	private void OnEnable()
	{
		// Setting up randomized coordinates using equation of circle
		float y = transform.localPosition.z;
		float x = Random.Range(-y, y);
		transform.localPosition = new Vector3(x, 0, Mathf.Sign(x) * Mathf.Sqrt(y * y - x * x));
	}

	// Update is called once per frame
	void Update()
	{
		transform.RotateAround(sun.position, Vector3.up, speed * Time.deltaTime * 3); // Implementing planet revolution
		transform.transform.Rotate(Vector3.up * Time.deltaTime * 15f); // Implementing planet rotation
	}
}
