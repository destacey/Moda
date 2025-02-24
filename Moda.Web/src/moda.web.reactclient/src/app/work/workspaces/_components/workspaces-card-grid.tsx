'use client'

import { WorkspaceListDto } from '@/src/services/moda-api'
import { Flex, List } from 'antd'
import { ReactElement, useMemo } from 'react'
import { WorkspaceCard } from '.'
import { ModaEmpty } from '@/src/components/common'

const { Item: ListItem } = List

export interface WorkspacesCardGridProps {
  workspaces: WorkspaceListDto[]
  viewSelector: ReactElement
  isLoading: boolean
}

const gridConfig = {
  gutter: 16,
  xs: 1,
  sm: 2,
  md: 3,
  lg: 3,
  xl: 4,
  xxl: 4,
}

const WorkspacesCardGrid: React.FC<WorkspacesCardGridProps> = (
  props: WorkspacesCardGridProps,
) => {
  const sortedWorkspaces = useMemo<WorkspaceListDto[]>(() => {
    if (!props.workspaces || props.workspaces.length === 0) {
      return []
    }
    return [...props.workspaces].sort((a, b) => a.name.localeCompare(b.name))
  }, [props.workspaces])

  return (
    <>
      <Flex justify="end" align="center" style={{ paddingBottom: '16px' }}>
        {props.viewSelector}
      </Flex>
      <List
        grid={gridConfig}
        loading={{
          spinning: props.isLoading,
          tip: 'Loading workspaces...',
          size: 'large',
        }}
        locale={{ emptyText: <ModaEmpty message="No workspaces found" /> }}
        dataSource={sortedWorkspaces}
        renderItem={(item: WorkspaceListDto) => (
          <ListItem>
            <WorkspaceCard workspace={item} />
          </ListItem>
        )}
      />
    </>
  )
}

export default WorkspacesCardGrid
