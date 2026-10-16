"""
BONKERS! analytics -> charts.

Every finished race sends one row to our Google Form (see Alpha/Assets/Scripts/Analytics/).
The form saves rows into a Google Sheet. This script reads that sheet and draws one
chart per metric into Analytics/charts/.

Usage:
    python3 Analytics/visualize.py                      # read the sheet (SHEET_CSV_URL below)
    python3 Analytics/visualize.py --csv results.csv    # read a downloaded CSV instead
    python3 Analytics/visualize.py --mock               # made-up data, for GDD mockups only

To get SHEET_CSV_URL: in the response sheet, File -> Share -> Publish to web ->
pick the sheet, format "Comma-separated values (.csv)" -> Publish, and paste the link.

Needs: pip install -r Analytics/requirements.txt
"""
import argparse
import os
import random
from typing import Optional

import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
import pandas as pd

SHEET_CSV_URL = "https://docs.google.com/spreadsheets/d/e/2PACX-1vTM8VB75PBF_pMHWLvSckpN9oGnv8yzutwRKOzSfVSZ5poY6FHzXQHJ2k16jOiIZMDWyFE56vNaIGoR/pub?gid=830941285&single=true&output=csv"

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(HERE, "charts")

# Same names, same order as the questions in the Google Form (see AnalyticsConfig.cs).
COLUMNS = ["session", "build", "level", "round", "winner", "race_time", "lead_changes",
           "finish_gap", "deaths", "powerups_used", "powerup_hits", "levers", "stomps"]

# Chart look
SIZE = (6, 3.6)
WIDE_SIZE = (6.5, 3.6)
LEAD_COLOR = "#4C9BE8"
GAP_COLOR = "#F28E2B"
HIT_COLOR = "#59A14F"
HAZARD_COLORS = {"SPIKED": "#BAB0AC", "TOASTED": "#E15759", "GROUND UP": "#76B7B2", "TRAPPED": "#B07AA1"}
OTHER_COLOR = "#999"

# Power-ups you don't aim at the rival, so they have no hit rate
NOT_AIMED = ("Shield", "SuperJump", "Rocket", "Ghost")

# Rows in a level (LevelBuilder.Rows). A death key looks like "row3:SPIKED".
ROWS = 6
ROW_NAMES = [f"row{i}" for i in range(1, ROWS + 1)]

# Columns that hold a plain number. A bad cell in one of these ("n/a", a typo) is ignored.
NUMBER_COLUMNS = ["level", "round", "race_time", "lead_changes", "finish_gap", "levers", "stomps"]


def parse_counts(cell: object) -> dict[str, int]:
    """'Fist=2;Ghost=1' -> {'Fist': 2, 'Ghost': 1}
    A blank cell gives {}. A malformed pair (no '=', or a count that isn't a number)
    is skipped, so one bad row can't stop every chart."""
    out = {}
    if isinstance(cell, str):
        for part in cell.split(";"):
            key, sep, value = part.partition("=")
            key = key.strip()
            if not sep or not key:
                continue
            try:
                count = int(float(value))
            except (ValueError, OverflowError):
                continue
            out[key] = out.get(key, 0) + count
    return out


def split_death_key(key: str) -> Optional[tuple[str, str]]:
    """'row3:SPIKED' -> ('row3', 'SPIKED'). None if the key isn't a row 1-6 plus a hazard."""
    row, sep, hazard = key.partition(":")
    row, hazard = row.strip(), hazard.strip()
    if not sep or not hazard or row not in ROW_NAMES:
        return None
    return row, hazard


def load(args: argparse.Namespace) -> pd.DataFrame:
    """Race rows from --mock, --csv, or the published sheet, in that order."""
    if args.mock:
        return mock_data()
    src = args.csv or SHEET_CSV_URL
    if not src:
        raise SystemExit("No data: set SHEET_CSV_URL, or pass --csv FILE, or --mock")
    try:
        df = pd.read_csv(src)
    except pd.errors.EmptyDataError:
        raise SystemExit(f"{src} is completely empty (not even a header row). Has any race been sent yet?")
    df.columns = [c.strip().lower() for c in df.columns]
    return df


def clean(df: pd.DataFrame) -> pd.DataFrame:
    """Make the sheet safe to chart. Stops with a clear message if a column is missing;
    otherwise the number columns become numbers, and a bad cell becomes NaN so only the
    chart that needs it skips it."""
    missing = [c for c in COLUMNS if c not in df.columns]
    if missing:
        raise SystemExit("The sheet is missing columns: " + ", ".join(missing) +
                         ". Compare the form question names with COLUMNS in this file.")
    df = df.copy()
    for col in NUMBER_COLUMNS:
        df[col] = pd.to_numeric(df[col], errors="coerce")
    df[NUMBER_COLUMNS] = df[NUMBER_COLUMNS].replace([float("inf"), float("-inf")], float("nan"))
    return df


def mock_data(n: int = 40) -> pd.DataFrame:
    """Plausible fake races, only used to sketch the charts in the GDD."""
    random.seed(7)
    rows = []
    types = ["Fist", "FreezeRay", "GravityBomb", "Reverse", "BrickWall", "Swap", "Banana", "FakeBox"]
    hit_rate = {"Fist": .45, "FreezeRay": .35, "GravityBomb": .55, "Reverse": .5, "BrickWall": .7,
                "Swap": .9, "Banana": .3, "FakeBox": .4}
    hazards = ["SPIKED", "TOASTED", "GROUND UP", "TRAPPED"]
    for i in range(n):
        level = i % 3 + 1
        used, hits = {}, {}
        for t in random.sample(types, 4):
            u = random.randint(1, 3)
            used[t] = u
            hits[t] = sum(random.random() < hit_rate[t] for _ in range(u))
        deaths = {}
        for _ in range(random.randint(1, 6)):
            k = f"row{random.choice([1, 2, 3, 3, 4, 5, 5, 6])}:{random.choice(hazards)}"
            deaths[k] = deaths.get(k, 0) + 1
        rows.append({
            "session": f"s{i // 3}", "build": "alpha", "level": level, "round": level,
            "winner": random.choice(["RED", "YELLOW"]), "race_time": round(random.uniform(25, 60), 1),
            "lead_changes": max(0, int(random.gauss(3.2, 1.6))),
            "finish_gap": round(abs(random.gauss(0.6, 0.5)), 2),
            "deaths": ";".join(f"{k}={v}" for k, v in deaths.items()),
            "powerups_used": ";".join(f"{k}={v}" for k, v in used.items()),
            "powerup_hits": ";".join(f"{k}={v}" for k, v in hits.items()),
            "levers": random.randint(0, 4), "stomps": random.randint(0, 2),
        })
    return pd.DataFrame(rows, columns=COLUMNS)


def save(fig: plt.Figure, name: str, mock: bool) -> None:
    """Write the chart to Analytics/charts/, stamped as a mockup if the data is fake."""
    if mock:
        fig.text(0.99, 0.01, "MOCKUP - made-up data", ha="right", va="bottom", fontsize=8, color=OTHER_COLOR)
    fig.tight_layout()
    path = os.path.join(OUT, name)
    fig.savefig(path, dpi=150)
    plt.close(fig)
    print("wrote", path)


def chart_lead_changes(df: pd.DataFrame, mock: bool) -> None:
    """Metric 1: how many races had 0, 1, 2... lead changes.
    Hypothesis: races average at least 3."""
    fig, ax = plt.subplots(figsize=SIZE)
    counts = df["lead_changes"].dropna().astype(int).value_counts().sort_index()
    ax.bar(counts.index.astype(str), counts.values, color=LEAD_COLOR)
    ax.set_title("Metric 1: Lead changes per race")
    ax.set_xlabel("Times the lead changed hands")
    ax.set_ylabel("Number of races")
    save(fig, "metric1_lead_changes.png", mock)


def chart_finish_gap(df: pd.DataFrame, mock: bool) -> None:
    """Metric 2: average gap (in rows) at the flag, per level.
    Hypothesis: The Gauntlet (level 3) has a larger gap than Lever Lab (level 2)."""
    fig, ax = plt.subplots(figsize=SIZE)
    means = df.dropna(subset=["level", "finish_gap"]).groupby("level")["finish_gap"].mean()
    ax.bar([f"Level {int(l)}" for l in means.index], means.values, color=GAP_COLOR)
    ax.set_title("Metric 2: Average finish gap by level")
    ax.set_ylabel("Gap at the flag (rows)")
    save(fig, "metric2_finish_gap.png", mock)


def chart_powerups(df: pd.DataFrame, mock: bool) -> None:
    """Metric 3: % of uses that hit, per aimed power-up.
    Hypothesis: Gravity Bomb and Brick Wall land 40-70%; Bonk Fist lands less."""
    used, hits = {}, {}
    for _, r in df.iterrows():
        for k, v in parse_counts(r["powerups_used"]).items():
            used[k] = used.get(k, 0) + v
        for k, v in parse_counts(r["powerup_hits"]).items():
            hits[k] = hits.get(k, 0) + v
    names = [k for k in sorted(used) if k not in NOT_AIMED and used[k] > 0]
    rate = [100 * hits.get(k, 0) / used[k] for k in names]
    fig, ax = plt.subplots(figsize=WIDE_SIZE)
    ax.bar(names, rate, color=HIT_COLOR)
    ax.set_title("Metric 3: Hit rate per attack power-up")
    ax.set_ylabel("% of uses that hit")
    ax.set_ylim(0, 100)
    plt.setp(ax.get_xticklabels(), rotation=30, ha="right")
    save(fig, "metric3_powerup_hit_rate.png", mock)


def chart_deaths(df: pd.DataFrame, mock: bool) -> None:
    """Metric 4: deaths per row, stacked by hazard.
    Hypothesis: most deaths are on rows 3 and 5, where saws and traps overlap."""
    table = {}
    for _, r in df.iterrows():
        for k, v in parse_counts(r["deaths"]).items():
            parsed = split_death_key(k)
            if parsed is None:
                continue
            row, hazard = parsed
            table.setdefault(hazard, {})
            table[hazard][row] = table[hazard].get(row, 0) + v
    rows = ROW_NAMES
    fig, ax = plt.subplots(figsize=WIDE_SIZE)
    bottom = [0] * len(rows)
    for hazard, per_row in sorted(table.items()):
        vals = [per_row.get(r, 0) for r in rows]
        ax.bar([r.replace("row", "Row ") for r in rows], vals, bottom=bottom, label=hazard.title(),
               color=HAZARD_COLORS.get(hazard, OTHER_COLOR))
        bottom = [b + v for b, v in zip(bottom, vals)]
    ax.set_title("Metric 4: Deaths by row and hazard")
    ax.set_ylabel("Deaths")
    if table:
        ax.legend(fontsize=8)
    save(fig, "metric4_deaths_by_row.png", mock)


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("--csv")
    ap.add_argument("--mock", action="store_true")
    args = ap.parse_args()
    os.makedirs(OUT, exist_ok=True)
    df = clean(load(args))
    if df.empty:
        raise SystemExit("The sheet has no races yet, so there is nothing to chart. "
                         "The existing charts were left as they are.")
    print(f"{len(df)} races")
    chart_lead_changes(df, args.mock)
    chart_finish_gap(df, args.mock)
    chart_powerups(df, args.mock)
    chart_deaths(df, args.mock)


if __name__ == "__main__":
    main()
