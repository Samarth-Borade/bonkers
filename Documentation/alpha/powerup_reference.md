# BONKERS! Power-up Tuning Reference (Alpha)

Every number and rule for the 12 power-ups, in one place, so the GDD and the playtest notes can
quote them without opening the code. Most of the numbers are written directly in
[`PowerUps.cs`](../../Alpha/Assets/Scripts/PowerUps/PowerUps.cs) (the `Use()` method), and a few are in the
files named in the tables. If you change a number there, update this page too.
Checked against the code at commit `01ca6fa`.

## How to read the tables

- **Gets it** is who is offered the power-up in a ? box. The player who is **behind** gets the first prize in a
  box and the **leader** gets the second (see "Rules every power-up shares" below).
- **Boxes** is how many of the 28 ? boxes, across all three levels, hold it (see
  [`level_design.md`](level_design.md)). Of the 28 chaser prizes, 16 are attacks (Bonk Fist, Freeze Ray, the two
  bombs, Brick Wall) and 12 are boosts. The 28 leader prizes are Bubble Shield 15, Banana Peel 9 and Fake Box 4.
- Times are in seconds, speeds in units per second, and `×` means a multiplier on normal speed. A player is 0.6 wide,
  a row is 3 high, and the level is 31 wide. Where a table says "jump 1.25×", that is the jump's launch speed, so the
  height changes by about its square.

## Tuning values

| Power-up | Gets it | Boxes | What it does, with the numbers | How to dodge or counter it | Where |
|---|---|---|---|---|---|
| Bonk Fist | Chaser | 5 | A punch flies 15 units/s in the direction you face for up to 2 s. On a hit: BONK (knockback) and slow 2.5 s (0.45× speed). | Jump over it. It stops at the first wall or floor. | `PowerUps.cs`, `Projectile.cs` |
| Freeze Ray | Chaser | 4 | An ice ball flies 12 units/s for up to 2 s. On a hit: BONK, then frozen for 1.8 s (no moving, jumping or power-ups). | Jump over it. | `PowerUps.cs`, `Projectile.cs` |
| Gravity Bomb | Chaser | 3 | A bomb drops on the rival from the roof of their row. Aim 0.5 s (beam follows them and leads their running by 0.45 s), lock 0.45 s (beam turns darker and stops), then it falls at 22 units/s. On a hit: BONK, slow 0.5 s, then **heavy** for 4 s (speed 0.7×, jump 0.6×, gravity 2×). | Leave the beam during the lock, or jump away as it falls. | `PowerUps.cs`, `DropStrike.cs` |
| Brain Scramble | Chaser | 2 | Same bomb and timings as Gravity Bomb. On a hit: BONK, slow 0.5 s, then left and right are flipped for 4 s. | Same as Gravity Bomb. | `PowerUps.cs`, `DropStrike.cs` |
| Brick Wall | Chaser | 2 | A wall (0.5 wide, 1.2 high) appears 3.5 units ahead of the rival in the direction their row runs (kept within x ±10). 0.6 s warning, then solid for 3.5 s, then it disappears. | Jump it. | `PowerUps.cs`, `Trap.cs` |
| Super Jump | Chaser | 3 | For 6 s, your jump is 1.25× stronger and passes up **through** floors. It only reaches the rival's row (one row below if they are on Row 6). Dying ends it. | Nothing to dodge: it only closes the gap. | `PowerUps.cs`, `PlayerController.cs` |
| Rocket Shoes | Chaser | 4 | Run 1.7× faster for 4 s. | Nothing to dodge. | `PowerUps.cs` |
| Ghost | Chaser | 2 | For 3 s lava, spikes, saws and spike traps can't hurt you (you are drawn see-through). BONKs still work on you. | Nothing to dodge. | `PowerUps.cs`, `PlayerController.cs` |
| Swaparoo | Chaser, Row 6 only | 3 | You and the rival instantly swap places and rows. | Have a Bubble Shield up: it cancels the swap. | `PowerUps.cs` |
| Bubble Shield | Leader | 15 | Lasts 10 s or until it blocks one attack. After a block you are immune for 0.4 s. | Not an attack. | `PowerUps.cs`, `PlayerController.cs` |
| Banana Peel | Leader | 9 | A peel drops 0.9 units behind you and stays for 25 s. Whoever steps on it is BONKED, slowed for 1 s and can't move for 1 s. You can't trigger your own for its first 1 s. | Jump over it, or don't run into it. | `PowerUps.cs`, `Banana.cs` |
| Fake Box | Leader | 4 | A fake ? box drops 1.2 units behind you and stays for 20 s. The rival who touches it is BONKED and slowed for 1.5 s. You can never trigger your own. | Know where the real boxes are. | `PowerUps.cs`, `FakeBox.cs` |

## What counts as a "hit" (analytics)

Each finished race sends how many times each power-up was **used** and how many **hit**
(see [`RaceAnalytics.cs`](../../Alpha/Assets/Scripts/Analytics/RaceAnalytics.cs)). Metric 3 in the README
is hits divided by uses. A power-up counts as **used** the moment its key is pressed, even if a shield then
cancels it. "The BONK lands" below means the rival's Bubble Shield doesn't block it and the rival isn't still
immune from an earlier BONK (1.2 s).

| Power-up | It is a hit when | It is not a hit when |
|---|---|---|
| Bonk Fist, Freeze Ray | The punch or ice ball touches the rival and the BONK lands. | It hits a wall or floor, expires, is jumped over, or the BONK doesn't land. |
| Gravity Bomb, Brain Scramble | The falling bomb touches the rival and the BONK lands. | The rival has left the beam by the time it falls, or the BONK doesn't land. |
| Banana Peel | The rival steps on the peel and the slip lands. | Nobody steps on it, the slip doesn't land, or **you** step on it (never counted). |
| Fake Box | The rival touches it and the BONK lands. | Nobody touches it, or the BONK doesn't land. |
| Brick Wall | The wall is placed (see the heads-up below). | The rival's Bubble Shield cancels it. |
| Swaparoo | The swap happens. | The rival's Bubble Shield cancels it. |
| Bubble Shield, Super Jump, Rocket Shoes, Ghost | These are never aimed at the rival, so they have no hit rate. They are left off the Metric 3 chart (`NOT_AIMED` in `visualize.py`). | |

> **Heads-up for the GDD:** Brick Wall and Swaparoo are counted as hits as soon as they take effect, whether or
> not the rival is ever stopped by them. Their hit rate will therefore read close to 100%, and can't be compared with
> the attacks above. The Metric 3 hypothesis that Brick Wall lands 40-70% can't be tested until "hit" means
> something stricter for it, for example "the rival touched the wall".

## Rules every power-up shares

- **Place decides the prize.** A player is **leading** if they are ahead by more than 0.1 rows. Neck and neck, both
  players count as chasing. The prize is decided at the moment of pickup.
- **One at a time.** You can't pick up a power-up while holding one.
- **Box cooldown.** Each player has their own 7 s cooldown on each box, so the leader can't take a box and leave the
  chaser nothing.
- **BONK.** Knocks the player back and slows them (0.45× speed) for the time shown. After a BONK the player is immune
  to further BONKs for 1.2 s, so nobody gets stun-locked.
- **Bubble Shield** blocks any BONK, the Swaparoo and the Brick Wall. A blocked power-up is still used up.
  Lava, spikes, saws and spike traps are not blocked by it.
- **Lever traps** are not power-ups: their timings are in the table in [`level_design.md`](level_design.md).
