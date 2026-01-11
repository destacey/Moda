'use client'

import { ModaGrid } from '@/src/components/common'
import {
  LifecycleStatusTagCellRenderer,
  PortfolioLinkCellRenderer,
  ProgramLinkCellRenderer,
  ProjectLinkCellRenderer,
} from '@/src/components/common/moda-grid-cell-renderers'
import { ProjectListDto } from '@/src/services/moda-api'
import { getSortedNames } from '@/src/utils'
import { ColDef } from 'ag-grid-community'
import dayjs from 'dayjs'
import { useCallback, useMemo } from 'react'

export interface ProjectsGridProps {
  projects: ProjectListDto[]
  isLoading: boolean
  refetch: () => void
  hidePortfolio?: boolean
  hideProgram?: boolean
  gridHeight?: number | undefined
  viewSelector?: React.ReactNode | undefined
}

const ProjectsGrid: React.FC<ProjectsGridProps> = (
  props: ProjectsGridProps,
) => {
  const { refetch } = props

  const columnDefs = useMemo<ColDef<ProjectListDto>[]>(
    () => [
      { field: 'key', width: 125 },
      {
        field: 'name',
        cellRenderer: ProjectLinkCellRenderer,
        width: 300,
        initialSort: 'asc',
      },
      {
        field: 'status.name',
        headerName: 'Status',
        width: 125,
        cellRenderer: LifecycleStatusTagCellRenderer,
      },
      {
        field: 'portfolio.name',
        headerName: 'Portfolio',
        width: 200,
        hide: props.hidePortfolio,
        cellRenderer: (params) =>
          PortfolioLinkCellRenderer({ ...params, data: params.data.portfolio }),
      },
      {
        field: 'program.name',
        headerName: 'Program',
        width: 200,
        hide: props.hideProgram,
        cellRenderer: (params) =>
          params.data.program &&
          ProgramLinkCellRenderer({ ...params, data: params.data.program }),
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
    [props.hidePortfolio, props.hideProgram],
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
        toolbarActions={props.viewSelector}
        height={props.gridHeight}
        emptyMessage="No projects found."
      />
    </>
  )
}

export default ProjectsGrid
