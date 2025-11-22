using UnityEngine;

// Simple inheritance for standard enemies
public class MinionMonster : Monster
{
    public override void ResetMonster()
    {
        base.ResetMonster();
        // Add specific minion logic here if needed (e.g. random colors)
    }
}