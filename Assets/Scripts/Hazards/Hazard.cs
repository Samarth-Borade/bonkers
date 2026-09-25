using UnityEngine;

// Lava, spikes, grinders: touch it and you go back to the start of your row.
public class Hazard : MonoBehaviour
{
    public string word = "OUCH!";

    void OnTriggerEnter2D(Collider2D other) => Hit(other);
    void OnTriggerStay2D(Collider2D other) => Hit(other);

    void Hit(Collider2D other)
    {
        var p = other.GetComponent<PlayerController>();
        if (p) p.Die(word);
    }
}
