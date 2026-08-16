<!-- SPECKIT START -->
For the current technology, architecture, art pipeline, performance budgets, source-size
limits, and project structure, read `specs/001-vertical-slice/plan.md` before making changes.
<!-- SPECKIT END -->

## Sub-agent Cost Policy

- Use `gpt-5.6-luna` for every delegated sub-agent task in this project.
- The primary agent must review all delegated output before it is accepted or applied.
- If a Luna agent fails, stalls, produces an uncertain result, or reaches a task that needs
  higher-confidence engineering judgment, the primary agent must take over directly.
- Delegated agents must stay inside this repository and must not upload files or connect the
  repository to ChatGPT, Codex Cloud, or any other cloud service.
