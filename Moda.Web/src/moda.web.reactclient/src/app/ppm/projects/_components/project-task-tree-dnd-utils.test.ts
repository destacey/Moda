import { ProjectTaskTreeDto } from '@/src/services/moda-api'
import {
  flattenTree,
  buildTree,
  getProjection,
  validateMove,
  calculateOrderInParent,
  updateTaskPlacement,
  FlattenedProjectTask,
  INDENTATION_WIDTH,
} from './project-task-tree-dnd-utils'

// Helper to create a mock task
const createMockTask = (
  id: string,
  name: string,
  typeName: string = 'Task',
  children: ProjectTaskTreeDto[] = [],
): ProjectTaskTreeDto => ({
  id,
  key: `TSK-${id}`,
  name,
  type: { id: 1, name: typeName, description: '', order: 1 },
  status: { id: 1, name: 'Not Started', description: '', order: 1 },
  children,
})

describe('project-task-tree-dnd-utils', () => {
  describe('flattenTree', () => {
    it('should flatten a simple tree with no children', () => {
      const tree: ProjectTaskTreeDto[] = [
        createMockTask('1', 'Task 1'),
        createMockTask('2', 'Task 2'),
      ]

      const result = flattenTree(tree)

      expect(result).toHaveLength(2)
      expect(result[0]).toMatchObject({
        id: '1',
        depth: 0,
        parentId: null,
        ancestorIds: [],
        flatIndex: 0,
      })
      expect(result[1]).toMatchObject({
        id: '2',
        depth: 0,
        parentId: null,
        ancestorIds: [],
        flatIndex: 1,
      })
    })

    it('should flatten a tree with nested children', () => {
      const tree: ProjectTaskTreeDto[] = [
        createMockTask('1', 'Task 1', 'Task', [
          createMockTask('1-1', 'Task 1.1', 'Task', [
            createMockTask('1-1-1', 'Task 1.1.1'),
          ]),
          createMockTask('1-2', 'Task 1.2'),
        ]),
        createMockTask('2', 'Task 2'),
      ]

      const result = flattenTree(tree)

      expect(result).toHaveLength(5)

      // Root level
      expect(result[0]).toMatchObject({
        id: '1',
        depth: 0,
        parentId: null,
        ancestorIds: [],
      })

      // First child
      expect(result[1]).toMatchObject({
        id: '1-1',
        depth: 1,
        parentId: '1',
        ancestorIds: ['1'],
      })

      // Grandchild
      expect(result[2]).toMatchObject({
        id: '1-1-1',
        depth: 2,
        parentId: '1-1',
        ancestorIds: ['1', '1-1'],
      })

      // Second child
      expect(result[3]).toMatchObject({
        id: '1-2',
        depth: 1,
        parentId: '1',
        ancestorIds: ['1'],
      })

      // Root level task 2
      expect(result[4]).toMatchObject({
        id: '2',
        depth: 0,
        parentId: null,
        ancestorIds: [],
      })
    })

    it('should preserve order within each level', () => {
      const tree: ProjectTaskTreeDto[] = [
        createMockTask('1', 'First'),
        createMockTask('2', 'Second'),
        createMockTask('3', 'Third'),
      ]

      const result = flattenTree(tree)

      expect(result[0].id).toBe('1')
      expect(result[1].id).toBe('2')
      expect(result[2].id).toBe('3')
      expect(result[0].flatIndex).toBe(0)
      expect(result[1].flatIndex).toBe(1)
      expect(result[2].flatIndex).toBe(2)
    })
  })

  describe('buildTree', () => {
    it('should rebuild a simple flat array into a tree', () => {
      const flatItems: FlattenedProjectTask[] = [
        {
          ...createMockTask('1', 'Task 1'),
          depth: 0,
          parentId: null,
          ancestorIds: [],
          flatIndex: 0,
        },
        {
          ...createMockTask('2', 'Task 2'),
          depth: 0,
          parentId: null,
          ancestorIds: [],
          flatIndex: 1,
        },
      ]

      const result = buildTree(flatItems)

      expect(result).toHaveLength(2)
      expect(result[0].id).toBe('1')
      expect(result[1].id).toBe('2')
      expect(result[0].children).toHaveLength(0)
      expect(result[1].children).toHaveLength(0)
    })

    it('should rebuild a nested tree structure', () => {
      const flatItems: FlattenedProjectTask[] = [
        {
          ...createMockTask('1', 'Task 1'),
          depth: 0,
          parentId: null,
          ancestorIds: [],
          flatIndex: 0,
        },
        {
          ...createMockTask('1-1', 'Task 1.1'),
          depth: 1,
          parentId: '1',
          ancestorIds: ['1'],
          flatIndex: 1,
        },
        {
          ...createMockTask('1-2', 'Task 1.2'),
          depth: 1,
          parentId: '1',
          ancestorIds: ['1'],
          flatIndex: 2,
        },
        {
          ...createMockTask('2', 'Task 2'),
          depth: 0,
          parentId: null,
          ancestorIds: [],
          flatIndex: 3,
        },
      ]

      const result = buildTree(flatItems)

      expect(result).toHaveLength(2)
      expect(result[0].id).toBe('1')
      expect(result[0].children).toHaveLength(2)
      expect(result[0].children?.[0].id).toBe('1-1')
      expect(result[0].children?.[1].id).toBe('1-2')
      expect(result[1].id).toBe('2')
      expect(result[1].children).toHaveLength(0)
    })

    it('should handle deep nesting', () => {
      const flatItems: FlattenedProjectTask[] = [
        {
          ...createMockTask('1', 'Task 1'),
          depth: 0,
          parentId: null,
          ancestorIds: [],
          flatIndex: 0,
        },
        {
          ...createMockTask('1-1', 'Task 1.1'),
          depth: 1,
          parentId: '1',
          ancestorIds: ['1'],
          flatIndex: 1,
        },
        {
          ...createMockTask('1-1-1', 'Task 1.1.1'),
          depth: 2,
          parentId: '1-1',
          ancestorIds: ['1', '1-1'],
          flatIndex: 2,
        },
      ]

      const result = buildTree(flatItems)

      expect(result).toHaveLength(1)
      expect(result[0].id).toBe('1')
      expect(result[0].children).toHaveLength(1)
      expect(result[0].children?.[0].id).toBe('1-1')
      expect(result[0].children?.[0].children).toHaveLength(1)
      expect(result[0].children?.[0].children?.[0].id).toBe('1-1-1')
    })
  })

  describe('validateMove', () => {
    it('should allow move to root level', () => {
      const activeTask: FlattenedProjectTask = {
        ...createMockTask('1', 'Task 1'),
        depth: 1,
        parentId: 'parent',
        ancestorIds: ['parent'],
        flatIndex: 0,
      }

      const result = validateMove(activeTask, null, null)

      expect(result.canMove).toBe(true)
    })

    it('should prevent task from being its own parent', () => {
      const activeTask: FlattenedProjectTask = {
        ...createMockTask('1', 'Task 1'),
        depth: 0,
        parentId: null,
        ancestorIds: [],
        flatIndex: 0,
      }

      const result = validateMove(activeTask, activeTask, '1')

      expect(result.canMove).toBe(false)
      expect(result.reason).toContain('own parent')
    })

    it('should prevent moving parent into its own descendant', () => {
      const activeTask: FlattenedProjectTask = {
        ...createMockTask('1', 'Task 1'),
        depth: 0,
        parentId: null,
        ancestorIds: [],
        flatIndex: 0,
      }

      const targetParent = createMockTask('1-1', 'Task 1.1')

      const result = validateMove(activeTask, targetParent, '1-1')

      // The active task's ancestor check won't catch this, but we should test the logic
      // In real scenario, the 1-1 task would have ancestorIds: ['1']
      const activeWithDescendant: FlattenedProjectTask = {
        ...createMockTask('1', 'Task 1'),
        depth: 0,
        parentId: null,
        ancestorIds: [],
        flatIndex: 0,
      }

      // Simulate that '1-1' is a descendant of '1'
      // This would be caught by checking if targetParent's ancestorIds includes activeTask.id
      const targetWithAncestors: FlattenedProjectTask = {
        ...createMockTask('1-1', 'Task 1.1'),
        depth: 1,
        parentId: '1',
        ancestorIds: ['1'],
        flatIndex: 1,
      }

      const resultWithAncestors = validateMove(
        activeWithDescendant,
        targetWithAncestors,
        '1-1',
      )

      expect(resultWithAncestors.canMove).toBe(false)
      expect(resultWithAncestors.reason).toContain('descendants')
    })

    it('should prevent moving into a milestone', () => {
      const activeTask: FlattenedProjectTask = {
        ...createMockTask('1', 'Task 1'),
        depth: 0,
        parentId: null,
        ancestorIds: [],
        flatIndex: 0,
      }

      const milestoneParent = createMockTask('2', 'Milestone', 'Milestone')

      const result = validateMove(activeTask, milestoneParent, '2')

      expect(result.canMove).toBe(false)
      expect(result.reason).toContain('Milestones cannot have child tasks')
    })

    it('should allow valid moves', () => {
      const activeTask: FlattenedProjectTask = {
        ...createMockTask('1', 'Task 1'),
        depth: 0,
        parentId: null,
        ancestorIds: [],
        flatIndex: 0,
      }

      const validParent = createMockTask('2', 'Task 2', 'Task')

      const result = validateMove(activeTask, validParent, '2')

      expect(result.canMove).toBe(true)
      expect(result.reason).toBeUndefined()
    })
  })

  describe('getProjection', () => {
    it('should calculate projection for same-level move', () => {
      const flatItems: FlattenedProjectTask[] = [
        {
          ...createMockTask('1', 'Task 1'),
          depth: 0,
          parentId: null,
          ancestorIds: [],
          flatIndex: 0,
        },
        {
          ...createMockTask('2', 'Task 2'),
          depth: 0,
          parentId: null,
          ancestorIds: [],
          flatIndex: 1,
        },
        {
          ...createMockTask('3', 'Task 3'),
          depth: 0,
          parentId: null,
          ancestorIds: [],
          flatIndex: 2,
        },
      ]

      // Move task 1 over task 3 with no horizontal offset (same depth)
      const projection = getProjection(flatItems, '1', '3', 0, INDENTATION_WIDTH)

      expect(projection.depth).toBe(0)
      expect(projection.parentId).toBeNull()
      expect(projection.canDrop).toBe(true)
    })

    it('should calculate projection for depth increase (make child)', () => {
      const flatItems: FlattenedProjectTask[] = [
        {
          ...createMockTask('1', 'Task 1'),
          depth: 0,
          parentId: null,
          ancestorIds: [],
          flatIndex: 0,
        },
        {
          ...createMockTask('2', 'Task 2'),
          depth: 0,
          parentId: null,
          ancestorIds: [],
          flatIndex: 1,
        },
      ]

      // Move task 2 over task 1 with positive horizontal offset (make child of 1)
      const projection = getProjection(
        flatItems,
        '2',
        '1',
        INDENTATION_WIDTH,
        INDENTATION_WIDTH,
      )

      expect(projection.depth).toBe(1)
      expect(projection.parentId).toBe('1')
      expect(projection.canDrop).toBe(true)
    })

    it('should prevent projection that creates circular reference', () => {
      const flatItems: FlattenedProjectTask[] = [
        {
          ...createMockTask('1', 'Task 1'),
          depth: 0,
          parentId: null,
          ancestorIds: [],
          flatIndex: 0,
        },
        {
          ...createMockTask('1-1', 'Task 1.1'),
          depth: 1,
          parentId: '1',
          ancestorIds: ['1'],
          flatIndex: 1,
        },
        {
          ...createMockTask('1-1-1', 'Task 1.1.1'),
          depth: 2,
          parentId: '1-1',
          ancestorIds: ['1', '1-1'],
          flatIndex: 2,
        },
      ]

      // Try to move parent (1) over its grandchild (1-1-1) with horizontal offset to become its child
      // This would make 1 a child of 1-1-1, which is a descendant of 1 - circular reference
      const projection = getProjection(
        flatItems,
        '1',
        '1-1-1',
        INDENTATION_WIDTH * 3, // Drag far right to attempt deep nesting
        INDENTATION_WIDTH,
      )

      expect(projection.canDrop).toBe(false)
      expect(projection.reason).toContain('descendants')
    })

    it('should prevent projection to milestone parent', () => {
      const flatItems: FlattenedProjectTask[] = [
        {
          ...createMockTask('1', 'Milestone 1', 'Milestone'),
          depth: 0,
          parentId: null,
          ancestorIds: [],
          flatIndex: 0,
        },
        {
          ...createMockTask('2', 'Task 2'),
          depth: 0,
          parentId: null,
          ancestorIds: [],
          flatIndex: 1,
        },
      ]

      // Try to make task 2 a child of milestone 1
      const projection = getProjection(
        flatItems,
        '2',
        '1',
        INDENTATION_WIDTH,
        INDENTATION_WIDTH,
      )

      expect(projection.canDrop).toBe(false)
      expect(projection.reason).toContain('Milestones')
    })

    it('should handle invalid task IDs gracefully', () => {
      const flatItems: FlattenedProjectTask[] = [
        {
          ...createMockTask('1', 'Task 1'),
          depth: 0,
          parentId: null,
          ancestorIds: [],
          flatIndex: 0,
        },
      ]

      const projection = getProjection(
        flatItems,
        'invalid',
        '1',
        0,
        INDENTATION_WIDTH,
      )

      expect(projection.canDrop).toBe(false)
      expect(projection.reason).toContain('not found')
    })

    it('should promote only child to sibling of parent (drag left)', () => {
      const flatItems: FlattenedProjectTask[] = [
        {
          ...createMockTask('1', 'Task 1'),
          depth: 0,
          parentId: null,
          ancestorIds: [],
          flatIndex: 0,
        },
        {
          ...createMockTask('1-1', 'Task 1.1'),
          depth: 1,
          parentId: '1',
          ancestorIds: ['1'],
          flatIndex: 1,
        },
      ]

      // Drag Task 1.1 (only child) left over itself to make it a sibling of Task 1
      const projection = getProjection(
        flatItems,
        '1-1',
        '1-1',
        -INDENTATION_WIDTH, // Drag left to decrease depth
        INDENTATION_WIDTH,
      )

      expect(projection.canDrop).toBe(true)
      expect(projection.depth).toBe(0) // Should become root level
      expect(projection.parentId).toBe(null) // No parent (root)
    })

    it('should promote last child to sibling of parent', () => {
      const flatItems: FlattenedProjectTask[] = [
        {
          ...createMockTask('1', 'Task 1'),
          depth: 0,
          parentId: null,
          ancestorIds: [],
          flatIndex: 0,
        },
        {
          ...createMockTask('1-1', 'Task 1.1'),
          depth: 1,
          parentId: '1',
          ancestorIds: ['1'],
          flatIndex: 1,
        },
        {
          ...createMockTask('1-2', 'Task 1.2'),
          depth: 1,
          parentId: '1',
          ancestorIds: ['1'],
          flatIndex: 2,
        },
      ]

      // Drag Task 1.2 (last child) left over itself
      const projection = getProjection(
        flatItems,
        '1-2',
        '1-2',
        -INDENTATION_WIDTH,
        INDENTATION_WIDTH,
      )

      expect(projection.canDrop).toBe(true)
      expect(projection.depth).toBe(0)
      expect(projection.parentId).toBe(null)
    })

    it('should promote deeply nested only child', () => {
      const flatItems: FlattenedProjectTask[] = [
        {
          ...createMockTask('1', 'Task 1'),
          depth: 0,
          parentId: null,
          ancestorIds: [],
          flatIndex: 0,
        },
        {
          ...createMockTask('1-1', 'Task 1.1'),
          depth: 1,
          parentId: '1',
          ancestorIds: ['1'],
          flatIndex: 1,
        },
        {
          ...createMockTask('1-1-1', 'Task 1.1.1'),
          depth: 2,
          parentId: '1-1',
          ancestorIds: ['1', '1-1'],
          flatIndex: 2,
        },
      ]

      // Drag Task 1.1.1 left to become sibling of 1.1 (depth 1, parent = '1')
      const projection = getProjection(
        flatItems,
        '1-1-1',
        '1-1-1',
        -INDENTATION_WIDTH,
        INDENTATION_WIDTH,
      )

      expect(projection.canDrop).toBe(true)
      expect(projection.depth).toBe(1) // One level up
      expect(projection.parentId).toBe('1') // Should have Task 1 as parent
    })
  })

  describe('calculateOrderInParent', () => {
    it('should calculate order for root level tasks (1-based)', () => {
      const flatItems: FlattenedProjectTask[] = [
        {
          ...createMockTask('1', 'Task 1'),
          depth: 0,
          parentId: null,
          ancestorIds: [],
          flatIndex: 0,
        },
        {
          ...createMockTask('2', 'Task 2'),
          depth: 0,
          parentId: null,
          ancestorIds: [],
          flatIndex: 1,
        },
        {
          ...createMockTask('3', 'Task 3'),
          depth: 0,
          parentId: null,
          ancestorIds: [],
          flatIndex: 2,
        },
      ]

      // Moving task '1' over task '2' (index 1) at root level
      const order = calculateOrderInParent(flatItems, '1', 1, null)

      expect(order).toBe(2) // Should be inserted after Task 2 (position 2)
    })

    it('should calculate order for child tasks (1-based)', () => {
      const flatItems: FlattenedProjectTask[] = [
        {
          ...createMockTask('1', 'Task 1'),
          depth: 0,
          parentId: null,
          ancestorIds: [],
          flatIndex: 0,
        },
        {
          ...createMockTask('1-1', 'Task 1.1'),
          depth: 1,
          parentId: '1',
          ancestorIds: ['1'],
          flatIndex: 1,
        },
        {
          ...createMockTask('1-2', 'Task 1.2'),
          depth: 1,
          parentId: '1',
          ancestorIds: ['1'],
          flatIndex: 2,
        },
        {
          ...createMockTask('1-3', 'Task 1.3'),
          depth: 1,
          parentId: '1',
          ancestorIds: ['1'],
          flatIndex: 3,
        },
      ]

      // Moving task '1-1' over task '1-2' (index 2)
      const order = calculateOrderInParent(flatItems, '1-1', 2, '1')

      expect(order).toBe(2) // Should be inserted after Task 1.2 (position 2)
    })

    it('should insert after when dropping on a task', () => {
      const flatItems: FlattenedProjectTask[] = [
        {
          ...createMockTask('1', 'Task 1'),
          depth: 0,
          parentId: null,
          ancestorIds: [],
          flatIndex: 0,
        },
      ]

      // Moving a new task (not in list) to root, over task '1' at index 0
      // Should insert AFTER task 1
      const order = calculateOrderInParent(flatItems, 'new-task', 0, null)

      expect(order).toBe(2) // Should be inserted after Task 1 (position 2)
    })
  })

  describe('updateTaskPlacement', () => {
    it('should update task parent and depth', () => {
      const flatItems: FlattenedProjectTask[] = [
        {
          ...createMockTask('1', 'Task 1'),
          depth: 0,
          parentId: null,
          ancestorIds: [],
          flatIndex: 0,
        },
        {
          ...createMockTask('2', 'Task 2'),
          depth: 0,
          parentId: null,
          ancestorIds: [],
          flatIndex: 1,
        },
      ]

      const result = updateTaskPlacement(flatItems, '2', '1', 1)

      expect(result[1]).toMatchObject({
        id: '2',
        depth: 1,
        parentId: '1',
        ancestorIds: ['1'],
      })

      // First item should remain unchanged
      expect(result[0]).toMatchObject({
        id: '1',
        depth: 0,
        parentId: null,
        ancestorIds: [],
      })
    })

    it('should update ancestorIds correctly for nested parents', () => {
      const flatItems: FlattenedProjectTask[] = [
        {
          ...createMockTask('1', 'Task 1'),
          depth: 0,
          parentId: null,
          ancestorIds: [],
          flatIndex: 0,
        },
        {
          ...createMockTask('1-1', 'Task 1.1'),
          depth: 1,
          parentId: '1',
          ancestorIds: ['1'],
          flatIndex: 1,
        },
        {
          ...createMockTask('2', 'Task 2'),
          depth: 0,
          parentId: null,
          ancestorIds: [],
          flatIndex: 2,
        },
      ]

      // Move task 2 to be child of 1-1
      const result = updateTaskPlacement(flatItems, '2', '1-1', 2)

      expect(result[2]).toMatchObject({
        id: '2',
        depth: 2,
        parentId: '1-1',
        ancestorIds: ['1', '1-1'],
      })
    })

    it('should handle moving to root level', () => {
      const flatItems: FlattenedProjectTask[] = [
        {
          ...createMockTask('1', 'Task 1'),
          depth: 0,
          parentId: null,
          ancestorIds: [],
          flatIndex: 0,
        },
        {
          ...createMockTask('1-1', 'Task 1.1'),
          depth: 1,
          parentId: '1',
          ancestorIds: ['1'],
          flatIndex: 1,
        },
      ]

      // Move task 1-1 to root
      const result = updateTaskPlacement(flatItems, '1-1', null, 0)

      expect(result[1]).toMatchObject({
        id: '1-1',
        depth: 0,
        parentId: null,
        ancestorIds: [],
      })
    })
  })

  describe('integration: flatten and rebuild', () => {
    it('should preserve tree structure through flatten and rebuild cycle', () => {
      const originalTree: ProjectTaskTreeDto[] = [
        createMockTask('1', 'Task 1', 'Task', [
          createMockTask('1-1', 'Task 1.1', 'Task', [
            createMockTask('1-1-1', 'Task 1.1.1'),
          ]),
          createMockTask('1-2', 'Task 1.2'),
        ]),
        createMockTask('2', 'Task 2'),
      ]

      const flattened = flattenTree(originalTree)
      const rebuilt = buildTree(flattened)

      // Check structure matches
      expect(rebuilt).toHaveLength(2)
      expect(rebuilt[0].id).toBe('1')
      expect(rebuilt[0].children).toHaveLength(2)
      expect(rebuilt[0].children?.[0].id).toBe('1-1')
      expect(rebuilt[0].children?.[0].children).toHaveLength(1)
      expect(rebuilt[0].children?.[0].children?.[0].id).toBe('1-1-1')
      expect(rebuilt[0].children?.[1].id).toBe('1-2')
      expect(rebuilt[1].id).toBe('2')
    })
  })
})
