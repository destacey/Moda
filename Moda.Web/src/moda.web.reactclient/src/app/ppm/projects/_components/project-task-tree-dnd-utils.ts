import { ProjectTaskTreeDto } from '@/src/services/moda-api'

/**
 * Flattened project task with metadata for drag-and-drop operations
 */
export interface FlattenedProjectTask extends ProjectTaskTreeDto {
  depth: number
  parentId: string | null
  ancestorIds: string[]
  flatIndex: number
}

/**
 * Projection result showing where a dragged task will land
 */
export interface TaskProjection {
  depth: number
  maxDepth: number
  minDepth: number
  parentId: string | null
  parentTask: ProjectTaskTreeDto | null
  canDrop: boolean
  reason?: string
}

/**
 * Constants for drag-and-drop calculations
 */
export const INDENTATION_WIDTH = 32 // pixels per depth level
export const DRAG_ACTIVATION_DISTANCE = 8 // pixels before drag activates

/**
 * Flattens a tree structure into an array with depth and parent metadata.
 * This is used to work with dnd-kit's flat array model while preserving hierarchical relationships.
 *
 * @param items - The tree items to flatten
 * @param parentId - The ID of the parent (null for root level)
 * @param depth - The current depth level (0 for root)
 * @param ancestorIds - Array of ancestor IDs for circular reference prevention
 * @returns Flattened array with metadata
 */
export function flattenTree(
  items: ProjectTaskTreeDto[],
  parentId: string | null = null,
  depth: number = 0,
  ancestorIds: string[] = [],
): FlattenedProjectTask[] {
  const result: FlattenedProjectTask[] = []
  let flatIndex = 0

  const flatten = (
    tasks: ProjectTaskTreeDto[],
    currentParentId: string | null,
    currentDepth: number,
    currentAncestors: string[],
  ): void => {
    tasks.forEach((task) => {
      result.push({
        ...task,
        depth: currentDepth,
        parentId: currentParentId,
        ancestorIds: currentAncestors,
        flatIndex: flatIndex++,
      })

      if (task.children && task.children.length > 0) {
        flatten(task.children, task.id, currentDepth + 1, [
          ...currentAncestors,
          task.id,
        ])
      }
    })
  }

  flatten(items, parentId, depth, ancestorIds)
  return result
}

/**
 * Rebuilds a tree structure from a flattened array.
 * Used after drag-and-drop to reconstruct the hierarchical structure.
 *
 * @param flatItems - The flattened items to rebuild into a tree
 * @returns Tree structure with nested children
 */
export function buildTree(
  flatItems: FlattenedProjectTask[],
): ProjectTaskTreeDto[] {
  // Create a root pseudo-node to simplify logic
  const root: ProjectTaskTreeDto & {
    id: string
    children: ProjectTaskTreeDto[]
  } = {
    id: '__root__',
    children: [],
  } as any

  // Create lookup map for all nodes
  const nodes: Record<
    string,
    ProjectTaskTreeDto & { children: ProjectTaskTreeDto[] }
  > = {
    [root.id]: root,
  }

  // Initialize all items with empty children arrays
  const items = flatItems.map((item) => {
    const { depth, parentId, ancestorIds, flatIndex, ...taskProps } = item
    return {
      ...taskProps,
      children: [],
    }
  })

  // Build the node lookup and attach children to parents
  for (const item of items) {
    const parentId =
      flatItems.find((f) => f.id === item.id)?.parentId ?? root.id
    const parent = nodes[parentId] ?? root

    nodes[item.id] = item
    parent.children.push(item)
  }

  return root.children
}

/**
 * Finds a task by ID in a flattened array
 */
function findTaskById(
  flatItems: FlattenedProjectTask[],
  id: string,
): FlattenedProjectTask | null {
  return flatItems.find((item) => item.id === id) ?? null
}

/**
 * Finds a task by ID in the original tree structure (for getting full task details)
 */
function findTaskInTree(
  items: ProjectTaskTreeDto[],
  id: string,
): ProjectTaskTreeDto | null {
  for (const item of items) {
    if (item.id === id) return item
    if (item.children && item.children.length > 0) {
      const found = findTaskInTree(item.children, id)
      if (found) return found
    }
  }
  return null
}

/**
 * Calculates the projected position and parent for a dragged task.
 * This is the core function that determines where a task will land based on:
 * - The vertical position (which task it's over)
 * - The horizontal offset (how far right/left it's been dragged)
 *
 * @param items - Flattened task array
 * @param activeId - ID of the task being dragged
 * @param overId - ID of the task being hovered over
 * @param dragOffset - Horizontal drag offset in pixels
 * @param indentationWidth - Pixels per indentation level
 * @returns Projection with new depth, parent, and validation
 */
export function getProjection(
  items: FlattenedProjectTask[],
  activeId: string,
  overId: string,
  dragOffset: number,
  indentationWidth: number = INDENTATION_WIDTH,
): TaskProjection {
  const activeTask = findTaskById(items, activeId)
  const overTask = findTaskById(items, overId)

  if (!activeTask || !overTask) {
    return {
      depth: 0,
      maxDepth: 0,
      minDepth: 0,
      parentId: null,
      parentTask: null,
      canDrop: false,
      reason: 'Task not found',
    }
  }

  const activeIndex = items.findIndex((item) => item.id === activeId)
  const overIndex = items.findIndex((item) => item.id === overId)

  // Calculate depth change from horizontal drag offset
  const dragDepth = Math.round(dragOffset / indentationWidth)

  // When no horizontal drag (dragDepth === 0):
  // - Default to matching the overTask's depth (standard "insert as sibling" behavior)
  // - BUT cap it to not exceed the activeTask's current depth unless user explicitly drags right
  // This prevents accidentally nesting deeper when dropping on a nested child
  let defaultDepth = overTask.depth
  if (dragDepth === 0 && overTask.depth > activeTask.depth) {
    // Don't auto-nest deeper than current level without explicit horizontal drag
    defaultDepth = activeTask.depth
  }
  const projectedDepth =
    dragDepth === 0 ? defaultDepth : activeTask.depth + dragDepth

  // Use the "over" item's context to determine depth constraints
  // The active item is being placed after/near the over item
  const previousItem = overIndex > 0 ? items[overIndex - 1] : null
  const nextItem = overIndex < items.length - 1 ? items[overIndex + 1] : null

  // When dragging item A over item B:
  // - If no horizontal drag: A becomes sibling of B (same depth)
  // - If dragged right (positive offset): A could become child of B (or previous sibling if B is self)
  // - If dragged left (negative offset): A moves to shallower depth
  // Special case: when dragging over self with positive offset, use previous item's depth
  const referenceTask =
    activeId === overId && dragDepth > 0 && previousItem
      ? previousItem
      : overTask
  const maxDepth = referenceTask.depth + 1
  const minDepth = 0

  // Constrain projected depth to valid range
  let depth = projectedDepth
  if (projectedDepth > maxDepth) {
    depth = maxDepth
  } else if (projectedDepth < minDepth) {
    depth = minDepth
  }

  // Calculate new parent ID based on depth
  const getParentId = (): string | null => {
    if (depth === 0) {
      return null // Root level
    }

    // If same depth as over task, they share the same parent
    if (depth === overTask.depth) {
      return overTask.parentId
    }

    // If deeper than over task, we need to find a parent by looking backwards
    // The parent should be the nearest task at (depth - 1) ABOVE the drop position
    // This handles the case: drag Task 3 between Task 1 and Task 2, drag right
    // → Task 1 should be the parent, not Task 2 (which we're hovering over)
    if (depth > overTask.depth) {
      // Look backwards to find the nearest task at the target parent depth
      for (let i = overIndex - 1; i >= 0; i--) {
        const item = items[i]
        // Skip the active task itself
        if (item.id === activeId) continue

        // Found a task at the target parent depth
        if (item.depth === depth - 1) {
          return item.id
        }
        // If we've gone past the target depth without finding a match, stop
        if (item.depth < depth - 1) {
          break
        }
      }
      return null
    }

    // If shallower than over task, we need to find the appropriate parent
    // by looking backwards from the over position to find a task at (depth - 1)
    // This handles the case where we're hovering over a deeply nested task
    // but want to stay at a shallower level
    for (let i = overIndex; i >= 0; i--) {
      const item = items[i]
      // Skip the active task itself
      if (item.id === activeId) continue

      // Found a task at the target parent depth
      if (item.depth === depth - 1) {
        return item.id
      }
      // If we've gone past the target depth without finding a match, stop
      if (item.depth < depth - 1) {
        break
      }
    }

    return null
  }

  const parentId = getParentId()

  // Find the parent task for validation
  let parentTask: ProjectTaskTreeDto | null = null
  if (parentId) {
    const parentFlat = findTaskById(items, parentId)
    if (parentFlat) {
      parentTask = parentFlat
    }
  }

  // Validate the projection
  const validation = validateMove(activeTask, parentTask, parentId)

  return {
    depth,
    maxDepth,
    minDepth,
    parentId,
    parentTask,
    canDrop: validation.canMove,
    reason: validation.reason,
  }
}

/**
 * Validates whether a task can be moved to a new parent.
 * Checks for:
 * - Circular references (parent moving into own descendant)
 * - Business rules (e.g., milestones can't be parents)
 * - Self-assignment
 *
 * @param activeTask - The task being moved
 * @param targetParent - The proposed new parent (null for root)
 * @param targetParentId - The ID of the proposed new parent
 * @returns Validation result with reason if invalid
 */
export function validateMove(
  activeTask: FlattenedProjectTask,
  targetParent: ProjectTaskTreeDto | null,
  targetParentId: string | null,
): { canMove: boolean; reason?: string } {
  // No parent (root level) is always valid
  if (!targetParentId) {
    return { canMove: true }
  }

  // Cannot be its own parent
  if (activeTask.id === targetParentId) {
    return {
      canMove: false,
      reason: 'A task cannot be its own parent',
    }
  }

  // Check if target parent is a descendant of the active task
  // This prevents circular references (moving a parent into its own child)
  if (targetParent && 'ancestorIds' in targetParent) {
    const targetAncestors =
      (targetParent as FlattenedProjectTask).ancestorIds || []
    if (targetAncestors.includes(activeTask.id)) {
      return {
        canMove: false,
        reason:
          'Cannot move a parent task into its own children or descendants',
      }
    }
  }

  // Business rule: Milestones cannot be parents
  if (targetParent?.type?.name === 'Milestone') {
    return {
      canMove: false,
      reason: 'Milestones cannot have child tasks',
    }
  }

  return { canMove: true }
}

/**
 * Calculates the new order/position for a task within its parent.
 * This determines the sequential position among siblings.
 * Note: The API uses 1-based ordering (1, 2, 3...), not 0-based.
 *
 * @param flatItems - Flattened task array
 * @param activeId - ID of the task being moved
 * @param overIndex - Index in the flat array where we're dropping (the task we're hovering over)
 * @param parentId - The new parent ID (null for root)
 * @returns One-based order index within the parent (starts at 1, not 0)
 */
export function calculateOrderInParent(
  flatItems: FlattenedProjectTask[],
  activeId: string,
  overIndex: number,
  parentId: string | null,
): number {
  // Get all current siblings with the same parent (excluding the active task)
  const siblings = flatItems.filter(
    (item) => item.parentId === parentId && item.id !== activeId,
  )

  if (siblings.length === 0) {
    return 1 // First (and only) item in parent
  }

  // Find the active task's original position to determine drag direction
  const activeIndex = flatItems.findIndex((item) => item.id === activeId)
  const isDraggingDown = activeIndex < overIndex

  // Find the task we're hovering over
  const overTask = flatItems[overIndex]

  // If we're hovering over a task with the same parent
  if (overTask.parentId === parentId) {
    const overSiblingIndex = siblings.findIndex((s) => s.id === overTask.id)
    if (overSiblingIndex >= 0) {
      // When dragging down, insert AFTER the hovered item
      // When dragging up, insert BEFORE (at) the hovered item's position
      return isDraggingDown ? overSiblingIndex + 2 : overSiblingIndex + 1
    }
  }

  // Find the first sibling that comes after overIndex in the flat array
  // This handles cases where we're hovering over a task at a different depth
  for (let i = 0; i < siblings.length; i++) {
    const siblingFlatIndex = flatItems.findIndex((t) => t.id === siblings[i].id)
    if (siblingFlatIndex > overIndex) {
      // Insert before this sibling
      return i + 1 // 1-based index
    }
  }

  // If no sibling comes after overIndex, insert at the end
  return siblings.length + 1
}

/**
 * Updates a task's parent and order in the flattened array.
 * This creates a new array with the updated task metadata.
 *
 * @param flatItems - Flattened task array
 * @param taskId - ID of task to update
 * @param newParentId - New parent ID
 * @param newDepth - New depth level
 * @returns Updated flattened array
 */
export function updateTaskPlacement(
  flatItems: FlattenedProjectTask[],
  taskId: string,
  newParentId: string | null,
  newDepth: number,
): FlattenedProjectTask[] {
  return flatItems.map((item) => {
    if (item.id === taskId) {
      // Update ancestor IDs based on new parent
      let newAncestorIds: string[] = []
      if (newParentId) {
        const parent = flatItems.find((f) => f.id === newParentId)
        newAncestorIds = parent
          ? [...parent.ancestorIds, newParentId]
          : [newParentId]
      }

      return {
        ...item,
        parentId: newParentId,
        depth: newDepth,
        ancestorIds: newAncestorIds,
      }
    }
    return item
  })
}
