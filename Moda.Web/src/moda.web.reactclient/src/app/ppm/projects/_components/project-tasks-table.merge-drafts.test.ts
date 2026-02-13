import { ProjectTaskTreeDto } from '@/src/services/moda-api'
import {
  DraftTask,
  mergeDraftTasksIntoTree,
} from './project-tasks-table.merge-drafts'

const makeTask = (
  id: string,
  children: ProjectTaskTreeDto[] = [],
): ProjectTaskTreeDto =>
  ({
    id,
    projectId: 'proj-1',
    key: id.toUpperCase(),
    wbs: id,
    name: `Task ${id}`,
    type: { id: 1, name: 'Task' },
    status: { id: 1, name: 'Not Started' },
    priority: { id: 2, name: 'Medium' },
    assignees: [],
    progress: 0,
    order: 0,
    children,
  }) as ProjectTaskTreeDto

describe('mergeDraftTasksIntoTree', () => {
  it('returns original tasks reference when there are no draft tasks', () => {
    const tasks = [makeTask('a')]

    const result = mergeDraftTasksIntoTree(tasks, [])

    expect(result).toBe(tasks)
  })

  it('adds root-level draft tasks at the end of the root collection', () => {
    const tasks = [makeTask('a'), makeTask('b')]
    const drafts: DraftTask[] = [{ id: 'draft-1', order: 0 }]

    const result = mergeDraftTasksIntoTree(tasks, drafts)

    expect(result).toHaveLength(3)
    expect(result.map((t) => t.id)).toEqual(['a', 'b', 'draft-1'])
    expect(result[2].name).toBe('')
    expect(result[2].type?.name).toBe('Task')
  })

  it('adds a child draft under a parent even when that parent originally has no children', () => {
    const tasks = [makeTask('parent'), makeTask('sibling')]
    const drafts: DraftTask[] = [{ id: 'draft-child', parentId: 'parent', order: 0 }]

    const result = mergeDraftTasksIntoTree(tasks, drafts)
    const parent = result.find((t) => t.id === 'parent')

    expect(parent?.children?.map((c) => c.id)).toEqual(['draft-child'])
    expect(result.find((t) => t.id === 'sibling')).toBe(tasks[1])
  })

  it('preserves existing children order and appends child drafts at that level', () => {
    const child1 = makeTask('child-1')
    const child2 = makeTask('child-2')
    const parent = makeTask('parent', [child1, child2])
    const tasks = [parent]
    const drafts: DraftTask[] = [{ id: 'draft-child', parentId: 'parent', order: 0 }]

    const result = mergeDraftTasksIntoTree(tasks, drafts)
    const mergedParent = result[0]

    expect(mergedParent.children?.map((c) => c.id)).toEqual([
      'child-1',
      'child-2',
      'draft-child',
    ])
    expect(parent.children).toEqual([child1, child2])
    expect(mergedParent).not.toBe(parent)
  })
})
