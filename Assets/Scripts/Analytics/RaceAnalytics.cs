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
public static class RaceAnalytics
{
    static readonly string SessionId = System.Guid.NewGuid().ToString("N").Substring(0, 8);

    static int leadChanges, lastLeader, levers, stomps;
    static readonly Dictionary<string, int> deaths = new Dictionary<string, int>();
    static readonly Dictionary<string, int> used = new Dictionary<string, int>();
    static readonly Dictionary<string, int> hits = new Dictionary<string, int>();

    public static int LeadChanges => leadChanges;

    public static void BeginRace()
    {
        leadChanges = lastLeader = levers = stomps = 0;
        deaths.Clear();
        used.Clear();
        hits.Clear();
    }

    // Called every frame during the race: 0 = neck and neck, 1 = Red ahead, 2 = Yellow ahead.
    public static void TrackLeader(int leader)
    {
        if (leader == 0) return;
        if (lastLeader != 0 && leader != lastLeader) leadChanges++;
        lastLeader = leader;
    }

    public static void Death(PlayerController p, string word) => Add(deaths, $"row{p.Row + 1}:{word.TrimEnd('!')}");
    public static void PowerUpUsed(PlayerController p, PowerUpType t) => Add(used, t.ToString());
    public static void Hit(PlayerController attacker, PowerUpType t) => Add(hits, t.ToString());
    public static void LeverPulled(PlayerController p) => levers++;
    public static void Stomp(PlayerController p) => stomps++;

    static void Add(Dictionary<string, int> d, string key) => d[key] = d.TryGetValue(key, out var n) ? n + 1 : 1;

    static string Flat(Dictionary<string, int> d) => string.Join(";", d.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}={kv.Value}"));

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
