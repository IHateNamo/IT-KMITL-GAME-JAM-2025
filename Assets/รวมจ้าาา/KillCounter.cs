using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class KillCounter : MonoBehaviour
{
    public GameManager gameManager;

    public PlayableDirector startChangingScene;

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "PastScene")
        {
            if (gameManager.AllKillCount >= 18)
            {
                gameManager.canSpawn = false;

                StartCoroutine(changeSceneOne());
            }
        }
        else if (SceneManager.GetActiveScene().name == "FutureScene")
        {
            if (gameManager.AllKillCount >= 24)
            {

            }
        }
    }

    private IEnumerator changeSceneOne()
    {
        if (startChangingScene != null)
        {
            startChangingScene.Play();

            while (startChangingScene.state == PlayState.Playing)
            {
                yield return null;
            }
        }
        else
        {
            Debug.LogWarning("SAI CHANGESCENE DUOI");
        }

        //Reset kill count
        gameManager.AllKillCount = 0;

        SceneManager.LoadScene("FutureScene");
    }
}
