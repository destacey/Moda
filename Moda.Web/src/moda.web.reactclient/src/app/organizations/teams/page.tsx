'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { useEffect, useMemo } from 'react'
import ModaGrid from '../../components/common/moda-grid'
import { ItemType } from 'antd/es/menu/hooks/useItems'
import { Button, Space, Switch } from 'antd'
import { useDocumentTitle } from '../../hooks/use-document-title'
import useAuth from '../../components/contexts/auth'
import { useAppSelector, useAppDispatch } from '../../hooks'
import {
  retrieveTeams,
  setIncludeInactive,
  setEditMode,
  selectTeamsContext,
  selectTeamIsInEditMode,
} from '../../../store/features/organizations/team-slice'
import { ModalCreateTeamForm } from '../components/create-team-form'
import {
  NestedTeamOfTeamsNameLinkCellRenderer,
  TeamNameLinkCellRenderer,
} from '../../components/common/moda-grid-cell-renderers'
import { TeamListItem } from '../types'
import { ColDef } from 'ag-grid-community'

const TeamListPage = () => {
  useDocumentTitle('Teams')
  const { data: teams, isLoading, error } = useAppSelector(selectTeamsContext)
  const isInEditMode = useAppSelector(selectTeamIsInEditMode)
  const includeDisabled = useAppSelector((state) => state.team.includeInactive)

  const columnDefs = useMemo<ColDef<TeamListItem>[]>(
    () => [
      { field: 'key', width: 90 },
      { field: 'name', cellRenderer: TeamNameLinkCellRenderer },
      { field: 'code', width: 125 },
      { field: 'type' },
      {
        field: 'teamOfTeams.name',
        headerName: 'Team of Teams',
        cellRenderer: NestedTeamOfTeamsNameLinkCellRenderer,
      },
      { field: 'isActive' }, // TODO: convert to yes/no
    ],
    [],
  )

  const dispatch = useAppDispatch()

  const { hasClaim } = useAuth()
  const canCreateTeam = hasClaim('Permission', 'Permissions.Teams.Create')
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

  useEffect(() => {
    error && console.error(error)
  }, [error])

  const onIncludeDisabledChange = (checked: boolean) => {
    dispatch(setIncludeInactive(checked))
    dispatch(retrieveTeams())
  }

  const controlItems: ItemType[] = [
    {
      label: (
        <Space>
          <Switch
            size="small"
            checked={includeDisabled}
            onChange={onIncludeDisabledChange}
          />
          Include Disabled
        </Space>
      ),
      key: '0',
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
          dispatch(retrieveTeams())
        }}
        isDataLoading={isLoading}
      />
      {isInEditMode && <ModalCreateTeamForm />}
    </>
  )
}

export default TeamListPage
