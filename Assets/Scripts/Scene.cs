using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene : MonoBehaviour
{
	public void Load(string SceneName)
	{
		SceneManager.LoadSceneAsync(SceneName);
	}
}