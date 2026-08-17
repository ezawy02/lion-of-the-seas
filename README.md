# أسد البحار: فتوحات المتوسط

**Lion of the Seas: Mediterranean Conquest** is a portrait mobile crowd-action game about
building a fleet, choosing sea gates, landing an army, and breaking coastal strongholds.

## Current status

The repository contains the approved Spec Kit foundation for the first playable vertical
slice. Unity implementation has not started yet. The first scope is three short sea-to-land
levels, one small loadout, one reward flow, mobile quality benchmarks, and a truthful
30-second store preview.

## Start here

- [Project constitution](.specify/memory/constitution.md)
- [Vertical-slice specification](specs/001-vertical-slice/spec.md)
- [Implementation plan](specs/001-vertical-slice/plan.md)
- [Research decisions](specs/001-vertical-slice/research.md)
- [Data model](specs/001-vertical-slice/data-model.md)
- [Quickstart](specs/001-vertical-slice/quickstart.md)
- [Gameplay contract](specs/001-vertical-slice/contracts/gameplay-contract.md)
- [Art quality contract](specs/001-vertical-slice/contracts/art-quality-contract.md)
- [Delivery quality contract](specs/001-vertical-slice/contracts/delivery-quality-contract.md)

## Non-negotiable rules

- Premium stylized graphics are a shipping requirement and are reviewed in a mobile build.
- Primary target: 60 fps with 300 visible agents; floor stress target: 30 fps at 500 agents.
- Authored source files stay below 1,500 non-blank lines, receive a split review at 1,000,
  and normally target 500 lines or fewer.
- Store media may show only gameplay and rewards reachable in the shipped build.
- Every third-party asset and dependency needs a compatible license record.
- Open world, online multiplayer, production backend, and a large economy are outside the
  first vertical slice.

Run `bash tools/check-source-size.sh` before publishing code. This checkout also uses the
versioned `.githooks/pre-push` hook to run the same check automatically before every push.

## Repository privacy

This project is intended for one private GitHub repository dedicated only to Lion of the
Seas. No unrelated project files belong in this repository.

## Tracked-file boundary

The tracked boundary is limited to project source, Unity configuration, specifications,
quality evidence that is explicitly intended for review, license records, and authored
art or media. Future binary `.blend`, `.fbx`, texture, audio, and video files use the
patterns in `.gitattributes` and must be stored through Git LFS; Unity-generated or local
machine output is not tracked.

Do not track `Library/`, `Temp/`, `Logs/`, `Build/`, `Builds/`, `UserSettings/`, local
captures, profiling output, credentials, signing material, secrets, caches, or unrelated
files. Before any remote transfer, review the exact `git ls-files` result and confirm that
the destination is the single private repository dedicated to this project.
