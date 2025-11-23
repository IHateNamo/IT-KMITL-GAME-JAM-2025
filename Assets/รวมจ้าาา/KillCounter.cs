using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class KillCounter : MonoBehaviour
{
    public GameManager gameManager;

    public VideoPlayer videoPlayer;

    public GameObject warpPlayer;

    private bool isChangingScene = false;

    public GameObject dontDestroyOnLoad;

    private void Update()
    {
        if (isChangingScene) return;

        string scene = SceneManager.GetActiveScene().name;

        // --- PAST SCENE LOGIC ---
        if (scene == "PastScene")
        {
            if (gameManager.AllKillCount >= 18)
            {
                gameManager.canSpawn = false;
                StartCoroutine(ChangeSceneOne());
                isChangingScene = true;
            }
        }

        // --- FUTURE SCENE LOGIC ---
        if (scene == "FutureScene")
        {
            if (gameManager.AllKillCount >= 24)
            {
                gameManager.canSpawn = false;
                StartCoroutine(ChangeToEnding());
                isChangingScene = true;
            }
        }
    }

    private IEnumerator ChangeSceneOne()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Prepare();
            while (!videoPlayer.isPrepared)
                yield return null;

            videoPlayer.Play();
            while (videoPlayer.isPlaying)
                yield return null;
        }
        else
        {
            Debug.LogWarning("VideoPlayer is missing.");
        }

        gameManager.AllKillCount = 0;
        warpPlayer.SetActive(false);
        SceneManager.LoadScene("FutureScene");

        isChangingScene = false;

        gameManager.canSpawn = true;
    }

    private IEnumerator ChangeToEnding()
    {
        // Wait a short moment (optional)
        yield return new WaitForSeconds(1f);

        Destroy(dontDestroyOnLoad);

        SceneManager.LoadScene("Ending");

    }
}