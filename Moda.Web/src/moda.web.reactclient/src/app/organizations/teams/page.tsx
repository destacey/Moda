'use client'

import PageTitle from '@/src/components/common/page-title'
import { useMemo } from 'react'
import ModaGrid from '../../../components/common/moda-grid'
import { ItemType } from 'antd/es/menu/interface'
import { Button } from 'antd'
import { useDocumentTitle } from '../../../hooks/use-document-title'
import useAuth from '../../../components/contexts/auth'
import { useAppSelector, useAppDispatch } from '../../../hooks'
import {
  setIncludeInactive,
  setEditMode,
  selectTeamIsInEditMode,
} from '../../../store/features/organizations/team-slice'
import { useGetTeamsQuery } from '../../../store/features/organizations/team-api'
import { ModalCreateTeamForm } from '../_components/create-team-form'
import {
  NestedTeamOfTeamsNameLinkCellRenderer,
  TeamNameLinkCellRenderer,
} from '../../../components/common/moda-grid-cell-renderers'
import { TeamListItem } from '../types'
import { ColDef } from 'ag-grid-community'
import { ControlItemSwitch } from '../../../components/common/control-items-menu'
import { authorizePage } from '../../../components/hoc'

const TeamListPage = () => {
  useDocumentTitle('Teams')
  const includeInactive = useAppSelector((state) => state.team.includeInactive)
  const {
    data: teams,
    isLoading,
    error,
    refetch,
  } = useGetTeamsQuery(includeInactive)
  const isInEditMode = useAppSelector(selectTeamIsInEditMode)

  const columnDefs = useMemo<ColDef<TeamListItem>[]>(
    () => [
      { field: 'key', width: 90 },
      {
        field: 'name',
        cellRenderer: TeamNameLinkCellRenderer,
        width: 250,
        comparator: (valueA, valueB) => {
          return valueA.toLowerCase().localeCompare(valueB.toLowerCase())
        },
      },
      { field: 'code', width: 125 },
      { field: 'type' },
      {
        field: 'teamOfTeams.name',
        headerName: 'Team of Teams',
        cellRenderer: NestedTeamOfTeamsNameLinkCellRenderer,
        width: 250,
        comparator: (valueA, valueB) => {
          if (!valueA && !valueB) return 0
          if (!valueA) return 1
          if (!valueB) return -1
          return valueA.toLowerCase().localeCompare(valueB.toLowerCase())
        },
      },
      { field: 'isActive' }, // TODO: convert to yes/no
    ],
    [],
  )

  const dispatch = useAppDispatch()

  const { hasPermissionClaim } = useAuth()
  const canCreateTeam = hasPermissionClaim('Permissions.Teams.Create')
  const showActions = canCreateTeam

  const actions = () => {
    if (!showActions) return null
    return (
      <>
        {canCreateTeam && (
          <Button onClick={() => dispatch(setEditMode(true))}>
            Create Team
          </Button>
        )}
      </>
    )
  }

  const onIncludeInactiveChange = (checked: boolean) => {
    dispatch(setIncludeInactive(checked))
  }

  const controlItems: ItemType[] = [
    {
      label: (
        <ControlItemSwitch
          label="Include Inactive"
          checked={includeInactive}
          onChange={onIncludeInactiveChange}
        />
      ),
      key: 'include-inactive',
      onClick: () => onIncludeInactiveChange(!includeInactive),
    },
  ]

  return (
    <>
      <PageTitle title="Teams" actions={actions()} />
      <ModaGrid
        columnDefs={columnDefs}
        gridControlMenuItems={controlItems}
        rowData={teams}
        loadData={() => {
          refetch()
        }}
        loading={isLoading}
        initialState={{
          sort: {
            sortModel: [{ colId: 'name', sort: 'asc' }],
          },
        }}
      />
      {isInEditMode && <ModalCreateTeamForm />}
    </>
  )
}

const TeamListPageWithAuthorization = authorizePage(
  TeamListPage,
  'Permission',
  'Permissions.Teams.View',
)

export default TeamListPageWithAuthorization
