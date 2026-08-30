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
- Delegated agents must stay inside this repository and must not upload files or connect the
  repository to ChatGPT, Codex Cloud, or any other cloud service.

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

## Mandatory Local-Only / No-Upload Policy

- Treat network conservation as a hard project constraint: do not upload any project file,
  source asset, model, texture, render, screenshot, video, log, archive, or generated output
  from this workspace to Codex Cloud, external AI services, image-analysis services, storage
  services, websites, APIs, or any other remote destination.
- Do not transmit Unity or Blender review captures for remote/model visual inspection. Save
  review evidence locally under `Artifacts/Local/` and have the user review the exact local
  revision inside Unity.
- Unity and Blender work must run on the user's local machine. Prefer local background/batch
  execution for imports, builds, tests, and captures; opening the GUI is allowed when the
  user needs to review the result locally.
- Do not use a supplied API key or invoke a remote generation/conversion API for this
  project unless the user explicitly authorizes that exact upload for the current task.
- Do not download packages or assets automatically. Reuse installed tools, cached packages,
  and existing local assets; obtain explicit approval before any network download.
- Sharing a local path or opening a local file on the user's machine is allowed and is not
  considered an upload. If a requested operation cannot be performed without transmitting a
  local file, stop and ask for explicit scoped permission.
