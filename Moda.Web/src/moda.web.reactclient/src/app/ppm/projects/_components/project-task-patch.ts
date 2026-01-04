export type ProjectTaskUpdate = {
  name?: string
  description?: string
  statusId?: number
  priorityId?: number
  progress?: number
  plannedStart?: string | null
  plannedEnd?: string | null
  plannedDate?: string | null
  estimatedEffortHours?: number | null
}

export type JsonPatchReplaceOperation = {
  op: 'replace'
  path: string
  value: unknown
}

// Note: ASP.NET Core JsonPatch expects PascalCase paths matching the C# property names
export const buildProjectTaskPatchOperations = (
  updates: ProjectTaskUpdate,
): JsonPatchReplaceOperation[] => {
  const patchOperations: JsonPatchReplaceOperation[] = []

  if (updates.name !== undefined) {
    patchOperations.push({ op: 'replace', path: '/Name', value: updates.name })
  }
  if (updates.description !== undefined) {
    patchOperations.push({
      op: 'replace',
      path: '/Description',
      value: updates.description,
    })
  }
  if (updates.statusId !== undefined) {
    patchOperations.push({
      op: 'replace',
      path: '/StatusId',
      value: updates.statusId,
    })
  }
  if (updates.priorityId !== undefined) {
    patchOperations.push({
      op: 'replace',
      path: '/PriorityId',
      value: updates.priorityId,
    })
  }
  if (updates.progress !== undefined) {
    patchOperations.push({
      op: 'replace',
      path: '/Progress',
      value: updates.progress,
    })
  }
  if (updates.plannedStart !== undefined) {
    patchOperations.push({
      op: 'replace',
      path: '/PlannedStart',
      value: updates.plannedStart,
    })
  }
  if (updates.plannedEnd !== undefined) {
    patchOperations.push({
      op: 'replace',
      path: '/PlannedEnd',
      value: updates.plannedEnd,
    })
  }
  if (updates.plannedDate !== undefined) {
    patchOperations.push({
      op: 'replace',
      path: '/PlannedDate',
      value: updates.plannedDate,
    })
  }
  if (updates.estimatedEffortHours !== undefined) {
    patchOperations.push({
      op: 'replace',
      path: '/EstimatedEffortHours',
      value: updates.estimatedEffortHours,
    })
  }

  return patchOperations
}
