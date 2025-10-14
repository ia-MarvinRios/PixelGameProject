using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody2D MyRb;
    public float Speed;


    void Start()
    {
        MyRb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        MyRb.linearVelocity = new Vector2(+Speed, 0);
    }
}