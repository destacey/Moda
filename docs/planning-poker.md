# Planning Poker

Planning Poker is a real-time, collaborative estimation technique used by teams to assign effort or complexity estimates to work items. A facilitator creates a session, adds rounds representing items to estimate, and participants vote using an estimation scale (e.g., Fibonacci). Votes are hidden until the facilitator reveals them, at which point the team discusses and reaches consensus.

## Concepts

- **Session** - A named estimation meeting that groups rounds together. Sessions use a specific estimation scale and are managed by a facilitator.
- **Round** - A single item being estimated within a session. Each round goes through a voting lifecycle: Voting, Revealed, and Accepted.
- **Vote** - A participant's estimate for a round, selected from the session's estimation scale.
- **Estimation Scale** - A predefined set of values used for voting (e.g., Fibonacci: 1, 2, 3, 5, 8, 13, 21).
- **Facilitator** - The employee who creates and manages the session (reveals votes, sets consensus, manages rounds).
- **Participant** - Any authenticated user who opens the session page. Presence is tracked in real time via SignalR.

## Models

- [Poker Session](#poker-session)
- [Poker Round](#poker-round)
- [Poker Vote](#poker-vote)
- [Estimation Scale](#estimation-scale)

## Poker Session

A poker session represents a single estimation meeting. It is created in an Active state and can be completed when the facilitator is done estimating.

### Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| Id | Guid | Yes | Primary key |
| Key | int | Yes | Auto-generated alternate key for display (e.g., PS-1) |
| Name | string | Yes | Session name (max 256 chars) |
| EstimationScaleId | int | Yes | The estimation scale used for voting |
| FacilitatorId | Guid | Yes | The employee who manages the session |
| Status | PokerSessionStatus | Yes | Current lifecycle state |
| ActivatedOn | Instant | Yes | Timestamp when the session was created |
| CompletedOn | Instant | No | Timestamp when the session was completed |

### Status Lifecycle

```
Created → Active (immediate on creation) → Completed
```

Sessions are created in `Active` status and can only transition to `Completed`. There is no draft or pending state.

### Operations

| Operation | Allowed When | Description |
|-----------|-------------|-------------|
| Create | Always | Creates session in Active status with a facilitator and estimation scale |
| Complete | Active | Marks session as Completed, sets CompletedOn timestamp |
| Add Round | Active | Adds a new round with an auto-incrementing order |
| Remove Round | Active | Removes a round and its votes |

## Poker Round

A poker round represents a single item being estimated. Rounds move through a voting lifecycle managed by the facilitator.

### Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| Id | Guid | Yes | Primary key |
| PokerSessionId | Guid | Yes | Parent session |
| Label | string | No | Free-text description of what is being estimated (max 512 chars) |
| Status | PokerRoundStatus | Yes | Current lifecycle state |
| ConsensusEstimate | string | No | Final agreed estimate (max 32 chars) |
| Order | int | Yes | Display order within session (auto-incrementing) |

### Status Lifecycle

```
Voting → Revealed → Accepted
           ↓
         Voting (reset)
```

| Status | Description |
|--------|-------------|
| Voting | Round is accepting votes. Vote values are hidden from other participants. |
| Revealed | Votes have been revealed. The facilitator can now set consensus or reset. |
| Accepted | Consensus estimate has been set. The round is finalized. |

### Operations

| Operation | Allowed When | Description |
|-----------|-------------|-------------|
| Submit Vote | Voting | Participant submits or updates their estimate |
| Reveal | Voting | Facilitator reveals all votes |
| Reset | Voting or Revealed | Clears all votes and returns to Voting status |
| Set Consensus | Revealed | Facilitator sets the agreed estimate, transitions to Accepted |

### Vote Visibility

When a round is in `Voting` status, the API returns an empty votes array to prevent clients from seeing other participants' votes. Once revealed, vote values are included in the response.

## Poker Vote

A vote records a participant's estimate for a specific round.

### Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| Id | Guid | Yes | Primary key |
| PokerRoundId | Guid | Yes | The round this vote belongs to |
| ParticipantId | Guid | Yes | The employee who submitted the vote |
| Value | string | Yes | The estimate value (max 32 chars) |
| SubmittedOn | Instant | Yes | Timestamp of submission |

A participant can only have one vote per round. Submitting again updates the existing vote.

## Estimation Scale

Estimation scales define the set of values available for voting.

### Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| Id | int | Yes | Primary key |
| Name | string | Yes | Scale name (max 256 chars) |
| Description | string | No | Optional description |
| IsActive | bool | Yes | Whether the scale is available for new sessions |
| Values | string[] | Yes | Ordered list of estimation values (minimum 2) |

Estimation scales are managed separately in Settings and shared across all poker sessions.

## Real-Time Communication

Planning Poker uses SignalR for real-time updates. All game events and participant presence are communicated through the `PlanningPokerHub`.

### Participant Presence

Presence is tracked in-memory (no database persistence). When a user opens a poker session page, they are registered as a connected participant. Presence resets on application restart.

| Event | Payload | Trigger |
|-------|---------|---------|
| ParticipantList | `{id, name}[]` | Sent to a user when they join — contains all currently connected participants |
| ParticipantJoined | `{id, name}` | Broadcast when a new user connects (not on additional tabs from same user) |
| ParticipantLeft | `{id}` | Broadcast when a user's last connection disconnects |

Multi-tab support: each user can have multiple connections (tabs). `ParticipantLeft` only fires when all connections for that user are closed.

The presence identity uses the application's user ID and the `name` claim from Azure AD (not the Employee record). This means any authenticated user appears in presence, regardless of whether they have an associated Employee.

### Game Events

Game events are broadcast to all connections in the session group. The frontend handles these by invalidating the RTK Query cache, causing the session data to re-fetch.

| Event | Trigger |
|-------|---------|
| VoteSubmitted | A participant submits or updates a vote |
| VotesRevealed | Facilitator reveals votes for a round |
| ConsensusSet | Facilitator sets the consensus estimate |
| RoundReset | Facilitator resets a round for re-voting |
| SessionCompleted | Facilitator completes the session |
| RoundAdded | A new round is added to the session |
| RoundRemoved | A round is removed from the session |

### Fallback

The frontend polls the session API every 5 seconds as a fallback if SignalR is unavailable (e.g., local development without SignalR configured).

## API Endpoints

Base route: `api/planning/poker-sessions`

| Method | Route | Description | Permission |
|--------|-------|-------------|------------|
| GET | `/` | List sessions (optional status filter) | View |
| GET | `/{idOrKey}` | Get session details | View |
| POST | `/` | Create session | Create |
| PUT | `/{id}/complete` | Complete session | Update |
| POST | `/{id}/rounds` | Add round | Update |
| DELETE | `/{id}/rounds/{roundId}` | Remove round | Update |
| PUT | `/{id}/rounds/{roundId}/reveal` | Reveal votes | Update |
| PUT | `/{id}/rounds/{roundId}/reset` | Reset round | Update |
| PUT | `/{id}/rounds/{roundId}/consensus` | Set consensus | Update |
| POST | `/{id}/rounds/{roundId}/vote` | Submit vote | Update |

## UI

### Session List Page

`/planning/poker-sessions`

Displays all poker sessions in an AG Grid table with columns: Key, Name, Status, Facilitator, Round Count. A "Create Session" button opens a modal form.

### Session Detail Page

`/planning/poker-sessions/{key}`

The detail page is divided into two areas:

**Header Bar**
- Session name with status tag (Active/Completed)
- Facilitator name
- Current round label, round status tag, and vote count (X/Y voted)
- Estimation scale name tag
- "Copy Invite Link" button
- Connected participant avatars (live presence)

**Main Content (left column)**
- Participant voting cards showing each connected participant and their vote status:
  - Pending (dashed border, empty) — has not voted
  - Voted (solid border, diamond symbol) — voted but not yet revealed
  - Revealed (highlighted, showing vote value) — votes have been revealed
- Action area:
  - Voting: "Reveal Cards" button (facilitator only)
  - Revealed: "Re-vote" button, consensus select dropdown, "Accept" button (facilitator only)
  - Accepted: consensus tag displayed
- Estimation card deck (Voting status only): clickable cards for each scale value, with selected state

**Sidebar (right column)**
- Session Summary: Estimated count (accepted/total), Total Points, Average Estimate
- Estimation History: scrollable list of rounds with status icons, consensus tags, and inline add-round input
- Complete Session button (facilitator only, Active sessions)

### Participant Cards

The voting area shows a card for each participant. The participant list is a union of:
1. **Connected participants** — users currently on the page (from SignalR presence)
2. **Round voters** — users who voted in the current round but may have disconnected

This ensures that a disconnected user's vote is still visible while also showing all currently connected users even if they haven't voted yet.

## Permissions

| Permission | Description |
|------------|-------------|
| Permissions.PokerSessions.View | View session list and details |
| Permissions.PokerSessions.Create | Create new sessions |
| Permissions.PokerSessions.Update | Manage rounds, reveal votes, set consensus, complete session, submit votes |

## Architecture Notes

- **No repository pattern**: Commands and queries use EF Core `IPlanningDbContext` directly.
- **Result pattern**: All domain operations return `Result` or `Result<T>` from CSharpFunctionalExtensions.
- **Presence is in-memory**: Uses static `ConcurrentDictionary` in the SignalR hub. Resets on app restart. Acceptable for ephemeral presence data.
- **Azure SignalR**: Conditionally enabled when `Azure:SignalR:ConnectionString` is configured. Falls back to in-process SignalR for local development.
- **Vote hiding**: The `HideUnrevealedVotes()` method on the DTO clears the votes array for rounds not in Revealed or Accepted status, preventing vote values from leaking to clients before the facilitator reveals them.
