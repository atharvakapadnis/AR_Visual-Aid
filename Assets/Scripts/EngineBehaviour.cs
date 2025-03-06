using UnityEngine;
using UnityEngine.UI;
using XGI.Tools.Window;

public class EngineBehaviour : MonoBehaviour
{
	float rotation;

	float cycle;
	bool phaseDirection;
	[SerializeField] int phase = 0;
	float cyclePhase
	{
		set
		{
			if ((cycle > value) != phaseDirection) // Phase changed
			{
				switch (++phase)
				{
					case 1:
						ParticleSystem.MainModule main = combustionAnimation.main;
						main.simulationSpeed = speed.value / 500;
						combustionAnimation.Play();
						if(!outerPartsState.isOn && speed.value < 200) { QuickMessage quickMessage = new QuickMessage("Combustion Stroke", Color.red, 40); }
						break;
					case 2:
						if(!outerPartsState.isOn && speed.value < 200) { QuickMessage quickMessage = new QuickMessage("Exhaust Stroke", Color.black, 40); }
						break;
					case 3:
						if(!outerPartsState.isOn && speed.value < 200) { QuickMessage quickMessage = new QuickMessage("Intake Stroke", Color.blue, 40); }
						break;
					case 4:
						if(!outerPartsState.isOn && speed.value < 200) { QuickMessage quickMessage = new QuickMessage("Compression Stroke", Color.green, 40); }
						phase = 0;
						break;
				}
			}
			phaseDirection = cycle > value;
			cycle = value;
		}
	}


	[Header("Control references")]
	public Slider speed;
	public Toggle outerPartsState;
	public ParticleSystem combustionAnimation;

	[Header("GameObject references")]
	[SerializeField] GameObject crankShaft;
	[SerializeField] GameObject pistonRod;
	[SerializeField] GameObject pistonHead;

	[SerializeField] GameObject[] hideableParts;

	[SerializeField] Transform pistonExtended;
	[SerializeField] Transform pistonContracted;

	private void Update()
	{
		rotation += Time.deltaTime * speed.value; // Incrementing rotation
		cyclePhase = Mathf.Cos((rotation - 55f) * Mathf.Deg2Rad); // Calculating phase
		crankShaft.transform.rotation = Quaternion.Euler(rotation, 0, 0); // Rotating crankshaft
		pistonRod.transform.LookAt(pistonHead.transform); // Aligning piston rod with piston head
		pistonHead.transform.position = Vector3.LerpUnclamped(pistonContracted.position, pistonExtended.position, cycle); // Positioning piston head
	}

	public void SetOuterPartsVisibility() { foreach (GameObject part in hideableParts) part.SetActive(outerPartsState.isOn); }

}