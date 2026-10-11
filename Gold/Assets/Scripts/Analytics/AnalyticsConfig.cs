// Where race analytics get sent. One row per finished race goes to a Google Form,
// and the form saves it into a Google Sheet that Analytics/visualize.py turns into charts.
//
// Setup (Product Manager, once):
//   1. Make a Google Form with 13 short-answer questions named exactly:
//      session, build, level, round, winner, race_time, lead_changes, finish_gap,
//      deaths, powerups_used, powerup_hits, levers, stomps
//   2. Form "..." menu -> Get pre-filled link -> type 1, 2, 3... in each box -> Get link.
//      The link contains entry.123456=1, entry.789012=2 ... copy each entry ID below.
//   3. FormUrl is the form's address with "/viewform" replaced by "/formResponse".
// Leave FormUrl empty to only print the data in the console (handy while testing).
public static class AnalyticsConfig
{
    public const string FormUrl = "";

    public const string Session = "entry.0000000001";
    public const string Build = "entry.0000000002";
    public const string Level = "entry.0000000003";
    public const string Round = "entry.0000000004";
    public const string Winner = "entry.0000000005";
    public const string RaceTime = "entry.0000000006";
    public const string LeadChanges = "entry.0000000007";
    public const string FinishGap = "entry.0000000008";
    public const string Deaths = "entry.0000000009";
    public const string PowerUpsUsed = "entry.0000000010";
    public const string PowerUpHits = "entry.0000000011";
    public const string Levers = "entry.0000000012";
    public const string Stomps = "entry.0000000013";
}
