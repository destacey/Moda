import { ProjectTaskTreeDto } from '@/src/services/moda-api'

export const countProjectTasks = (tasks: ProjectTaskTreeDto[]): number => {
  const count = (items: ProjectTaskTreeDto[]): number =>
    items.reduce(
      (acc, item) =>
        acc + 1 + (item.children?.length ? count(item.children) : 0),
      0,
    )

  return count(tasks)
}

export const findProjectTaskById = (
  tasks: ProjectTaskTreeDto[],
  id: string,
): ProjectTaskTreeDto | null => {
  for (const task of tasks) {
    if (task.id === id) return task
    if (task.children?.length) {
      const found = findProjectTaskById(task.children, id)
      if (found) return found
    }
  }

  return null
}
