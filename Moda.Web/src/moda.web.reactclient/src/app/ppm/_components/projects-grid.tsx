'use client'

import { ModaGrid } from '@/src/components/common'
import {
  PortfolioLinkCellRenderer,
  ProjectLinkCellRenderer,
} from '@/src/components/common/moda-grid-cell-renderers'
import { ProjectListDto } from '@/src/services/moda-api'
import { getSortedNames } from '@/src/utils'
import { ColDef } from 'ag-grid-community'
import { MessageInstance } from 'antd/es/message/interface'
import dayjs from 'dayjs'
import { useCallback, useMemo } from 'react'

export interface ProjectsGridProps {
  projects: ProjectListDto[]
  isLoading: boolean
  refetch: () => void
  messageApi: MessageInstance
  hidePortfolio?: boolean
}

const ProjectsGrid: React.FC<ProjectsGridProps> = (
  props: ProjectsGridProps,
) => {
  const { refetch } = props

  const columnDefs = useMemo<ColDef<ProjectListDto>[]>(
    () => [
      { field: 'key', width: 90 },
      { field: 'name', cellRenderer: ProjectLinkCellRenderer, width: 300 },
      { field: 'status.name', headerName: 'Status', width: 125 },
      {
        field: 'portfolio.name',
        headerName: 'Portfolio',
        width: 200,
        hide: props.hidePortfolio,
        cellRenderer: (params) =>
          PortfolioLinkCellRenderer({ data: params.data.portfolio }),
      },
      {
        field: 'start',
        width: 125,
        valueGetter: (params) =>
          params.data.start && dayjs(params.data.start).format('M/D/YYYY'),
      },
      {
        field: 'end',
        width: 125,
        valueGetter: (params) =>
          params.data.end && dayjs(params.data.end).format('M/D/YYYY'),
      },
      {
        field: 'projectSponsors',
        headerName: 'Sponsors',
        valueGetter: (params) => getSortedNames(params.data.projectSponsors),
      },
      {
        field: 'projectOwners',
        headerName: 'Owners',
        valueGetter: (params) => getSortedNames(params.data.projectOwners),
      },
      {
        field: 'projectManagers',
        headerName: 'Managers',
        valueGetter: (params) => getSortedNames(params.data.projectManagers),
      },
      {
        field: 'strategicThemes',
        headerName: 'Strategic Themes',
        valueGetter: (params) => getSortedNames(params.data.strategicThemes),
      },
    ],
    [],
  )

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  return (
    <>
      <ModaGrid
        columnDefs={columnDefs}
        rowData={props.projects}
        loadData={refresh}
        loading={props.isLoading}
        emptyMessage="No projects found."
      />
    </>
  )
}

export default ProjectsGrid
