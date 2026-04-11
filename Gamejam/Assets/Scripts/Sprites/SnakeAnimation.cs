using UnityEngine;

public class SnakeAnimation : MonoBehaviour
{
    private Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        PlayAnimation();
    }
    private void PlayAnimation()
    {
        animator = GetComponent<Animator>();
        animator.Play("SnakeMove");
    }
}
