# BONKERS!

A two-player, one-keyboard platform race where whoever falls behind gets the weapons
(attack power-ups and trap levers) while the leader can only defend, so every race stays
close until the flag.

Made in **Unity 6 (6000.3)**. No external assets: every shape, color and sound is made in code.
Art is deliberately minimal until the Gold milestone.

## Builds (one link per milestone, never overwritten)

| Milestone | Play | Document |
|---|---|---|
| Paired prototype (10/02) | https://samarth-borade.github.io/bonkers/ | [`Documentation/paired/`](Documentation/paired/) |
| Alpha Progress Check (10/16) | https://samarth-borade.github.io/bonkers/alpha-check/ | [`Documentation/alpha/`](Documentation/alpha/) |
| Alpha (10/23) | https://samarth-borade.github.io/bonkers/alpha/ | [`Documentation/alpha/`](Documentation/alpha/) |
| Beta | https://samarth-borade.github.io/bonkers/beta/ | [`Documentation/beta/`](Documentation/beta/) |
| Gold | https://samarth-borade.github.io/bonkers/gold/ | [`Documentation/gold/`](Documentation/gold/) |

See [VERSIONS.md](VERSIONS.md) for what changed in each milestone and how versions are kept.

## Controls

| Action | Red | Yellow |
|---|---|---|
| Move | A / D | Left / Right |
| Jump (tap again in water to swim) | W | Up |
| Use power-up | S | Down |
| Shove / Stomp | run into your rival / land on their head | same |
| Pull a lever (chaser only) | jump onto its ledge | same |

**Space** start / next race · **R** restart this race · **Esc** back to the menu

## How it plays

- **Best of 3:** first to win 2 races wins the match. Each race is on a new level: Classic Climb, Lever Lab, The Gauntlet.
- Run along a row, then jump up through the gap at its end. Six rows, flag at the top.
- **Lava, spikes, saws, traps**: back to the start of your row. **Water**: slow and floaty.
- A progress bar and a **LEADING / CHASING** tag always show who is ahead.

### The twist: the player behind gets the weapons

- **Place-based ? boxes.** Every box has two fixed prizes. Chasing (or level): an attack or catch-up power. Leading: defence only. No dice rolls.
- **Chaser-only levers.** A lever on a small ledge springs its trap (same letter and color) further up the course: a gate to jump or a spike strip. The leader can't use levers.
- **Every attack can be dodged.** Punches fly along the row; bombs and walls warn first. After a BONK you're immune for 1.2 s.

| Power-up | Who gets it | What it does |
|---|---|---|
| Bonk Fist | Chaser | Punch flies forward: BONK + slow |
| Freeze Ray | Chaser | Ice ball: opponent frozen |
| Gravity Bomb | Chaser | Warning beam, then a bomb drops: heavy, tiny jumps |
| Brain Scramble | Chaser | Warning beam, then a bomb drops: left/right flipped |
| Brick Wall | Chaser | A wall pops up in front of the leader |
| Super Jump | Chaser | Jump up through the floors |
| Rocket Shoes | Chaser | Run super fast |
| Ghost | Chaser | Hazards can't hurt you for 3 s |
| Swaparoo | Chaser (last row) | Swap places with your opponent |
| Bubble Shield | Leader | Blocks the next attack |
| Banana Peel | Leader | Drop behind you: whoever steps on it spins out |
| Fake Box | Leader | A ? box that BONKs the chaser who grabs it |

## Run it locally

1. Unity Hub → **Add** → pick this folder (Unity 6000.3.x).
2. It opens `Assets/Scenes/Main.unity` automatically. Press **Play**.
3. To see a level without playing: **Bonkers → Show Level in Scene View**.

## Build

- **Bonkers → Build WebGL → Alpha Progress Check / Alpha / Beta / Gold** writes to `docs/<milestone>/`.
  It asks before overwriting, because each submission needs its own link.
- `docs/` itself is the paired prototype as submitted. Don't rebuild it.
- GitHub Pages: repo **Settings → Pages**, branch `main`, folder `/docs`.

## Analytics

Each finished race sends one row (lead changes, finish gap, power-up hits, deaths by row) to a
Google Form. Set it up in `Assets/Scripts/Analytics/AnalyticsConfig.cs`, then draw the charts with:

```
python3 Analytics/visualize.py --csv responses.csv   # or set SHEET_CSV_URL in the script
python3 Analytics/visualize.py --mock                # made-up data, for GDD mockups
```

## Project structure

```
Assets/
  Scenes/Main.unity          a Camera + GameManager; the level is built by code at Play
  Editor/BonkersSetup.cs     "Bonkers" menu: builds per milestone, level preview
  Scripts/
    Core/                    GameManager (match, races), CameraRig, Palette, Shapes, Sfx,
                             DevScreenshots + DevBot (test helpers, only run with a command-line flag)
    Level/                   LevelBuilder (walls, floors, helpers), LevelLayouts (the 3 levels),
                             Lever, Trap, Flag
    Player/                  PlayerController (move, jump, shove, stomp, status effects)
    PowerUps/                PowerUps (what each does), PowerUpBox, Projectile, DropStrike, Banana, FakeBox
    Hazards/                 Hazard (lava / spikes), Water, Grinder (saws)
    Analytics/               RaceAnalytics (counts + sends), AnalyticsConfig (form IDs)
    Effects/                 FX (the "BONK!" text and other pop-up words)
    UI/                      Hud (panels, progress bar, title + win screens)
Analytics/                   visualize.py + charts/
docs/                        WebGL builds served by GitHub Pages (one folder per milestone)
Documentation/               paired/, alpha/, beta/, gold/  (GDD, diagrams, screenshots)
```

## Team

| Name | GitHub |
|---|---|
| Samarth Borade | Samarth-Borade |
| Harsh Mangukiya | harshmangukiya-usc |
| Prachi Dave | prachicode-design |
| Aashwin Singh | Ash-Si |
| Shivam Bhosle | 007-Shivam |
