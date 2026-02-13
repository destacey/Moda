import { ProjectTaskTreeDto } from '@/src/services/moda-api'

export interface DraftTask {
  id: string
  parentId?: string
  order: number
}

// Merges in-memory draft rows into the existing task tree for inline creation.
export const mergeDraftTasksIntoTree = (
  tasks: ProjectTaskTreeDto[],
  draftTasks: DraftTask[],
): ProjectTaskTreeDto[] => {
  if (draftTasks.length === 0) return tasks

  const createDraftTaskDto = (draft: DraftTask): ProjectTaskTreeDto => ({
    id: draft.id,
    projectId: '',
    key: '',
    wbs: '',
    name: '',
    type: { id: 1, name: 'Task' },
    status: { id: 1, name: 'Not Started' },
    priority: { id: 2, name: 'Medium' },
    assignees: [],
    progress: 0,
    order: draft.order,
    children: [],
  })

  const insertDraftsAtLevel = (
    levelTasks: ProjectTaskTreeDto[],
    parentId: string | undefined,
  ): ProjectTaskTreeDto[] => {
    const processedTasks = levelTasks.map((task) => {
      const currentChildren = task.children ?? []
      const nextChildren = insertDraftsAtLevel(currentChildren, task.id)

      if (
        nextChildren.length === currentChildren.length &&
        nextChildren.every((child, index) => child === currentChildren[index])
      ) {
        return task
      }

      return {
        ...task,
        children: nextChildren,
      }
    })

    const draftsForLevel = draftTasks.filter((d) => d.parentId === parentId)
    if (draftsForLevel.length > 0) {
      return [
        ...processedTasks,
        ...draftsForLevel.map((draft) => createDraftTaskDto(draft)),
      ]
    }

    return processedTasks
  }

  return insertDraftsAtLevel(tasks, undefined)
}
