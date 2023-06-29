'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { useCallback, useMemo, useState } from 'react'
import ModaGrid from '../../components/common/moda-grid'
import { getTeamsClient, getTeamsOfTeamsClient } from '@/src/services/clients'
import { ItemType } from 'antd/es/menu/hooks/useItems'
import { Space, Switch } from 'antd'
import Link from 'next/link'
import { TeamListItem } from '../types'
import { useDocumentTitle } from '../../hooks/use-document-title'

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

  return (
    <>
      <PageTitle title="Teams" />
      <ModaGrid
        columnDefs={columnDefs}
        gridControlMenuItems={controlItems}
        rowData={teams}
        loadData={getTeams}
      />
    </>
  )
}

export default TeamListPage
