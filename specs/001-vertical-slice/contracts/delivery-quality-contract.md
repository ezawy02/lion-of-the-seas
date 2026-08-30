# Delivery Quality Contract

## Performance gates

### Primary profile

- Reference class: Snapdragon 778G/Exynos 1380 equivalent, 6 GB RAM.
- Target: 60 fps with 300 visible ordinary agents plus one boss.
- Capture points: multiplier burst, approach, landing, peak melee, boss armor break,
  destruction, and reward.
- No sustained frame-time excursion above the 60 fps budget for more than five consecutive
  frames during normal battle after warm-up.

### Floor profile

- Reference class: Snapdragon 680/Helio G85 equivalent, 4 GB RAM.
- Target: at least 30 fps in the 500-agent stress scene with Reduced quality active.
- Reduced quality may lower shadows, particles, water detail, LOD distance, and displayed
  visual agents, but cannot change logical count, gate result, damage, or reward.
- Thermal testing uses a repeated 10-minute loop after the functional slice is complete.

### Evidence

Each Performance Gate records build ID, commit, physical device model, OS, profile, scenario,
agent count, median and p95 frame time, minimum fps, peak memory, and capture paths.

## Maintainability gates

- New or changed authored source limit: at most 1,000 physical lines per file.
- Absolute ceiling for every authored source file: at most 1,500 physical lines.
- Normal target: 500 lines or fewer.
- Generated files, package cache, and vendor code are excluded and reported separately.
- A legacy file from 1,001 through 1,500 lines is frozen except for a behavior-preserving
  reduction or split and cannot receive a feature, fix, or new responsibility.
- A file above 1,500 lines blocks all work except an immediate split, as well as
  merge and release.
- Every contributor MUST read `AGENTS.md` and the active implementation plan before editing.
- New modules expose a narrow purpose and do not depend on Presentation from deterministic
  Core or Crowd assemblies.

The repository check reports path, category, physical line count, threshold, and result.

## Test gates

- EditMode tests pass for arithmetic, validation, persistence, rewards, and deterministic
  domain behavior.
- PlayMode tests pass for each direct-launch user story, lifecycle, landing, boss, retry,
  pause/resume, and quality-outcome parity.
- Performance test scenes produce current evidence for both device classes.
- Manual visual review passes the Art Quality Contract in both quality profiles.

## Store-truth gates

- Every final preview shot has a `StoreMoment` record.
- The record points to a production level, clean-save reach time under ten minutes, build ID,
  and uninterrupted source capture.
- Camera dramatization is allowed only when arithmetic, choices, units, hazards, boss phases,
  rewards, and outcome remain unchanged.
- A missing, prototype-only, or behaviorally different moment blocks publication.

## License gates

- Every third-party source has URL, author, license, commercial-use confirmation,
  modification permission, change note, and repository paths.
- GPL or unclear copyleft code cannot enter the closed-source game without a specific legal
  and distribution decision.
- CC0 and MIT materials are preferred for prototypes, but their provenance is still recorded.
- Ripped store assets, music, video frames, logos, characters, and unlicensed imitations are
  prohibited.

## Private repository boundary

- The authorized remote is one private GitHub repository dedicated to this project.
- No files outside `/Users/apple/Desktop/أسد البحار Lion of the Seas` may be
  uploaded under this authorization.
- Before remote transfer, review the exact tracked file list and scan for credentials,
  tokens, personal logs, caches, build output, and unrelated content.
- Remote visibility is verified as private after creation and after the first push.
