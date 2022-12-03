# Work Domain

# Models
- [Workspace](#workspace)
- [Work Item](#work-item)
- [Work Item Revision](#work-item-revision)
- [Work Item Revision Change](#work-item-revision-change)
- [Work Process](#work-process)
- [Work Process Configuration](#work-process-configuration)
- [Work Type](#work-type)
- [Backlog Category](#backlog-category)
- [Backlog Level](#backlog-level)
- [Workflow](#workflow)
- [Workflow Configurtion](#workflow-configuration)
- [Work State](#work-state)
- [Work State Category](#work-state-category)

## Workspace
A workspace is a container for work items. A workspace lives within a single [organization](./organization.md).

A workspace is either owned by Moda or managed by an external application and synchronized with Moda.

## Work Item
A work item represents a piece of work that needs to be completed.

## Work Item Revision
The work item revision is a change record for a work item.

## Work Item Revision Change
A specific field change within a work item revision.

## Work Process
The work process defines a set of work process configurations that can be used within a workspace.  A work process can be used in many workspaces.

### Business Rules
- A work process requires at least one work type to be configured.
- A work type can only be defined once within a work process.
- A single work process can be used by multiple workspaces.

## Work Process Configuration
The work process configuration links work types with workflows and defines the backlog level for each to the work process.

## Work Type
Represents the type of work item.  Examples:
- Story
- Bug
- Feature
- Epic


## Backlog Category
An enum that enables backlog levels to be grouped based on purpose and functionality and normalized across the organization.
- Portfolio - Portfolio backlogs provide a way to group related items into a hierarchical structure.  This is the only backlog category that allows multiple backlog levels.
- Requirement - The requirement backlog category contains your base level work items.  These work items are owned by a single team and represent the actual work.
- Task - The task backlog contains task work items that are owned and managed by a parent work item.  The parent work item is typically from the requirement backlog category.

## Backlog Level
Allows work types to be grouped and defined in a hierarchy that is normalized across the organization.  The default levels are:
- Portfolio
- Product
- Team
- Task

## Workflow
A workflow is a set of work states that define the different stages a work item must go through to be considered done.

### Business Rules
- A workflow requires at least three work states be configured.
- Each of the work state categories must be represented in a workflow for it to be valid.
- A work state can only be defined once within a workflow.
- The order of work state categories within the workflow configuration must be grouped.  Proposed items are always at the beginning of the workflow and Done items are always at the end.
  - Valid Example (Order, Work State, Work State Category)
    - 1, New, Proposed
    - 2, In Progress, Active
    - 3, In Review, Active
    - 4, Completed, Done
  - Invalid Example (Order, Work State, Work State Category)
    - 1, New, Proposed
    - 2, In Progress, Active   <-- An Active work state category can not be configured in between configs with Proposed work state categories
    - 3, Ready, Proposed
    - 4, Completed, Done

### Open Questions
- [ ] Future - need to define transitions and rules for owned workspaces.
- [ ] How should we handle changes?  I think workflows should be immutable if are or ever have been assigned.

## Workflow Configuration
The workflow configuration links work states, work state categories, and the order to the workflow.

## Work State
The name of the stage within the workflow.  Each work state can be used in many workflows.

The name of the work state cannot be changed.  A new name represents a new work state.

## Work State Category
The work state category is an enum that helps sort and normalize work states across workflows.  The order and available values are:
1. Proposed - The work has been proposed but not yet started.  This is also useful for workflows that have additional work states used to collect requirements, but don't want to consider those as active or in progress.
2. Active - The work is currently being performed.
3. Done - The work has been completed.
4. Removed - The work has been removed from the backlog without being completed.

# ERD
![work domain erd](/work-domain-erd.drawio.svg)