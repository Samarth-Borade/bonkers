using UnityEngine;

// First one to touch this wins.
public class Flag : MonoBehaviour
{
    Transform cloth;

    public static void Create(Vector2 groundPos, Transform parent)
    {
        var go = new GameObject("Flag");
        go.transform.SetParent(parent, false);
        go.transform.position = groundPos;

        Shapes.Box("Pole", new Vector2(0, 1.1f), new Vector2(0.12f, 2.2f), Color.white, 5, go.transform);
        Shapes.Box("Base", new Vector2(0, 0.08f), new Vector2(0.6f, 0.16f), Color.white, 5, go.transform);

        var cloth = new GameObject("Cloth").transform;
        cloth.SetParent(go.transform, false);
        cloth.localPosition = new Vector2(0.06f, 1.85f);
        var tri = Shapes.Make("Triangle", Shapes.Triangle, new Vector2(0.45f, 0), new Vector2(0.7f, 0.9f), Palette.Go, 6, cloth);
        tri.transform.localRotation = Quaternion.Euler(0, 0, -90f);

        FX.Label("FINISH", new Vector2(1.6f, 1.0f), Palette.Go, 0.4f, go.transform, 6);

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.offset = new Vector2(0, 1.1f);
        col.size = new Vector2(1f, 2.2f);

        go.AddComponent<Flag>().cloth = cloth;
    }

    void Update()
    {
        cloth.localScale = new Vector3(1f + 0.12f * Mathf.Sin(Time.time * 6f), 1f, 1f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var p = other.GetComponent<PlayerController>();
        if (p && GameManager.I) GameManager.I.Win(p);
    }
}
