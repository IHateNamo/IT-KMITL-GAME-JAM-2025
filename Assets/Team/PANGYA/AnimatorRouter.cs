using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorRouter : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private void Reset()
    {
        animator = GetComponent<Animator>();
    }

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void OnEnable()
    {
        GameEvents.OnTrigger += HandleTrigger;
        GameEvents.OnBool    += HandleBool;
        GameEvents.OnInt     += HandleInt;
    }

    private void OnDisable()
    {
        GameEvents.OnTrigger -= HandleTrigger;
        GameEvents.OnBool    -= HandleBool;
        GameEvents.OnInt     -= HandleInt;
    }

    private void HandleTrigger(string paramName)
    {
        if (animator != null)
        {
            animator.SetTrigger(paramName);
        }
    }

    private void HandleBool(string paramName, bool value)
    {
        if (animator != null)
        {
            animator.SetBool(paramName, value);
        }
    }

    private void HandleInt(string paramName, int value)
    {
        if (animator != null)
        {
            animator.SetInteger(paramName, value);
        }
    }
}
