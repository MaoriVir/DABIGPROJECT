using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Enemy1Script : MonoBehaviour
{
    
   
    
    private Rigidbody2D rb;
    private Animator anim;
    
    private bool isGrounded;
    private bool isFacingRight = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   
    
}
