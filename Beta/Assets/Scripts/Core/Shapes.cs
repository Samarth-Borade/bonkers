using UnityEngine;

// Makes plain shapes (square, circle, triangle) in code, so the game needs zero art files.
public static class Shapes
{
    static Sprite square, circle, triangle;

    public static Sprite Square => square ? square : square = Build("Square", 8, (x, y, n) => true, FilterMode.Point);

    public static Sprite Circle => circle ? circle : circle = Build("Circle", 64, (x, y, n) =>
    {
        float r = n / 2f, dx = x + 0.5f - r, dy = y + 0.5f - r;
        return dx * dx + dy * dy <= r * r;
    }, FilterMode.Bilinear);

    // Points up. Rotate the object to point it somewhere else.
    public static Sprite Triangle => triangle ? triangle : triangle = Build("Triangle", 64,
        (x, y, n) => Mathf.Abs(x + 0.5f - n / 2f) <= (n - y) / 2f, FilterMode.Bilinear);

    static Sprite Build(string name, int n, System.Func<int, int, int, bool> inside, FilterMode filter)
    {
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false)
        {
            name = name, filterMode = filter, wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.DontUnloadUnusedAsset
        };
        var px = new Color32[n * n];
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
                px[y * n + x] = inside(x, y, n) ? new Color32(255, 255, 255, 255) : new Color32(255, 255, 255, 0);
        tex.SetPixels32(px);
        tex.Apply();
        var sprite = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        sprite.hideFlags = HideFlags.DontUnloadUnusedAsset;
        return sprite;
    }

    // One sprite = one GameObject. Size is in world units (1 unit = 1 square).
    public static SpriteRenderer Make(string name, Sprite sprite, Vector2 pos, Vector2 size, Color color,
        int order = 0, Transform parent = null)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = order;
        return sr;
    }

    public static SpriteRenderer Box(string name, Vector2 pos, Vector2 size, Color color,
        int order = 0, Transform parent = null) => Make(name, Square, pos, size, color, order, parent);
}
