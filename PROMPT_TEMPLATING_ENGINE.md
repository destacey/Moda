# Prompt Templating Overview

This document describes the general structure, versioning approach, and operational expectations for AI prompt templates used across Moda's AI-assisted workflows. It defines the principles and conventions for building, storing, and executing prompts consistently across multiple features.

---

## 1. Purpose

Establish a reusable and versioned pattern for how Moda constructs, manages, and executes prompts for LLM calls. This approach ensures:

* Consistency across AI-powered features (e.g., objective health, summaries, classifications)
* Predictable model behavior and output formats
* Version traceability and easy iteration without code changes

---

## 2. Prompt Layers

Prompts are composed of four versioned layers that are combined dynamically at runtime:

1. **System Layer** – Defines model behavior, schema, and output contract.
2. **Policy Layer** – Contains rubric text and domain-specific guidance (e.g., definitions of statuses or categories).
3. **Few-Shot Layer** – Includes example input-output pairs for in-context learning.
4. **Context Layer** – The runtime data for the current request (e.g., objective and work items).

Each layer is separately versioned and traceable. Layers 1–3 are stored as reusable templates, while layer 4 is built at runtime.

---

## 3. Template Storage Model

Prompt templates are stored as configuration records in the database or static JSON files. Each record includes:

| Field           | Description                                                                   |
| :-------------- | :---------------------------------------------------------------------------- |
| `name`          | Template name (e.g., `objective_health`)                                      |
| `version`       | Semantic or date-based version (e.g., `2025-11-12`)                           |
| `system_text`   | The system instruction defining behavior and output contract                  |
| `policy_text`   | Domain-specific rubric or rules (e.g., how to interpret data)                 |
| `examples_json` | Array of few-shot examples with `input` and `output` fields                   |
| `schema_json`   | JSON Schema defining valid output format                                      |
| `limits_json`   | Model parameters such as `max_output_tokens`, `temperature`, and `timeout_ms` |

A separate alias table (or file) maps policy references (e.g., `rubric_v1`) to a concrete template version for reproducibility.

---

## 4. Rendering and Execution Flow

The runtime process for generating and executing a prompt:

1. **Resolve Template** – Use the alias (e.g., `rubric_v1`) to load the correct template version.
2. **Assemble Messages** – Combine layers into standard LLM message structure:

   * `system`: from `system_text`
   * `user`: from few-shot examples (alternating input/output) and the current `context`
3. **Trim Context** – Apply deterministic token budgeting (drop low-value evidence first).
4. **Send Request** – Execute against the configured model endpoint with template’s limits.
5. **Validate Response** – Check against `schema_json`; attempt repair once if invalid.
6. **Persist Result** – Store output, template version, context hash, and model metadata.

---

## 5. Layer Descriptions

### 5.1 System Layer

Defines invariant behavior and schema expectations.

* Instructs the model to respond in strict JSON.
* Specifies allowed values, tone, and format.
* Prevents hallucinations, markdown, or irrelevant details.

### 5.2 Policy Layer

Domain guidance defining how the model should reason about the data.

* Encodes rules and definitions (e.g., what counts as Healthy vs. At Risk).
* May include thresholds or heuristics for ambiguous cases.
* Versioned independently to allow iterative refinement.

### 5.3 Few-Shot Layer

Provides concise, realistic examples that demonstrate expected reasoning and output.

* 3–5 examples covering edge and typical cases.
* Minimal, generic language (avoid team/product names).
* Updated alongside the policy version.

### 5.4 Context Layer

Data assembled dynamically by the API at runtime.

* Derived from Moda’s local SQL mirrors or APIs.
* Structured as a **Context Pack** object.
* Serialized into readable text for the model.
* Includes relevant evidence, aggregates, and timestamps.

---

## 6. Validation and Guardrails

All model responses must conform to the template’s `schema_json`. The worker will:

* Validate required fields and types.
* Coerce known synonyms (e.g., `Green` → `Healthy`).
* Trim or sanitize free text fields.
* Retry once with a repair prompt if invalid.
* Log validation status and result.

Failures (invalid twice) are logged and marked with `error=model_output_invalid`.

---

## 7. Versioning and Drift Control

* **Version Bumping:** Any change to rubric text or few-shot examples increments the template version.
* **Shadow Testing:** Run old and new versions in parallel on a sample to measure drift.
* **Alias Updates:** Once a new version is validated, update the alias (e.g., `rubric_v1`) to point to it.
* **Auditability:** Every result stores the template version, alias, and model fingerprint.

---

## 8. Practical Defaults

| Setting               | Recommended Default |
| :-------------------- | :------------------ |
| Temperature           | 0.1                 |
| Max output tokens     | 400                 |
| Timeout (fast-path)   | 3–5 seconds         |
| Timeout (worker)      | 20–30 seconds       |
| Examples per template | 3                   |
| Evidence cap          | 15 items            |
| Description cap       | 600 characters      |

---

## 9. Implementation Notes

* The **Prompt Engine** module is responsible for assembling all layers, applying trimming, and validating responses.
* Configuration should be hot-reloadable for experimentation, but versioned for audit.
* All calls should log token counts, latency, and success rate.
* Prompt and policy text should be centrally managed to avoid duplication across services.

---

## 10. Example Use Case (Objective Health)

For `objective_health`:

* **System:** Instructs model to output `{status, description}` only.
* **Policy:** Defines meaning of `Healthy`, `At Risk`, `Unhealthy`.
* **Few-Shot:** Three examples showing each status type.
* **Context:** Built from objective + work item data in Moda.

Resulting call: System + Policy + Few-Shot + Context → Model → Validated JSON response.

---

This structure ensures all AI features in Moda follow the same composable, versioned, and auditable pattern for prompt design.
