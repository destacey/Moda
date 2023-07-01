'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { useCallback, useMemo, useState } from 'react'
import ModaGrid from '../../components/common/moda-grid'
import { getTeamsClient, getTeamsOfTeamsClient } from '@/src/services/clients'
import { ItemType } from 'antd/es/menu/hooks/useItems'
import { Button, Modal, Space, Switch } from 'antd'
import Link from 'next/link'
import { TeamListItem } from '../types'
import { useDocumentTitle } from '../../hooks/use-document-title'
import CreateTeamForm from '../../components/common/organizations/create-team-form'

const TeamLinkCellRenderer = ({ value, data }) => {
  const teamRoute = data.type === 'Team' ? 'teams' : 'team-of-teams'
  return (
    <Link href={`/organizations/${teamRoute}/${data.localId}`}>{value}</Link>
  )
}

const TeamOfTeamsLinkCellRenderer = ({ value, data }) => {
  return (
    <Link href={`/organizations/team-of-teams/${data.teamOfTeams?.localId}`}>
      {value}
    </Link>
  )
}

const TeamListPage = () => {
  useDocumentTitle('Teams')
  const [teams, setTeams] = useState<TeamListItem[]>([])
  const [includeDisabled, setIncludeDisabled] = useState<boolean>(false)
  const [openCreateTeamModal, setOpenCreateTeamModal] = useState<boolean>(false)

  const columnDefs = useMemo(
    () => [
      { field: 'localId', headerName: '#', width: 90 },
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
    []
  )

  const Actions = () => {
    return (
      <>
        <Button onClick={() => setOpenCreateTeamModal(true)}>
          Create Team
        </Button>
      </>
    )
  }

  const onIncludeDisabledChange = (checked: boolean) => {
    setIncludeDisabled(checked)
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

  const getTeams = useCallback(async () => {
    const teamsClient = await getTeamsClient()
    const teamsDtos = await teamsClient.getList(includeDisabled)
    const teamOfTeamsClient = await getTeamsOfTeamsClient()
    const teamOfTeamsDtos = await teamOfTeamsClient.getList(includeDisabled)
    const teamVMs = [
      ...(teamsDtos as TeamListItem[]),
      ...(teamOfTeamsDtos as TeamListItem[]),
    ]
    setTeams(teamVMs)
  }, [includeDisabled])

  const onCreateTeamFormClosed = (wasCreated: boolean) => {
    setOpenCreateTeamModal(false)
    if (wasCreated) {
      // TODO: refresh grid, what dependency is needed for the useCallback to trigger this?
    }
  }

  return (
    <>
      <PageTitle title="Teams" actions={<Actions />} />
      <ModaGrid
        columnDefs={columnDefs}
        gridControlMenuItems={controlItems}
        rowData={teams}
        loadData={getTeams}
      />
      <CreateTeamForm
        showForm={openCreateTeamModal}
        onFormCreate={() => onCreateTeamFormClosed(true)}
        onFormCancel={() => onCreateTeamFormClosed(false)}
      />
    </>
  )
}

export default TeamListPage
