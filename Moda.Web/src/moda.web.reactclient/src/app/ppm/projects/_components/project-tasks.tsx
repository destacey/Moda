'use client'

import { ProjectDetailsDto } from '@/src/services/moda-api'
import { FC } from 'react'
import { ProjectTasksTable } from '.'
import { useGetProjectTaskTreeQuery } from '@/src/store/features/ppm/project-tasks-api'

export interface ProjectTasksProps {
  project: ProjectDetailsDto
  canManageTasks: boolean
}

const ProjectTasks: FC<ProjectTasksProps> = ({
  project,
  canManageTasks,
}: ProjectTasksProps) => {
  const {
    data: tasksData,
    isLoading: tasksDataIsLoading,
    refetch: refetchTasksData,
  } = useGetProjectTaskTreeQuery(
    { projectIdOrKey: project?.key || '' },
    { skip: !project?.key },
  )

  if (!project) return null

  return (
    <ProjectTasksTable
      projectKey={project.key}
      tasks={tasksData}
      isLoading={tasksDataIsLoading}
      canManageTasks={canManageTasks}
      refetch={refetchTasksData}
    />
  )
}

export default ProjectTasks

