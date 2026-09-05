# أسد البحار: فتوحات المتوسط

**Lion of the Seas: Mediterranean Conquest** is a portrait mobile crowd-action game about
building a fleet, choosing sea gates, landing an army, and breaking coastal strongholds.

## Current status

The repository contains the Unity implementation and an active source remediation of the
three-level vertical slice. `Bootstrap` now launches `Level_01_Playable_Trial`; victory
unlocks the next encounter, and the result screen opens the existing three-slot loadout UI.
Standalone playable shells for Chain Strait and Storm Fortress bind their existing art scenes.

The current changes have source-level checks, not current Unity playthrough or visual approval.
Asset import, all three uninterrupted journeys, exact-revision art approval, Android 300/500-agent
performance evidence, and the store preview remain pending. See
[the remediation record](specs/001-vertical-slice/source-remediation.md) for implemented behavior
and verification limits. Task checkmarks from older revisions are not evidence for this revision.

## Working without opening editors

Use Git LFS when checking out binary assets. The project targets Unity `6000.3.22f1`.
The following check invokes only the installed C# compiler and managed test runtime; it does
not launch Unity or Blender, import assets, or touch another project:

```sh
python3 tools/check-csharp-without-editor.py --unity-resources /path/to/Unity.app/Contents/Resources --run-domain-tests
bash tools/check-source-size.sh
```

The compiler check reports the assemblies it can verify from the provided installation.
It is not a substitute for importing and testing with the pinned project version.

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
- Every contributor reads `AGENTS.md` and the active plan before editing authored code.
- New or changed authored source files stay at or below 1,000 physical lines and normally
  target 500 or fewer. Legacy files above 1,000 are split before behavioral changes;
  1,500 lines is the absolute ceiling.
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
