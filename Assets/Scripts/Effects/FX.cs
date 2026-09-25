using UnityEngine;

// All the juice: floating text, the big BONK!, particle bursts, confetti.
public static class FX
{
    static Font font;
    public static Font Font => font ? font : font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    // World-space text. size = roughly the letter height in world units.
    public static TextMesh Label(string text, Vector2 pos, Color color, float size,
        Transform parent = null, int order = 50, bool outline = false)
    {
        var go = new GameObject("Text " + text);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;

        var tm = go.AddComponent<TextMesh>();
        tm.font = Font;
        tm.text = text;
        tm.fontSize = 64;
        tm.characterSize = size * 0.15f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontStyle = FontStyle.Bold;
        tm.color = color;
        var mr = go.GetComponent<MeshRenderer>();
        mr.sharedMaterial = Font.material;
        mr.sortingOrder = order;

        if (outline)
        {
            var shadow = Label(text, new Vector2(0.06f, -0.06f) * size, Color.black, size, go.transform, order - 1);
            shadow.name = "Shadow";
        }
        return tm;
    }

    // Text that pops in, floats up and fades away.
    public static void Pop(string text, Vector2 pos, Color color, float size, float life = 0.9f)
    {
        var tm = Label(text, pos, color, size, null, 60, true);
        tm.gameObject.AddComponent<FloatingText>().Init(life, 1.2f, 1.2f);
    }

    // The star of the show.
    public static void Bonk(Vector2 pos, string subtitle)
    {
        var tm = Label("BONK!", pos + Vector2.up * 0.3f, Palette.Bonk, 1.5f, null, 70, true);
        tm.gameObject.AddComponent<FloatingText>().Init(1.0f, 1.0f, 1.8f, 12f);
        if (!string.IsNullOrEmpty(subtitle))
        {
            var sub = Label(subtitle, pos - Vector2.up * 0.5f, Color.white, 0.55f, null, 70, true);
            sub.gameObject.AddComponent<FloatingText>().Init(1.0f, 1.0f, 1.2f);
        }
        Burst(pos, Palette.Bonk, 14, 0.14f, 9f);
        CameraRig.Shake(0.25f, 0.3f);
        Sfx.Play(Sfx.Bonk);
    }

    public static void Burst(Vector2 pos, Color color, int count, float size, float speed = 6f)
    {
        for (int i = 0; i < count; i++)
        {
            var sr = Shapes.Box("Bit", pos, Vector2.one * size * Random.Range(0.6f, 1.4f), color, 40);
            sr.gameObject.AddComponent<Debris>().velocity = Random.insideUnitCircle.normalized * speed * Random.Range(0.4f, 1f);
        }
    }

    public static void Confetti(Vector2 pos)
    {
        for (int i = 0; i < 80; i++)
        {
            var c = Color.HSVToRGB(Random.value, 0.8f, 1f);
            var sr = Shapes.Box("Confetti", pos, new Vector2(0.18f, 0.1f), c, 45);
            var d = sr.gameObject.AddComponent<Debris>();
            d.velocity = new Vector2(Random.Range(-9f, 9f), Random.Range(6f, 16f));
            d.life = Random.Range(1.5f, 2.5f);
            d.gravity = 12f;
        }
    }
}

public class FloatingText : MonoBehaviour
{
    float life, age, rise, popScale, wobble;
    TextMesh[] meshes;
    Color[] baseColors;
    Vector3 start;

    public void Init(float life, float rise, float popScale, float wobble = 0f)
    {
        this.life = life;
        this.rise = rise;
        this.popScale = popScale;
        this.wobble = wobble;
        start = transform.position;
        meshes = GetComponentsInChildren<TextMesh>();
        baseColors = new Color[meshes.Length];
        for (int i = 0; i < meshes.Length; i++) baseColors[i] = meshes[i].color;
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        age += Time.deltaTime;
        float t = age / life;
        if (t >= 1f) { Destroy(gameObject); return; }

        // Pop: grow past full size then settle back.
        float s = t < 0.15f ? Mathf.Lerp(0f, popScale, t / 0.15f) : Mathf.Lerp(popScale, 1f, Mathf.Min(1f, (t - 0.15f) / 0.2f));
        transform.localScale = Vector3.one * s;
        transform.position = start + Vector3.up * (rise * Mathf.Sqrt(t));
        transform.rotation = Quaternion.Euler(0, 0, wobble * Mathf.Sin(age * 25f) * (1 - t));

        float alpha = t < 0.6f ? 1f : 1f - (t - 0.6f) / 0.4f;
        for (int i = 0; i < meshes.Length; i++)
        {
            var c = baseColors[i];
            c.a *= alpha;
            meshes[i].color = c;
        }
    }
}

public class Debris : MonoBehaviour
{
    public Vector2 velocity;
    public float life = 0.6f;
    public float gravity = 18f;
    float age, spin;
    Vector3 baseScale;
    SpriteRenderer sr;

    void Start()
    {
        baseScale = transform.localScale;
        spin = Random.Range(-720f, 720f);
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float dt = Time.deltaTime;
        age += dt;
        if (age >= life) { Destroy(gameObject); return; }
        velocity.y -= gravity * dt;
        transform.position += (Vector3)(velocity * dt);
        transform.Rotate(0, 0, spin * dt);
        transform.localScale = baseScale * (1f - age / life);
    }
}
