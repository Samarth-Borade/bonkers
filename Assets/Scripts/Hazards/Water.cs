using UnityEngine;

// Not deadly, just annoying: slow, floaty, and you can swim (press jump again and again).
public class Water : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        var p = other.GetComponent<PlayerController>();
        if (p) p.EnterWater();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var p = other.GetComponent<PlayerController>();
        if (p) p.ExitWater();
    }
}
