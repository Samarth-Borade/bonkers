using UnityEngine;

// Not deadly, just annoying: slow, floaty, and you can swim (press jump again and again).
public class Water : MonoBehaviour
{
    SpriteRenderer sr;
    float seed;

    void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        seed = Random.value * 10f;
    }

    void Update()
    {
        if (!sr) return;
        var c = Palette.Water;
        c.a += 0.06f * Mathf.Sin(Time.time * 2f + seed);
        sr.color = c;
    }

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
