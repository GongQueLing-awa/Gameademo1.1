using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sign : MonoBehaviour
{
    public GameObject signSprite;
    public Transform playerTransform;
    private Animator anim;
    private bool isTablet;

    private void Awake()
    {
        anim = signSprite.GetComponent<Animator>();
    }

private void Update()
    {
       signSprite.GetComponent<SpriteRenderer>().enabled = isTablet;
      
       float direction = playerTransform.localScale.x > 0 ? 1f : -1f;
       signSprite.transform.localScale = new Vector3(0.5f * direction, 0.5f, 0.5f);
    }







    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("interactable"))
        {
            isTablet = true;
            anim.SetBool("isTablet", isTablet);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("interactable"))
        {
            isTablet = false;
            anim.SetBool("isTablet", isTablet);
        }
    }
}
