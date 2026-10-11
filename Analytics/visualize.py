"""
BONKERS! analytics -> charts.

Every finished race sends one row to our Google Form (see Assets/Scripts/Analytics/).
The form saves rows into a Google Sheet. This script reads that sheet and draws one
chart per metric into Analytics/charts/.

Usage:
    python3 Analytics/visualize.py                      # read the sheet (SHEET_CSV_URL below)
    python3 Analytics/visualize.py --csv results.csv    # read a downloaded CSV instead
    python3 Analytics/visualize.py --mock               # made-up data, for GDD mockups only

To get SHEET_CSV_URL: in the response sheet, File -> Share -> Publish to web ->
pick the sheet, format "Comma-separated values (.csv)" -> Publish, and paste the link.

Needs: pip install pandas matplotlib
"""
import argparse
import os
import random

import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
import pandas as pd

SHEET_CSV_URL = ""  # paste the published CSV link here

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(HERE, "charts")

COLUMNS = ["session", "build", "level", "round", "winner", "race_time", "lead_changes",
           "finish_gap", "deaths", "powerups_used", "powerup_hits", "levers", "stomps"]


def parse_counts(cell):
    """'Fist=2;Ghost=1' -> {'Fist': 2, 'Ghost': 1}"""
    out = {}
    if isinstance(cell, str):
        for part in cell.split(";"):
            if "=" in part:
                k, v = part.split("=", 1)
                out[k.strip()] = out.get(k.strip(), 0) + int(float(v))
    return out


def load(args):
    if args.mock:
        return mock_data()
    src = args.csv or SHEET_CSV_URL
    if not src:
        raise SystemExit("No data: set SHEET_CSV_URL, or pass --csv FILE, or --mock")
    df = pd.read_csv(src)
    df.columns = [c.strip().lower() for c in df.columns]
    return df


def mock_data(n=40):
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


def save(fig, name, mock):
    if mock:
        fig.text(0.99, 0.01, "MOCKUP - made-up data", ha="right", va="bottom", fontsize=8, color="#999")
    fig.tight_layout()
    path = os.path.join(OUT, name)
    fig.savefig(path, dpi=150)
    plt.close(fig)
    print("wrote", path)


def chart_lead_changes(df, mock):
    fig, ax = plt.subplots(figsize=(6, 3.6))
    counts = df["lead_changes"].astype(int).value_counts().sort_index()
    ax.bar(counts.index.astype(str), counts.values, color="#4C9BE8")
    ax.set_title("Metric 1: Lead changes per race")
    ax.set_xlabel("Times the lead changed hands")
    ax.set_ylabel("Number of races")
    save(fig, "metric1_lead_changes.png", mock)


def chart_finish_gap(df, mock):
    fig, ax = plt.subplots(figsize=(6, 3.6))
    means = df.groupby("level")["finish_gap"].mean()
    ax.bar([f"Level {int(l)}" for l in means.index], means.values, color="#F28E2B")
    ax.set_title("Metric 2: Average finish gap by level")
    ax.set_ylabel("Gap at the flag (rows)")
    save(fig, "metric2_finish_gap.png", mock)


def chart_powerups(df, mock):
    used, hits = {}, {}
    for _, r in df.iterrows():
        for k, v in parse_counts(r["powerups_used"]).items():
            used[k] = used.get(k, 0) + v
        for k, v in parse_counts(r["powerup_hits"]).items():
            hits[k] = hits.get(k, 0) + v
    # only things you aim at someone have a hit rate
    names = [k for k in sorted(used) if k not in ("Shield", "SuperJump", "Rocket", "Ghost")]
    rate = [100 * hits.get(k, 0) / used[k] for k in names]
    fig, ax = plt.subplots(figsize=(6.5, 3.6))
    ax.bar(names, rate, color="#59A14F")
    ax.set_title("Metric 3: Hit rate per attack power-up")
    ax.set_ylabel("% of uses that hit")
    ax.set_ylim(0, 100)
    plt.setp(ax.get_xticklabels(), rotation=30, ha="right")
    save(fig, "metric3_powerup_hit_rate.png", mock)


def chart_deaths(df, mock):
    table = {}
    for _, r in df.iterrows():
        for k, v in parse_counts(r["deaths"]).items():
            row, hazard = k.split(":", 1)
            table.setdefault(hazard, {})
            table[hazard][row] = table[hazard].get(row, 0) + v
    rows = [f"row{i}" for i in range(1, 7)]
    fig, ax = plt.subplots(figsize=(6.5, 3.6))
    bottom = [0] * len(rows)
    colors = {"SPIKED": "#BAB0AC", "TOASTED": "#E15759", "GROUND UP": "#76B7B2", "TRAPPED": "#B07AA1"}
    for hazard, per_row in sorted(table.items()):
        vals = [per_row.get(r, 0) for r in rows]
        ax.bar([r.replace("row", "Row ") for r in rows], vals, bottom=bottom, label=hazard.title(),
               color=colors.get(hazard, "#999"))
        bottom = [b + v for b, v in zip(bottom, vals)]
    ax.set_title("Metric 4: Deaths by row and hazard")
    ax.set_ylabel("Deaths")
    ax.legend(fontsize=8)
    save(fig, "metric4_deaths_by_row.png", mock)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--csv")
    ap.add_argument("--mock", action="store_true")
    args = ap.parse_args()
    os.makedirs(OUT, exist_ok=True)
    df = load(args)
    print(f"{len(df)} races")
    chart_lead_changes(df, args.mock)
    chart_finish_gap(df, args.mock)
    chart_powerups(df, args.mock)
    chart_deaths(df, args.mock)


if __name__ == "__main__":
    main()
