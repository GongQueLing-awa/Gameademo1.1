using UnityEngine;

public class Bullet : MonoBehaviour
{
    public ShootController controller;
    private void OnTriggerEnter2D(Collider2D other)
    {
        controller.imTrying(other);
    }
}
