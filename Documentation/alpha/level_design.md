# BONKERS! Level Design Reference (Alpha)

All three levels are built in code, in
[`Alpha/Assets/Scripts/Level/LevelLayouts.cs`](../../Alpha/Assets/Scripts/Level/LevelLayouts.cs),
one method per level. This page lists what is on every row, so you can talk about a level
without opening the code. If you change a layout, update this page too.

## How to read the tables

- **Rows** are numbered 1-6 from the bottom, as in the HUD and the GDD. In code they are 0-5
  (`Lava(0, ...)` is on Row 1).
- **x** runs from -15.5 (left wall) to +15.5 (right wall); 0 is the middle of the screen.
- **Direction:** Rows 1, 3 and 5 run right (→); Rows 2, 4 and 6 run left (←). Each row ends in a gap
  you jump up through. The flag is at x = -14 on Row 6.
- **Width** is in the same units as x. A player is 0.6 wide and 0.7 tall; a row is 3 high.
- **Box: chaser / leader.** The player who is behind gets the first prize, the leader gets the second.

| Hazard | In code | Effect | Death word |
|---|---|---|---|
| Lava | `Lava(row, x, width)` | Back to the start of your row | TOASTED |
| Spikes | `Spikes(row, x, width)` | Back to the start of your row | SPIKED |
| Saw | `Saw(row, fromX, toX, height, speed)` | Rolls back and forth between fromX and toX; back to row start | GROUND UP |
| Water | `WaterPool(row, x, width)` | Slow and floaty, tap jump to swim. Not deadly | – |
| Gate trap | `GateTrap(row, x, tag)` | Pops up when its lever is pulled: a hurdle to jump | – |
| Spike trap | `SpikeTrap(row, x, width, tag)` | Pops up when its lever is pulled: deadly strip | TRAPPED |

## Design rules

These are from the header of `LevelLayouts.cs`, plus what earlier test runs taught us.

1. **Keep the ends of each row clear (|x| > 10).** That's where players land and jump up to the next row.
2. **Hazards are no wider than 2**, so they can always be jumped.
3. **Never put a lever ledge above or right next to a hazard.** Players bump their head on the
   ledge mid-jump and fall in. This happened in our Week 8 test runs.
4. **A lever and its trap share a letter and a color**, and the trap is always further up the course.
5. **Only the chaser can pull a lever** (8 s cooldown), so levers are a catch-up tool, never a leader's weapon.

### Lever and trap timings

| | Value | In code |
|---|---|---|
| Lever cooldown | 8 s | `Lever.Cooldown` |
| Trap warning (outline flashes) | 0.6 s | `Trap.WarnTime` |
| Trap active | 3.5 s | `Trap.ActiveTime` |
| Gate size | 0.5 wide, 1.2 high | `Trap.GateWidth`, `Trap.GateHeight` |
| Lever ledge | 1.4 wide, underside 1.0 above the floor | `Lever.LedgeWidth`, `Lever.LedgeHeight` |

A trap springs 0.6 s after the pull, so a leader right on top of it still gets a moment to react.

## At a glance

| | Classic Climb | Lever Lab | The Gauntlet |
|---|---|---|---|
| Deadly hazards (lava, spikes, saws) | 13 | 7 | 14 |
| Water pools | 2 | 1 | 1 |
| ? boxes | 11 | 7 | 10 |
| Levers | 1 (A) | 5 (A-E) | 5 (A-E) |
| Idea | The paired-prototype layout | Fewer hazards, a lever on every row | Everything at once |

---

## Level 1: Classic Climb

The paired-prototype layout, plus one lever.

| Row | Dir | Hazards | Boxes (chaser / leader) | Levers and traps |
|---|---|---|---|---|
| 6 | ← | Lava x 4 (w 2), Spikes x -2 (w 1), Saw -9 ↔ -6 | x 8.5 Swaparoo / Bubble Shield | **FLAG** at x -14 |
| 5 | → | Spikes x -5 (w 1), Lava x 1 (w 1.8), Saw 5.5 ↔ 8.5 | x -8.5 Freeze Ray / Bubble Shield; x 4 Rocket Shoes / Banana Peel | |
| 4 | ← | Saw 1 ↔ 7, Water x -5.5 (w 5) | x 8.5 Super Jump / Bubble Shield; x -1.5 Brain Scramble / Banana Peel | |
| 3 | → | Lava x -5 (w 2), Spikes x 1.5 (w 0.8), Spikes x 6.5 (w 0.8) | x -1.5 Gravity Bomb / Bubble Shield; x 9 Bonk Fist / Banana Peel | Trap **A**: gate at x 4 |
| 2 | ← | Water x 2 (w 6), Saw -7.5 ↔ -3 | x 7.5 Freeze Ray / Banana Peel; x -9.5 Super Jump / Bubble Shield | Lever **A** at x 9.5 → gate on Row 3 |
| 1 | → | Spikes x -5 (w 1), Lava x 3 (w 2) | x -1 Bonk Fist / Bubble Shield; x 8 Rocket Shoes / Banana Peel | |

## Level 2: Lever Lab

Fewer hazards, and a lever on every row from 1 to 5. Each lever springs a trap on the row above.

| Row | Dir | Hazards | Boxes (chaser / leader) | Levers and traps |
|---|---|---|---|---|
| 6 | ← | Lava x 5 (w 1.5), Saw -9 ↔ -6 | x 8.5 Swaparoo / Bubble Shield | Trap **E**: spikes at x 1 (w 2). **FLAG** at x -14 |
| 5 | → | Spikes x -5 (w 1), Saw 3.5 ↔ 6.5 | x -8 Gravity Bomb / Fake Box | Lever **E** at x 9 → spikes on Row 6. Trap **D**: gate at x 0 |
| 4 | ← | Water x 5 (w 4) | x 8.5 Freeze Ray / Banana Peel; x -5.5 Rocket Shoes / Bubble Shield | Lever **D** at x -8.5 → gate on Row 5. Trap **C**: spikes at x -2 (w 2) |
| 3 | → | Lava x -4 (w 1.5) | x 6 Brick Wall / Bubble Shield | Lever **C** at x 8.5 → spikes on Row 4. Trap **B**: gate at x 2 |
| 2 | ← | Saw 3 ↔ 7 | x -5 Ghost / Fake Box | Lever **B** at x -8.5 → gate on Row 3. Trap **A**: spikes at x 0 (w 2) |
| 1 | → | Lava x 3 (w 1.5) | x -2 Bonk Fist / Bubble Shield | Lever **A** at x 7.5 → spikes on Row 2 |

## Level 3: The Gauntlet

Everything at once: the most hazards, a lever on Rows 1-5, and Fake Boxes for the leader.

| Row | Dir | Hazards | Boxes (chaser / leader) | Levers and traps |
|---|---|---|---|---|
| 6 | ← | Spikes x 4 (w 1), Lava x -4 (w 1.5), Saw -10 ↔ -7 | x 8.5 Swaparoo / Bubble Shield | Trap **E**: gate at x 0. **FLAG** at x -14 |
| 5 | → | Lava x -4 (w 1.5), Spikes x 8 (w 1) | x -8 Super Jump / Bubble Shield; x 0 Bonk Fist / Banana Peel | Lever **E** at x -9.5 → gate on Row 6. Trap **D**: spikes at x 3 (w 2) |
| 4 | ← | Lava x 3 (w 1.5), Saw -9 ↔ -6.5 | x 7 Brick Wall / Bubble Shield; x -1 Brain Scramble / Fake Box | Lever **D** at x 9.5 → spikes on Row 5. Trap **C**: gate at x -4 |
| 3 | → | Spikes x -4 (w 1), Spikes x 1 (w 1), Saw 7 ↔ 9.5 | x -1.5 Ghost / Banana Peel | Lever **C** at x -9 → gate on Row 4. Trap **B**: gate at x 5 |
| 2 | ← | Water x 4 (w 4), Lava x -4.5 (w 1.5) | x 8 Freeze Ray / Fake Box; x 0 Gravity Bomb / Bubble Shield | Lever **B** at x -9.5 → gate on Row 3. Trap **A**: spikes at x -3 (w 2) |
| 1 | → | Spikes x -6 (w 1), Lava x -1 (w 1.5), Saw 3 ↔ 7 | x 1.5 Bonk Fist / Banana Peel; x 9 Rocket Shoes / Bubble Shield | Lever **A** at x -9.5 → spikes on Row 2 |

---

## Spots to watch in playtests

These spots aren't broken, but they're where a trap lands close to an existing hazard.
Check them first if the deaths chart shows a spike.

- **Classic Climb, Row 3:** gate A (x 4) sits between two spike strips (x 1.5 and x 6.5).
  Jumping the gate might land a player on spikes.
- **The Gauntlet, Row 3:** gate B (x 5) is just before the saw (7 ↔ 9.5), at the end of the row.
- **The Gauntlet, Row 6:** the saw reaches x -10, right at the edge of the clear zone in front of the flag.

## Previewing and testing a level

- **See a level without playing:** in Unity, **Bonkers → Show Level in Scene View**
  (and **Bonkers → Hide Level Preview** to clear it).
- **Start on a specific level:** build with **Bonkers → Build Game (Mac)**, then run it with `-bonkersLevel N`.
- **Check that a level can be finished:** run the build with `-bonkersBot`. A simple bot plays Red through the level
  and logs how many times it died and whether it reached the flag. If the bot can finish, a person can too.

  ```
  Alpha/Builds/Mac/Bonkers.app/Contents/MacOS/Bonkers -bonkersBot -bonkersLevel 3 -logFile -
  ```

  `-logFile -` prints the log to the terminal.

## Which playtest data is about levels

Each race sends one analytics row (see the README's Analytics section). Two of the four metrics are for level design:

| Metric | What it tells us about levels | Hypothesis in the GDD |
|---|---|---|
| **#2 Finish gap by level** | Which level gives the closest finishes | The Gauntlet has a bigger gap than Lever Lab |
| **#4 Deaths by row and hazard** | Which row, and which hazard on it, is too hard | Most deaths are on Rows 3 and 5; spacing hazards out should flatten it |

If one row has far more deaths than the rest, move or narrow its hazards. If trap deaths (TRAPPED) are near zero,
the levers are too weak; if they dominate, they're too strong.

## Ideas for Beta (from the GDD)

- One-way trap doors.
- A lever that reverses a row's conveyor.
- A short tutorial race that introduces one mechanic per row.

Trap doors and conveyors don't exist in the code yet, so they need new hazard scripts, not just layout changes.
