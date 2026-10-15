using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

// Counts what happens during one race and sends a single summary row when it ends.
// The four things we measure (see the GDD "Analytics" section):
//   1. Lead changes per race        -> is the race actually close?
//   2. Finish gap (in rows)         -> how close was the end?
//   3. Power-up use vs. hits        -> which power-ups land, which get dodged?
//   4. Deaths by hazard and row     -> where is the level too hard?
//
// The deaths, powerups_used and powerup_hits columns pack several counts into one cell,
// as key=count pairs joined by ';' and sorted by key:
//   deaths         row3:SPIKED=2;row5:TOASTED=1    (row 1-6 : the hazard's pop-up word without '!')
//   powerups_used  Fist=2;GravityBomb=1            (PowerUpType names)
//   powerup_hits   Fist=1                          (same names; a use that missed has no hit)
// Analytics/visualize.py unpacks them with parse_counts().
public static class RaceAnalytics
{
    // One id per page load, so races played in the same sitting can be grouped.
    static readonly string SessionId = System.Guid.NewGuid().ToString("N").Substring(0, 8);

    static int leadChanges, lastLeader, levers, stomps;
    static readonly Dictionary<string, int> deaths = new Dictionary<string, int>();
    static readonly Dictionary<string, int> used = new Dictionary<string, int>();
    static readonly Dictionary<string, int> hits = new Dictionary<string, int>();

    /// <summary>Lead changes so far in the current race.</summary>
    public static int LeadChanges => leadChanges;

    /// <summary>Clears all counts. Called by GameManager when the countdown hits GO.</summary>
    public static void BeginRace()
    {
        leadChanges = lastLeader = levers = stomps = 0;
        deaths.Clear();
        used.Clear();
        hits.Clear();
    }

    /// <summary>
    /// Called every frame during the race: 0 = neck and neck, 1 = Red ahead, 2 = Yellow ahead.
    /// A tie doesn't count as a change, so Red -> tie -> Red is still 0 changes.
    /// </summary>
    public static void TrackLeader(int leader)
    {
        if (leader == 0) return;
        if (lastLeader != 0 && leader != lastLeader) leadChanges++;
        lastLeader = leader;
    }

    /// <summary>A player died on their current row. Called by PlayerController with the hazard's pop-up word.</summary>
    public static void Death(PlayerController p, string word) => Add(deaths, $"row{p.Row + 1}:{word.TrimEnd('!')}");
    /// <summary>A power-up was used. Called by PowerUps when the player presses the power key.</summary>
    public static void PowerUpUsed(PlayerController p, PowerUpType t) => Add(used, t.ToString());
    /// <summary>A power-up landed on the rival. Called from the power-up's hit callback (PowerUps, Banana, FakeBox).</summary>
    public static void Hit(PlayerController attacker, PowerUpType t) => Add(hits, t.ToString());
    /// <summary>The chaser pulled a lever. Called by Lever.</summary>
    public static void LeverPulled(PlayerController p) => levers++;
    /// <summary>A player stomped the rival's head. Called by PlayerController.</summary>
    public static void Stomp(PlayerController p) => stomps++;

    static void Add(Dictionary<string, int> d, string key) => d[key] = d.TryGetValue(key, out var n) ? n + 1 : 1;

    static string Flat(Dictionary<string, int> d) => string.Join(";", d.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}={kv.Value}"));

    /// <summary>
    /// Sends the race's summary row. Called by GameManager.Win.
    /// level and round are 0-based here and sent 1-based. finishGap is how many rows
    /// the loser still had to climb when the winner touched the flag.
    /// </summary>
    public static void EndRace(MonoBehaviour host, int level, int round, string winner, float raceTime, float finishGap)
    {
        var row = new Dictionary<string, string>
        {
            [AnalyticsConfig.Session] = SessionId,
            [AnalyticsConfig.Build] = Application.version,
            [AnalyticsConfig.Level] = (level + 1).ToString(),
            [AnalyticsConfig.Round] = (round + 1).ToString(),
            [AnalyticsConfig.Winner] = winner,
            [AnalyticsConfig.RaceTime] = raceTime.ToString("0.0"),
            [AnalyticsConfig.LeadChanges] = leadChanges.ToString(),
            [AnalyticsConfig.FinishGap] = finishGap.ToString("0.00"),
            [AnalyticsConfig.Deaths] = Flat(deaths),
            [AnalyticsConfig.PowerUpsUsed] = Flat(used),
            [AnalyticsConfig.PowerUpHits] = Flat(hits),
            [AnalyticsConfig.Levers] = levers.ToString(),
            [AnalyticsConfig.Stomps] = stomps.ToString(),
        };

        Debug.Log("ANALYTICS " + string.Join(" | ", row.Values));
        if (!string.IsNullOrEmpty(AnalyticsConfig.FormUrl)) host.StartCoroutine(Send(row));
    }

    static IEnumerator Send(Dictionary<string, string> row)
    {
        var form = new WWWForm();
        foreach (var kv in row) form.AddField(kv.Key, kv.Value);
        using (var req = UnityWebRequest.Post(AnalyticsConfig.FormUrl, form))
        {
            yield return req.SendWebRequest();
            // In the browser Google Forms answers without CORS headers, so the request
            // is saved but reported as an error here. That's expected; the row still arrives.
        }
    }
}
