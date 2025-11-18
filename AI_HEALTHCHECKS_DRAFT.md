# AI Objective Health Assessment – Context Pack v1

This document defines the contract between the **Moda API** (which assembles objective and work-item data) and the **AI worker** (which evaluates objective health via an LLM).

---

## 1. Purpose

Provide a compact, deterministic data payload (“Context Pack”) that represents one objective’s current state, so the AI model can classify overall health (**Healthy**, **At Risk**, or **Unhealthy**) and give a concise rationale.

---

## 2. Context Pack (API → Worker)

### Overview

* Format: JSON
* Size target: ≤ 6k tokens total
* One pack per objective per evaluation
* Field names are stable; versioned by `policy.rubric: v1`

### Schema

| Field              | Type              | Required | Description                                        |         |                 |    |
| :----------------- | :---------------- | :------- | :------------------------------------------------- | ------- | --------------- | -- |
| `objective`        | object            | ✅        | Core metadata for the objective                    |         |                 |    |
| ├─ `id`            | string            | ✅        | Internal objective ID                              |         |                 |    |
| ├─ `title`         | string            | ✅        | Short title                                        |         |                 |    |
| ├─ `description`   | string            | ✅        | Free-text description                              |         |                 |    |
| ├─ `targetWindow`  | object OR null     | ☐        | Optional schedule window                           |         |                 |    |
|     ├─ `start`     | string (ISO 8601) | ☐        | Planned start date                                 |         |                 |    |
|     ├─ `end`       | string (ISO 8601) | ☐        | Planned end date                                   |         |                 |    |
| `aggregates`       | object            | ✅        | Pre-computed summary metrics                       |         |                 |    |
| ├─ `items`         | object            | ✅        | `{total, done, active, blocked, newLast7d}`        |         |                 |    |
| ├─ `schedule`      | object            | ✅        | `{daysElapsed, daysTotal, daysRemaining, overdue}` |         |                 |    |
| ├─ `velocity2w`    | object            | ✅        | `{planned, completed}` (last 14 days)              |         |                 |    |
| ├─ `scopeChange2w` | number            | ✅        | Net Δ items (+ adds – removals, 14 days)           |         |                 |    |
| ├─ `freshness`     | object            | ✅        | `{meanLastUpdateDays, pctStaleGt7d}`               |         |                 |    |
| ├─ `riskFlags`     | string[]          | ✅        | Short labels, e.g. `dependency:PaymentsAPI`        |         |                 |    |
| `evidence`         | object[]          | ✅        | Representative work items (≤ 20)                   |         |                 |    |
| ├─ `id`            | string            | ✅        | Work item ID                                       |         |                 |    |
| ├─ `state`         | string            | ✅        | `Active                                            | Blocked | Done            | …` |
| ├─ `priority`      | string            | ☐        | `High                                              | Medium  | Low`            |    |
| ├─ `lastUpdate`    | string (ISO 8601) | ✅        | Last activity timestamp                            |         |                 |    |
| ├─ `title`         | string            | ✅        | One-line title (≤ 120 chars)                       |         |                 |    |
| ├─ `note`          | string            | ☐        | Short note (≤ 120 chars)                           |         |                 |    |
| `contextQuality`   | object            | ✅        | `{completeness: high                               | medium  | low, dataAsOf}` |    |
| `policy`           | object            | ✅        | `{rubric: v1}` – version marker                    |         |                 |    |
| `contextHash`      | string            | ✅        | SHA-256 of core fields for caching/idempotency     |         |                 |    |

### Evidence Selection Policy

1. All **Blocked** items (High/Critical first)
2. 3–5 most recent **Active** items
3. 3–5 most recent **Done** items
4. Any **stale Active** (> 14 days since update)
5. Cap 20 total lines (earlier rules take precedence)

---

## 3. Expected LLM Output (Worker ← Model)

### Schema

| Field         | Type   | Required | Description                     |         |            |
| :------------ | :----- | :------- | :------------------------------ | ------- | ---------- |
| `status`      | string | ✅        | `Healthy                        | At Risk | Unhealthy` |
| `description` | string | ✅        | Short explanation (≤ 600 chars) |         |            |

### Validation Rules

* Must include both fields and **no extras**
* `status` must match the enum (case-insensitive)

  * Map synonyms: `Green → Healthy`, `Yellow → At Risk`, `Red → Unhealthy`
* Trim `description` to ≤ 600 chars; plain text only (no markdown/URLs)
* If data is stale or sparse, API may override to `At Risk` and prefix description with `(Stale data) …`
* Persist with metadata: `evaluatedAt`, `model`, `contextHash`

---

## 4. Prompt Rubric (used by Worker)

| Status        | Meaning                                                                |
| :------------ | :--------------------------------------------------------------------- |
| **Healthy**   | On schedule or minor slip with momentum; no critical blockers.         |
| **At Risk**   | Credible plan exists but schedule slip / rising risk / stale activity. |
| **Unhealthy** | Material delay and unresolved critical blockers or scope instability.  |

---

## 5. Prompt Construction

### System Instruction

You are assessing the health of a PI objective. Return **strict JSON** with exactly two fields: `status` and `description`. The `status` value must be one of: `Healthy`, `At Risk`, or `Unhealthy`. The `description` should concisely explain why that status was chosen, grounded in the provided evidence. Do not include markdown, formatting, or extra fields.

---

## 6. Versioning & Caching

* Increment `policy.rubric` on material prompt/schema changes.
* Skip regeneration if `contextHash` matches a recent result (< 24 h).

---

## 7. Example Context Pack → Expected Output

Input (truncated):

objective: { id: obj-123, title: Reduce checkout failures, ... }
aggregates: { items: { total: 42, done: 18, active: 19, blocked: 3, newLast7d: 7 }, ... }
evidence: [ { id: 98123, state: Blocked, priority: High, title: Integrate Payments API, note: waiting on key } ]
contextHash: sha256:abcd...

Model Output:

status: At Risk
description: Multiple high-priority tasks remain blocked by external dependencies; progress has slowed though recent fixes show partial momentum.
