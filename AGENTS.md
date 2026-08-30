<!-- SPECKIT START -->
For the current technology, architecture, art pipeline, performance budgets, source-size
limits, and project structure, read `specs/001-vertical-slice/plan.md` before making changes.
<!-- SPECKIT END -->

## Mandatory Agent Preflight and Source-Size Policy

- Every model, agent, or contributor MUST read this `AGENTS.md` file and the active
  implementation plan before making any project change.
- Before editing authored code, run `bash tools/check-source-size.sh` and inspect the
  target file's current physical line count, including blank and comment-only lines.
- New or changed authored code files MUST NOT exceed 1,000 physical lines. Split the
  responsibility before adding code that would cross that limit.
- Existing legacy files between 1,001 and 1,500 physical lines may only be reduced or
  split; they MUST NOT receive features, fixes, or new responsibilities while oversized.
- A file above 1,500 physical lines is an absolute violation and blocks all further
  work on that file except an immediate behavior-preserving split.
- The normal target remains 500 physical lines or fewer per authored file.
- Every implementation task implicitly includes a source-size preflight and post-change
  check, even when the task text does not repeat this rule.
- Generated, package-cache, and third-party vendor files are exempt from counting, but
  agents MUST NOT hand-edit them.

## Sub-agent Cost Policy

- Use `gpt-5.6-luna` first for delegated sub-agent tasks in this project.
- The primary agent must review all delegated output before it is accepted or applied.
- If Luna fails because of reasoning, implementation quality, or uncertainty, retry once
  with `gpt-5.6-terra` at low reasoning effort before the primary agent takes over.
- If the failure is clearly an unavailable terminal, missing tool, permission boundary, or
  other environment issue that a model change cannot fix, skip the retry and let the primary
  agent take over directly.
- If Terra also fails, stalls, or remains uncertain, the primary agent must take over.
- Delegated agents must stay inside the current repository and task host. They must not create
  additional cloud tasks, connect other repositories, or upload project data to other services.

## Mandatory User Art Approval

- No concept, model, texture, material, animation, VFX pass, benchmark scene, or art task
  may be labeled final, approved, accepted, Art Lock, or complete without the user first
  reviewing it inside Unity and explicitly approving that exact revision.
- Blender renders, automated validators, synthetic reviewers, and passing import tests are
  preparation evidence only; none of them substitutes for the user's visual approval.
- Rejected art may remain only as clearly labeled prototype/reference material and its task
  must remain unchecked until a replacement revision receives explicit approval.

## Mandatory 3D Asset Storage

- Every newly created or externally generated 3D model MUST be stored in the established
  project asset pipeline before it is placed in a scene; temporary folders and download
  locations are not valid long-term storage.
- Keep the editable source under
  `ArtSource/Blender/<Characters|Environment|Ships>/<AssetId>/`, the Unity-ready export
  under the matching `Assets/_Project/Art/<Characters|Environment|Ships>/` folder, and
  local review evidence under `Artifacts/Local/Approval/<LevelOrAsset>/`.
- Provider downloads and untouched incoming files belong under
  `ArtSource/Blender/Incoming/<Provider>/<AssetId>/`. They are source evidence only and
  MUST NOT be referenced directly by a Unity scene.
- Preserve a stable asset ID across source, export, textures, materials, review captures,
  and manifests so future agents can find and revise the model without guessing.
- A model is not ready for scene integration until its editable source, game-ready export,
  `.meta` file, and review location are all accounted for.

## Mandatory Private GitHub / Codex Cloud Workflow

- The user has explicitly authorized this project to use the private GitHub repository
  `ezawy02/lion-of-the-seas` and Codex Cloud. Keep the repository private and keep the Codex
  GitHub connector scoped to this repository only.
- Treat Git and Git LFS as the only project-file transport to the cloud. Do not attach or
  directly upload source assets, images, renders, logs, archives, databases, or local project
  folders to chats or any other service.
- Cloud tasks must check out the repository from GitHub and use branch `001-vertical-slice`
  or an explicitly created descendant branch. Do not repeatedly encode or resend unchanged
  binary assets; transfer changed LFS objects only through normal Git/LFS pushes.
- Keep data-improvement and model-training sharing disabled. Never opt this project into
  training, public sharing, or repository visibility changes.
- Unity and Blender GUI work, local captures, device testing, and the user's mandatory art
  approval remain local. Cloud work may edit repository code and text-based assets, prepare
  changes, and run available headless checks, but it cannot approve art for the user.
- Do not use third-party AI, generation, conversion, storage, or analysis services, and do not
  grant access to any repository other than the private repository above, without new explicit
  scoped approval from the user.
- Avoid reading or opening high-resolution image, model, and review folders unless a task
  strictly requires a targeted file. Prefer manifests, textual metrics, and focused repository
  files, and never embed local project files as base64 task content.
