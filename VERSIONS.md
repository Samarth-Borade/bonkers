# Versions

The Unity project lives in `Alpha/`. Each submission keeps its own build folder (and link) in `docs/`,
and a git tag on the submitted commit.

| Milestone | Due | Play | Tag |
|---|---|---|---|
| Paired prototype | 10/02 | https://samarth-borade.github.io/bonkers/ | `paired-prototype` |
| Alpha Progress Check | 10/16 | https://samarth-borade.github.io/bonkers/alpha-check/ | `alpha-check` (on submit) |
| Alpha | 10/23 | https://samarth-borade.github.io/bonkers/alpha/ | `alpha` (on submit) |

Build with **Bonkers → Build WebGL → <milestone>** in Unity. Never rebuild a submitted folder.

## What changed

### Paired prototype
- One level, six zig-zag rows, place-based ? boxes, 9 power-ups, lava / spikes / saws / water.

### Alpha
- Best-of-3 match on three levels: Classic Climb, Lever Lab, The Gauntlet.
- Chaser-only **levers** on ledges that spring **gate** or **spike** traps further up the course.
- New power-ups: **Brick Wall**, **Ghost** (chaser), **Fake Box** (leader). 12 total.
- **Super Jump** only reaches the rival's row (one below on the top row) and ends when you die.
- Race analytics (lead changes, finish gap, hit rates, deaths) + chart script.
