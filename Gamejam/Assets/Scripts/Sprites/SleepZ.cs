using UnityEngine;

public class SleepZ : MonoBehaviour
{
    private Animator animator;
    public GameObject ZZZPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayAnimation();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void PlayAnimation()
    {
        animator = GetComponent<Animator>();
        animator.Play("SleepZ");
        Destroy(ZZZPrefab, 1f);
    }

}
