'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { useEffect, useMemo } from 'react'
import ModaGrid from '../../components/common/moda-grid'
import { ItemType } from 'antd/es/menu/hooks/useItems'
import { Button, Space, Switch } from 'antd'
import Link from 'next/link'
import { useDocumentTitle } from '../../hooks/use-document-title'
import useAuth from '../../components/contexts/auth'
import { useAppSelector, useAppDispatch } from '../../hooks'
import {
  retrieveTeams,
  setIncludeInactive,
  setEditMode,
  selectTeamsContext,
  selectTeamIsInEditMode,
} from '../team-slice'
import { ModalCreateTeamForm } from '../components/create-team-form'

const TeamLinkCellRenderer = ({ value, data }) => {
  const teamRoute = data.type === 'Team' ? 'teams' : 'team-of-teams'
  return <Link href={`/organizations/${teamRoute}/${data.key}`}>{value}</Link>
}

const TeamOfTeamsLinkCellRenderer = ({ value, data }) => {
  return (
    <Link href={`/organizations/team-of-teams/${data.teamOfTeams?.key}`}>
      {value}
    </Link>
  )
}

const TeamListPage = () => {
  useDocumentTitle('Teams')
  const { data: teams, isLoading, error } = useAppSelector(selectTeamsContext)
  const isInEditMode = useAppSelector(selectTeamIsInEditMode)
  const includeDisabled = useAppSelector((state) => state.team.includeInactive)

  const dispatch = useAppDispatch()

  const { hasClaim } = useAuth()
  const canCreateTeam = hasClaim('Permission', 'Permissions.Teams.Create')
  const showActions = canCreateTeam

  const columnDefs = useMemo(
    () => [
      { field: 'key', width: 90 },
      { field: 'name', cellRenderer: TeamLinkCellRenderer },
      { field: 'code', width: 125 },
      { field: 'type' },
      {
        field: 'teamOfTeams.name',
        headerName: 'Team of Teams',
        cellRenderer: TeamOfTeamsLinkCellRenderer,
      },
      { field: 'isActive' }, // TODO: convert to yes/no
    ],
    [],
  )

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
      ({isInEditMode && <ModalCreateTeamForm />})
    </>
  )
}

export default TeamListPage
