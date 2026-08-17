# Quality evidence

This directory holds small, reviewable evidence indexes for the Lion of the Seas build.
Large raw captures and device-local diagnostics remain local and are never committed.

## Naming

- Use lowercase kebab-case for folders and Markdown records.
- Name a run `YYYY-MM-DD_build-<id>_<device-class>_<scenario>`.
- Every evidence record states the commit, build ID, Unity version, device model/class, OS,
  quality profile, scenario, agent count, median and p95 frame time, minimum FPS, peak
  memory, result, reviewer, and paths to its local captures.
- Keep comparable Primary and Reduced captures under the same run name and battle seed.

## Boundaries

- `Artifacts/Local/` is ignored and owns raw screenshots, profiler traces, logs, temporary
  exports, playtest notes containing participant details, and incomplete evidence.
- `Artifacts/Performance/`, `Artifacts/Quality/`, and `Artifacts/StorePreview/` may contain
  approved text indexes and final deliverables explicitly required by the task plan.
- Never commit signing files, secrets, personal identifiers, device identifiers, caches, or
  generated Unity builds. Reference a local relative path instead of embedding private data.
- Promote an evidence summary out of `Local/` only after the linked build and outcome have
  been reproduced and reviewed.
