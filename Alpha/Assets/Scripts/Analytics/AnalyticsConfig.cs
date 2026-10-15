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
//   4. Form "Responses" tab -> Link to Sheets, so every row lands in a Google Sheet.
//   5. In the sheet: File -> Share -> Publish to web -> the responses sheet, as .csv
//      -> Publish. Paste that link into SHEET_CSV_URL in Analytics/visualize.py.
// Leave FormUrl empty to only print the data in the console (handy while testing).
//
// Checking it works: play one race in a WebGL build and look for a new row in the sheet.
// The browser console will show a network error for the request; that's expected
// (Google Forms sends no CORS headers) and the row is still saved.
public static class AnalyticsConfig
{
    public const string FormUrl = "https://docs.google.com/forms/d/e/1FAIpQLScCAMVY7Mml4_0mKXo6JvBBbEdeYY7uNv0O519u21iBktSbFg/formResponse";

    public const string Session = "entry.1959864136";
    public const string Build = "entry.1264408889";
    public const string Level = "entry.370240552";
    public const string Round = "entry.595916300";
    public const string Winner = "entry.1089639943";
    public const string RaceTime = "entry.1451205492";
    public const string LeadChanges = "entry.2056917619";
    public const string FinishGap = "entry.538780445";
    public const string Deaths = "entry.1150240733";
    public const string PowerUpsUsed = "entry.1760519166";
    public const string PowerUpHits = "entry.1944806255";
    public const string Levers = "entry.207500472";
    public const string Stomps = "entry.353529746";
}
