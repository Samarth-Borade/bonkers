using UnityEngine;

// On-screen text: player panels, score, countdown, title and win screens.
// Uses Unity's simple OnGUI so there's no Canvas to set up.
public class Hud : MonoBehaviour
{
    Texture2D white;
    GUIStyle style;

    void OnGUI()
    {
        var gm = GameManager.I;
        if (!gm) return;
        if (!white) { white = Texture2D.whiteTexture; }
        if (style == null) style = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, wordWrap = false, richText = true };

        float s = Mathf.Min(Screen.height / 720f, Screen.width / 1280f);

        PlayerPanel(gm.Red, new Rect(14 * s, 10 * s, 380 * s, 70 * s), TextAnchor.UpperLeft, s);
        PlayerPanel(gm.Yellow, new Rect(Screen.width - 394 * s, 10 * s, 380 * s, 70 * s), TextAnchor.UpperRight, s);

        // Score in the middle
        Text(new Rect(0, 12 * s, Screen.width, 40 * s),
            $"<color=#{Hex(Palette.Red)}>{GameManager.RedWins}</color>  -  <color=#{Hex(Palette.Yellow)}>{GameManager.YellowWins}</color>",
            30 * s, Color.white, TextAnchor.UpperCenter);
        if (gm.Phase == GameManager.State.Race || gm.Phase == GameManager.State.Won)
            Text(new Rect(0, 46 * s, Screen.width, 30 * s), gm.RaceTime.ToString("0.0") + "s", 18 * s, new Color(1, 1, 1, 0.6f), TextAnchor.UpperCenter);
        ProgressBar(gm, s);

        switch (gm.Phase)
        {
            case GameManager.State.Title: TitleScreen(s); break;
            case GameManager.State.Countdown:
                Text(new Rect(0, 0, Screen.width, Screen.height), Mathf.CeilToInt(gm.CountdownLeft).ToString(),
                    200 * s, Color.white, TextAnchor.MiddleCenter);
                break;
            case GameManager.State.Won: WinScreen(gm, s); break;
        }
    }

    void PlayerPanel(PlayerController p, Rect r, TextAnchor align, float s)
    {
        Fill(r, new Color(0, 0, 0, 0.45f));
        Fill(new Rect(align == TextAnchor.UpperLeft ? r.x : r.xMax - 6 * s, r.y, 6 * s, r.height), p.color);

        var inner = new Rect(r.x + 16 * s, r.y + 6 * s, r.width - 32 * s, r.height);
        string keys = p.displayName == "RED" ? "S" : "DOWN";
        bool leading = GameManager.I.IsLeading(p);
        string place = leading ? "<color=#7CFF9A>LEADING</color>" : "<color=#FF9A5C>CHASING</color>";
        Text(inner, $"{p.displayName}   <size={(int)(15 * s)}>row {p.Row + 1}/{LevelBuilder.Rows}   {place}</size>", 22 * s, p.color, align);

        string power = p.Held == PowerUpType.None
            ? "<color=#888888>no power - find a ? box</color>"
            : $"<color=#{Hex(PowerUps.ColorOf(p.Held))}>{PowerUps.NameOf(p.Held)}</color>  <color=#bbbbbb>[{keys}] {PowerUps.HintOf(p.Held)}</color>";
        Text(new Rect(inner.x, inner.y + 30 * s, inner.width, 24 * s), power, 16 * s, Color.white, align);

        string fx = Effects(p);
        if (fx.Length > 0)
            Text(new Rect(inner.x, r.yMax + 2 * s, inner.width, 22 * s), fx, 14 * s, Color.white, align);
    }

    // Both racers on one bar, start -> flag. Nobody ever has to wonder how far behind they are.
    void ProgressBar(GameManager gm, float s)
    {
        float w = 360 * s, h = 8 * s;
        var bar = new Rect((Screen.width - w) / 2f, 84 * s, w, h);
        Fill(new Rect(bar.x - 2 * s, bar.y - 2 * s, bar.width + 4 * s, bar.height + 4 * s), new Color(0, 0, 0, 0.5f));
        Fill(bar, new Color(1, 1, 1, 0.12f));
        for (int i = 1; i < LevelBuilder.Rows; i++)
            Fill(new Rect(bar.x + bar.width * i / LevelBuilder.Rows - 1 * s, bar.y, 2 * s, h), new Color(1, 1, 1, 0.25f));
        Fill(new Rect(bar.xMax - 4 * s, bar.y - 6 * s, 4 * s, h + 12 * s), Palette.Go);

        Marker(gm.Red, bar, s, -1);
        Marker(gm.Yellow, bar, s, 1);
    }

    void Marker(PlayerController p, Rect bar, float s, int side)
    {
        float t = Mathf.Clamp01(LevelBuilder.Progress(p) / LevelBuilder.Rows);
        float size = 14 * s;
        float x = bar.x + bar.width * t - size / 2f;
        float y = side < 0 ? bar.y - size - 1 * s : bar.yMax + 1 * s;
        Fill(new Rect(x, y, size, size), p.color);
    }

    static string Effects(PlayerController p)
    {
        string e = "";
        if (p.slowT > 0) e += "SLOW  ";
        if (p.freezeT > 0) e += "FROZEN  ";
        if (p.heavyT > 0) e += "HEAVY  ";
        if (p.reverseT > 0) e += "REVERSED  ";
        if (p.speedT > 0) e += "ZOOM  ";
        if (p.superJumpT > 0) e += "SUPER JUMP  ";
        if (p.shieldT > 0) e += "SHIELD  ";
        return e.Trim();
    }

    void TitleScreen(float s)
    {
        Fill(new Rect(0, 0, Screen.width, Screen.height), new Color(0, 0, 0, 0.72f));
        float cx = Screen.width / 2f, y = Screen.height * 0.16f;

        Text(new Rect(0, y, Screen.width, 130 * s),
            $"<color=#{Hex(Palette.Red)}>BON</color><color=#{Hex(Palette.Yellow)}>KERS!</color>", 110 * s, Color.white, TextAnchor.MiddleCenter);
        y += 125 * s;
        Text(new Rect(0, y, Screen.width, 30 * s), "Race to the flag. Bonk your friend on the way.", 22 * s, new Color(1, 1, 1, 0.8f), TextAnchor.MiddleCenter);
        y += 60 * s;

        float w = 330 * s;
        Controls(new Rect(cx - w - 20 * s, y, w, 140 * s), "RED", Palette.Red, "A / D", "W", "S");
        Controls(new Rect(cx + 20 * s, y, w, 140 * s), "YELLOW", Palette.Yellow, "LEFT / RIGHT", "UP", "DOWN");
        y += 160 * s;

        Text(new Rect(0, y, Screen.width, 60 * s),
            "Hidden <b>?</b> boxes give a FIXED prize based on your place: <b>chasing = attacks</b>, <b>leading = defence</b>.\n" +
            "Every attack can be dodged. Shove your rival, STOMP their head. Lava, spikes and saws send you back.",
            15 * s, new Color(1, 1, 1, 0.7f), TextAnchor.MiddleCenter);
        y += 80 * s;

        Text(new Rect(0, y, Screen.width, 40 * s), "Press SPACE to start", 30 * s, Color.white, TextAnchor.MiddleCenter);
    }

    void Controls(Rect r, string who, Color c, string move, string jump, string use)
    {
        float s = Mathf.Min(Screen.height / 720f, Screen.width / 1280f);
        Fill(r, new Color(1, 1, 1, 0.06f));
        Fill(new Rect(r.x, r.y, r.width, 5 * s), c);
        Text(new Rect(r.x, r.y + 12 * s, r.width, 30 * s), who, 26 * s, c, TextAnchor.UpperCenter);
        Text(new Rect(r.x, r.y + 50 * s, r.width, 80 * s),
            $"Move   <b>{move}</b>\nJump   <b>{jump}</b>\nPower  <b>{use}</b>", 18 * s, Color.white, TextAnchor.UpperCenter);
    }

    void WinScreen(GameManager gm, float s)
    {
        var w = gm.Winner;
        Fill(new Rect(0, Screen.height * 0.36f, Screen.width, 190 * s), new Color(0, 0, 0, 0.7f));
        Text(new Rect(0, Screen.height * 0.36f + 10 * s, Screen.width, 110 * s), w.displayName + " WINS!", 90 * s, w.color, TextAnchor.MiddleCenter);
        Text(new Rect(0, Screen.height * 0.36f + 120 * s, Screen.width, 30 * s), $"in {gm.RaceTime:0.0} seconds", 20 * s, Color.white, TextAnchor.MiddleCenter);
        Text(new Rect(0, Screen.height * 0.36f + 150 * s, Screen.width, 30 * s), "SPACE = rematch     ESC = menu", 18 * s, new Color(1, 1, 1, 0.7f), TextAnchor.MiddleCenter);
    }

    // ---------- helpers ----------

    void Text(Rect r, string text, float size, Color color, TextAnchor align)
    {
        style.fontSize = Mathf.Max(8, Mathf.RoundToInt(size));
        style.alignment = align;
        var shadow = r;
        shadow.x += 2; shadow.y += 2;
        style.normal.textColor = new Color(0, 0, 0, color.a * 0.8f);
        GUI.Label(shadow, StripColors(text), style);
        style.normal.textColor = color;
        GUI.Label(r, text, style);
    }

    void Fill(Rect r, Color c)
    {
        var old = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(r, white);
        GUI.color = old;
    }

    static string Hex(Color c) => ColorUtility.ToHtmlStringRGB(c);

    static string StripColors(string s) => System.Text.RegularExpressions.Regex.Replace(s, "</?color[^>]*>", "");
}
