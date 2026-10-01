using UnityEngine;

// On-screen words: "BONK!", "SPIKED!", "GO!" and so on.
// Kept simple on purpose: the text appears, waits a moment, and disappears.
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

    // Text that shows for a moment and then goes away.
    public static void Pop(string text, Vector2 pos, Color color, float size, float life = 0.9f)
    {
        var tm = Label(text, pos, color, size, null, 60, true);
        Object.Destroy(tm.gameObject, life);
    }

    // Someone got hit: a big "BONK!" with a small word under it saying what hit them.
    public static void Bonk(Vector2 pos, string subtitle)
    {
        Pop("BONK!", pos + Vector2.up * 0.3f, Palette.Bonk, 1.5f, 0.9f);
        if (!string.IsNullOrEmpty(subtitle))
            Pop(subtitle, pos - Vector2.up * 0.5f, Color.white, 0.55f, 0.9f);
        Sfx.Play(Sfx.Bonk);
    }
}
