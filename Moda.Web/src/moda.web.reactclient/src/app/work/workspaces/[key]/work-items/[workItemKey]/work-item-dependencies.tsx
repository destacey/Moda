'use client'

import { WorkItemDependenciesGrid } from '@/src/app/components/common/work'
import { WorkItemDetailsDto } from '@/src/services/moda-api'
import { useGetWorkItemDependenciesQuery } from '@/src/store/features/work-management/workspace-api'
import { useEffect } from 'react'

export interface WorkItemDependenciesProps {
  workItem: WorkItemDetailsDto
}

const WorkItemDependencies: React.FC<WorkItemDependenciesProps> = ({
  workItem,
}) => {
  const {
    data: dependencyData,
    isLoading,
    error,
    refetch,
  } = useGetWorkItemDependenciesQuery(
    {
      workspaceIdOrKey: workItem.workspace.key,
      workItemKey: workItem.key,
    },
    { skip: !workItem },
  )

  useEffect(() => {
    error && console.error(error)
  }, [error])

  return (
    <WorkItemDependenciesGrid
      workItem={workItem}
      dependencies={dependencyData}
      isLoading={isLoading}
      refetch={refetch}
    />
  )
}

export default WorkItemDependencies
