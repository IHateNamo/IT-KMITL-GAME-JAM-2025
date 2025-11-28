using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMusic : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip pastMusic;
    public AudioClip futureMusic;

    void Start()
    {
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    void OnEnable()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        PlayMusicForScene(newScene.name);
    }

    void PlayMusicForScene(string sceneName)
    {
        if (sceneName == "PastScene")
        {
            audioSource.clip = pastMusic;
        }
        else if (sceneName == "FutureScene")
        {
            audioSource.clip = futureMusic;
        }
        else
        {
            return; // Do nothing for other scenes
        }

        audioSource.Play();
    }
}