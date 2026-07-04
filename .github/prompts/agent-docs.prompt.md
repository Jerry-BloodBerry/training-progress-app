---
name: agent-docs
description: Use this prompt to generate rules docs for agents in this workspace to reference when they are executing their tasks. The agents will pick up the docs file as long as it is relevant to their task and ALWAYS follow the rules from it.
agent: rules-docs-writer
---

Generate a rules documentation file for agents in this workspace.

The file will be saved in `training-progress-app/docs/` and automatically picked up by any agent whose task is relevant to its content.

Provide a description of the rules you want to create. Include:
- The specific domain, layer, or process the rules govern (e.g., "API error handling", "Angular component naming", "SCSS theming")
- Optionally, a file name to save the rules under (e.g., `error-handling.md`). If omitted, a name will be derived from the description.

The generated file will include:
- A summary of the rules and their purpose
- A detailed description with context and rationale
- Technologies or tools involved (where applicable)
- Concrete examples of correct and incorrect usage
- A DO's and DON'Ts section for quick agent reference

