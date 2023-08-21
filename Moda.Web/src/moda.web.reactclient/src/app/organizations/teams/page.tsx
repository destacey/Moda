'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { useMemo } from 'react'
import ModaGrid from '../../components/common/moda-grid'
import { ItemType } from 'antd/es/menu/hooks/useItems'
import { Button, Space, Switch } from 'antd'
import Link from 'next/link'
import { useDocumentTitle } from '../../hooks/use-document-title'
import { CreateTeamForm } from '../components'
import useAuth from '../../components/contexts/auth'
import { useAppSelector, useAppDispatch } from '../../hooks'
import { retrieveTeams, setIncludeDisabled, setCreateTeamOpen } from '../teamsSlice'
import { ModalCreateTeamForm } from '../components/create-team-form'

const TeamLinkCellRenderer = ({ value, data }) => {
  const teamRoute = data.type === 'Team' ? 'teams' : 'team-of-teams'
  return (
    <Link href={`/organizations/${teamRoute}/${data.key}`}>{value}</Link>
  )
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
  const teams = useAppSelector((state) => state.teams.teams)
  const teamsLoadingstatus = useAppSelector((state) => state.teams.isLoading)
  const includeDisabled = useAppSelector((state) => state.teams.includeInactiveTeams)
  const createTeamOpen = useAppSelector((state) => state.teams.createTeam.isOpen)

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

  const Actions = () => {
    if (!showActions) return null
    return (
      <>
        {canCreateTeam && (
          <Button onClick={() => dispatch(setCreateTeamOpen(true))}>
            Create Team
          </Button>
        )}
      </>
    )
  }

  const onIncludeDisabledChange = (checked: boolean) => {
    dispatch(setIncludeDisabled(checked))
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
      <PageTitle title="Teams" actions={<Actions />} />
      <ModaGrid
        columnDefs={columnDefs}
        gridControlMenuItems={controlItems}
        rowData={teams}
        loadData={() => {dispatch(retrieveTeams())}}
        isDataLoading={teamsLoadingstatus}
      />
      ({ createTeamOpen && 
      <ModalCreateTeamForm />})
    </>
  )
}

export default TeamListPage
