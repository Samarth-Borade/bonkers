# Versions

The game grows in one Unity project on `main`. Each milestone is kept three ways:

1. **A build folder** in `docs/` with its own GitHub Pages link. Old folders are never overwritten.
2. **A git tag** on the commit that was submitted (`git checkout <tag>` to see that exact code).
3. **A document folder** in `Documentation/` with that milestone's GDD, diagrams and screenshots.

| Milestone | Due | Build folder | Tag | Rule |
|---|---|---|---|---|
| Paired prototype | 10/02 | `docs/` (root) | `paired-prototype` | done, frozen |
| Alpha Progress Check | 10/16 | `docs/alpha-check/` | `alpha-check` | graybox, new features |
| Alpha | 10/23 | `docs/alpha/` | `alpha` | features + 1 analytics pipeline |
| Beta | TBD | `docs/beta/` | `beta` | more features, tutorialization, no art |
| Gold | TBD | `docs/gold/` | `gold` | **art and polish only, no new features** |

To submit a milestone:
1. **Bonkers → Build WebGL → <milestone>**
2. Commit and push.
3. `git tag <milestone> && git push --tags`

## What changed

### Paired prototype
- One level, six zig-zag rows, place-based ? boxes, 9 power-ups, lava / spikes / saws / water.

### Alpha (in progress)
- Best-of-3 match on three levels: Classic Climb, Lever Lab, The Gauntlet.
- Chaser-only **levers** on ledges that spring **gate** or **spike** traps further up the course.
- New power-ups: **Brick Wall**, **Ghost** (chaser), **Fake Box** (leader). 12 total.
- Race analytics (lead changes, finish gap, hit rates, deaths) sent to a Google Form + chart script.
