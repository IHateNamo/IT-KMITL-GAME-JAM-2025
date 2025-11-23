using UnityEngine;

public class KillCounter : MonoBehaviour
{
    public GameManager gameManager;

    private void Update()
    {
        if (gameManager.AllKillCount >= 18)
        {
            //Debug.LogWarning("Changed Scene");
        }
    }
}
