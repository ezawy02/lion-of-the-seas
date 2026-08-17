<!-- SPECKIT START -->
For the current technology, architecture, art pipeline, performance budgets, source-size
limits, and project structure, read `specs/001-vertical-slice/plan.md` before making changes.
<!-- SPECKIT END -->

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
