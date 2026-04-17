'use client'

import { ModaGrid } from '@/src/components/common'
import { ProjectTeamMemberDto } from '@/src/services/moda-api'
import { useGetProjectTeamQuery } from '@/src/store/features/ppm/projects-api'
import { ColDef } from 'ag-grid-community'
import { CustomCellRendererProps } from 'ag-grid-react'
import Link from 'next/link'
import { FC } from 'react'

const PersonLinkCellRenderer = (
  props: CustomCellRendererProps<ProjectTeamMemberDto>,
) => {
  if (!props.data) return null
  return (
    <Link
      href={`/organizations/employees/${props.data.employee.key}`}
      prefetch={false}
    >
      {props.data.employee.name}
    </Link>
  )
}

const columnDefs: ColDef<ProjectTeamMemberDto>[] = [
  {
    field: 'employee.name',
    headerName: 'Person',
    cellRenderer: PersonLinkCellRenderer,
    minWidth: 200,
    flex: 1,
  },
  {
    field: 'roles',
    headerName: 'Roles',
    minWidth: 200,
    flex: 1,
    valueGetter: (params) => params.data?.roles?.join(', '),
  },
  {
    field: 'assignedPhases',
    headerName: 'Assigned Phases',
    minWidth: 200,
    flex: 1,
    valueGetter: (params) => params.data?.assignedPhases?.join(', ') || null,
  },
  {
    field: 'activeWorkItemCount',
    headerName: 'Active Tasks',
    width: 130,
  },
]

interface ProjectTeamGridProps {
  projectIdOrKey: string
}

const ProjectTeamGrid: FC<ProjectTeamGridProps> = ({ projectIdOrKey }) => {
  const {
    data: teamData,
    isLoading,
    refetch,
  } = useGetProjectTeamQuery(projectIdOrKey)

  const refresh = async () => {
    refetch()
  }

  return (
    <ModaGrid
      columnDefs={columnDefs}
      rowData={teamData}
      loadData={refresh}
      loading={isLoading}
      emptyMessage="No team members assigned."
    />
  )
}

export default ProjectTeamGrid
