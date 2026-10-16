"""
Tests for Analytics/visualize.py. They need no internet and write no files into the repo.

    pip install -r Analytics/requirements-dev.txt
    python3 -m pytest Analytics
"""
import io
import re
import sys
from argparse import Namespace
from pathlib import Path

import pandas as pd
import pytest

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE.parent))
import visualize as v  # noqa: E402

REPO = HERE.parents[1]
SCRIPTS = REPO / "Alpha" / "Assets" / "Scripts"

CHARTS = [v.chart_lead_changes, v.chart_finish_gap, v.chart_powerups, v.chart_deaths]


@pytest.fixture(autouse=True)
def charts_in_tmp(tmp_path, monkeypatch):
    """Charts go to a temp folder, never into Analytics/charts/."""
    monkeypatch.setattr(v, "OUT", str(tmp_path))
    return tmp_path


def sheet(*rows: dict) -> pd.DataFrame:
    """Race rows as they come out of the published sheet: written to CSV and read back."""
    base = dict(session="s1", build="alpha", level=1, round=1, winner="RED", race_time=30.0,
                lead_changes=3, finish_gap=0.5, deaths="row3:SPIKED=1", powerups_used="Fist=1",
                powerup_hits="Fist=1", levers=0, stomps=0)
    frame = pd.DataFrame([{**base, **r} for r in (rows or ({},))])
    return pd.read_csv(io.StringIO(frame.to_csv(index=False)))


# ---------------- parse_counts ----------------

def test_parse_counts_reads_the_formats_unity_sends():
    # Examples from the comment at the top of RaceAnalytics.cs
    assert v.parse_counts("Fist=2;GravityBomb=1") == {"Fist": 2, "GravityBomb": 1}
    assert v.parse_counts("row3:SPIKED=2;row5:TOASTED=1") == {"row3:SPIKED": 2, "row5:TOASTED": 1}


@pytest.mark.parametrize("cell", ["", None, float("nan"), 7])
def test_parse_counts_blank_cells_are_empty(cell):
    assert v.parse_counts(cell) == {}


def test_parse_counts_skips_malformed_pairs_and_keeps_the_good_ones():
    cell = "Fist=abc;Ghost=1;;=3;oops;Banana=nan;Rocket=inf;Shield=2"
    assert v.parse_counts(cell) == {"Ghost": 1, "Shield": 2}


def test_parse_counts_adds_repeated_keys_and_ignores_spaces():
    assert v.parse_counts("Fist=1; Fist = 2.0") == {"Fist": 3}


# ---------------- split_death_key ----------------

@pytest.mark.parametrize("key, expected", [
    ("row3:SPIKED", ("row3", "SPIKED")),
    ("row6:GROUND UP", ("row6", "GROUND UP")),
    ("row1:TOASTED", ("row1", "TOASTED")),
])
def test_split_death_key_good(key, expected):
    assert v.split_death_key(key) == expected


@pytest.mark.parametrize("key", ["row3SPIKED", "row3:", ":SPIKED", "row7:SPIKED", "row0:SPIKED", "SPIKED"])
def test_split_death_key_bad_returns_none(key):
    assert v.split_death_key(key) is None


# ---------------- loading and cleaning ----------------

def test_load_empty_file_stops_with_a_message(tmp_path):
    empty = tmp_path / "empty.csv"
    empty.write_text("")
    with pytest.raises(SystemExit, match="completely empty"):
        v.load(Namespace(mock=False, csv=str(empty)))


def test_clean_names_the_missing_columns():
    with pytest.raises(SystemExit, match="lead_changes.*deaths|deaths.*lead_changes"):
        v.clean(pd.DataFrame({"level": [1]}))


def test_clean_turns_bad_numbers_into_nan_and_keeps_the_row():
    df = v.clean(sheet({"lead_changes": "x", "finish_gap": "n/a"}, {}))
    assert len(df) == 2
    assert pd.isna(df.loc[0, "lead_changes"]) and pd.isna(df.loc[0, "finish_gap"])
    assert df.loc[1, "lead_changes"] == 3


def test_main_with_no_races_stops_and_leaves_old_charts_alone(charts_in_tmp, monkeypatch):
    header_only = charts_in_tmp / "header_only.csv"
    header_only.write_text(",".join(v.COLUMNS) + "\n")
    monkeypatch.setattr(sys, "argv", ["visualize.py", "--csv", str(header_only)])
    with pytest.raises(SystemExit, match="no races yet"):
        v.main()
    assert not list(charts_in_tmp.glob("*.png"))


# ---------------- charts ----------------

@pytest.mark.parametrize("chart", CHARTS)
def test_every_chart_draws_from_the_mock_data(chart, charts_in_tmp):
    chart(v.clean(v.mock_data()), True)
    assert len(list(charts_in_tmp.glob("*.png"))) == 1


@pytest.mark.parametrize("chart", CHARTS)
def test_every_chart_survives_bad_cells(chart, charts_in_tmp):
    bad = sheet(
        {"lead_changes": "x", "finish_gap": "y", "level": "n/a"},
        {"deaths": "row3SPIKED=2;row9:TOASTED=1;row2:SPIKED=abc", "powerups_used": "Fist=0;Ghost=x"},
        {"deaths": "", "powerups_used": "", "powerup_hits": "Fist=4"},
        {"level": 2},
    )
    chart(v.clean(bad), False)
    assert len(list(charts_in_tmp.glob("*.png"))) == 1


@pytest.mark.parametrize("chart", CHARTS)
def test_every_chart_survives_inf_in_number_columns(chart, charts_in_tmp):
    bad = sheet({"lead_changes": "inf", "finish_gap": "-inf", "level": "inf"}, {})
    chart(v.clean(bad), False)
    assert len(list(charts_in_tmp.glob("*.png"))) == 1


def test_a_power_up_with_zero_uses_does_not_crash_the_hit_rate_chart(charts_in_tmp):
    df = v.clean(sheet({"powerups_used": "Fist=0", "powerup_hits": "Fist=1"}))
    v.chart_powerups(df, False)
    assert len(list(charts_in_tmp.glob("*.png"))) == 1


# ---------------- mock data round trip ----------------

def test_mock_data_has_the_sheets_columns():
    df = v.mock_data()
    assert list(df.columns) == v.COLUMNS
    assert len(df) == 40


def test_mock_data_survives_the_csv_round_trip():
    """The sheet is a CSV, so the same counts must come out after a write and a read."""
    mock = v.mock_data()
    back = v.clean(pd.read_csv(io.StringIO(mock.to_csv(index=False))))
    for column in ("deaths", "powerups_used", "powerup_hits"):
        assert [v.parse_counts(c) for c in back[column]] == [v.parse_counts(c) for c in mock[column]]
    assert back["lead_changes"].tolist() == mock["lead_changes"].tolist()


def test_mock_hits_never_exceed_uses_and_death_keys_are_valid():
    for _, r in v.mock_data().iterrows():
        used, hits = v.parse_counts(r["powerups_used"]), v.parse_counts(r["powerup_hits"])
        assert all(hits[k] <= used[k] for k in hits)
        assert all(v.split_death_key(k) for k in v.parse_counts(r["deaths"]))


# ---------------- the script and the Unity code agree on names ----------------

def powerup_names() -> set:
    src = (SCRIPTS / "PowerUps" / "PowerUps.cs").read_text(encoding="utf-8-sig")
    src = re.sub(r"/\*.*?\*/", "", src, flags=re.S)
    src = re.sub(r"//[^\n]*", "", src)
    body = re.search(r"enum\s+PowerUpType\b[^{]*\{(.*?)\}", src, re.S).group(1)
    return {m.group(1) for part in body.split(",") if (m := re.match(r"\s*(\w+)", part))}


def test_power_up_names_match_the_unity_enum():
    names = powerup_names()
    assert set(v.NOT_AIMED) <= names
    for _, r in v.mock_data().iterrows():
        assert set(v.parse_counts(r["powerups_used"])) <= names


def test_every_hazard_word_in_unity_has_a_chart_colour():
    words = set()
    for cs in SCRIPTS.rglob("*.cs"):
        if cs.name == "Hazard.cs":  # its "OUCH!" is only a default; every hazard sets its own word
            continue
        words |= set(re.findall(r'\.word\s*=\s*"([A-Z ]+)!"', cs.read_text(encoding="utf-8-sig")))
    assert words, "found no hazard words: has the C# changed shape?"
    assert words <= set(v.HAZARD_COLORS)