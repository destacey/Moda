import Link from 'next/link'
import { useCallback, useMemo, useState } from 'react'
import ModaGrid from '../moda-grid'
import { RiskListDto } from '@/src/services/moda-api'
import { ItemType } from 'antd/es/menu/hooks/useItems'
import { Button, Space, Switch } from 'antd'
import dayjs from 'dayjs'
import useAuth from '../../contexts/auth'
import CreateRiskForm from './create-risk-form'

export interface RisksGridProps {
  getRisks: (id: string, includeClosed: boolean) => Promise<RiskListDto[]>
  getRisksObjectId: string
  teamId?: string | null
  newRisksAllowed?: boolean
  hideTeamColumn?: boolean
}

const RiskLinkCellRenderer = ({ value, data }) => {
  return <Link href={`/planning/risks/${data.localId}`}>{value}</Link>
}

const TeamLinkCellRenderer = ({ value, data }) => {
  const teamRoute = data.team?.type === 'Team' ? 'teams' : 'team-of-teams'
  return (
    <Link href={`/organizations/${teamRoute}/${data.team?.localId}`}>
      {value}
    </Link>
  )
}

const AssigneeLinkCellRenderer = ({ value, data }) => {
  return (
    <Link href={`/organizations/employees/${data.assignee?.localId}`}>
      {value}
    </Link>
  )
}

const RisksGrid = ({
  getRisks,
  getRisksObjectId,
  teamId,
  newRisksAllowed = false,
  hideTeamColumn = false,
}: RisksGridProps) => {
  const [risks, setRisks] = useState<RiskListDto[]>()
  const [hideTeam, setHideTeam] = useState<boolean>(hideTeamColumn)
  const [openCreateRiskForm, setOpenCreateRiskForm] = useState<boolean>(false)

  const { hasClaim } = useAuth()
  const canCreateRisks = hasClaim('Permission', 'Permissions.Risks.Create')
  const showActions = newRisksAllowed && canCreateRisks

  const onHideTeamChange = (checked: boolean) => {
    setHideTeam(checked)
  }

  // TODO: prevent from reloading if already loaded unless refresh button is clicked
  const loadRisks = useCallback(async () => {
    const riskDtos = await getRisks(getRisksObjectId, false)
    setRisks(riskDtos)
  }, [getRisks, getRisksObjectId])

  const Actions = () => {
    return (
      <>
        {canCreateRisks && (
          <Button onClick={() => setOpenCreateRiskForm(true)}>
            Create Risk
          </Button>
        )}
      </>
    )
  }

  const controlItems: ItemType[] = [
    {
      label: (
        <>
          <Space direction="vertical" size="small">
            <Space>
              <Switch
                size="small"
                checked={hideTeam}
                onChange={onHideTeamChange}
              />
              Hide Team
            </Space>
          </Space>
        </>
      ),
      key: '0',
    },
  ]

  // TODO: dates are formatted correctly and filter, but the filter is string based, not date based
  const columnDefs = useMemo(
    () => [
      { field: 'localId', headerName: '#', width: 90 },
      { field: 'summary', width: 300, cellRenderer: RiskLinkCellRenderer },
      {
        field: 'team.name',
        cellRenderer: TeamLinkCellRenderer,
        hide: hideTeam,
      },
      { field: 'category', width: 125 },
      { field: 'exposure', width: 125 },
      {
        field: 'followUp',
        valueGetter: (params) =>
          params.data.followUpDate
            ? dayjs(params.data.followUpDate).format('M/D/YYYY')
            : null,
      },
      { field: 'assignee.name', cellRenderer: AssigneeLinkCellRenderer },
      {
        field: 'reportedOn',
        valueGetter: (params) =>
          dayjs(params.data.reportedOn).format('M/D/YYYY'),
      },
    ],
    [hideTeam]
  )

  const onCreateRiskFormClosed = (wasCreated: boolean) => {
    setOpenCreateRiskForm(false)
    if (wasCreated) {
      loadRisks()
    }
  }

  return (
    <>
      {/* TODO:  setup dynamic height */}
      <ModaGrid
        height={550}
        columnDefs={columnDefs}
        rowData={risks}
        loadData={loadRisks}
        actions={showActions && <Actions />}
        gridControlMenuItems={controlItems}
      />
      <CreateRiskForm
        createForTeamId={teamId}
        showForm={openCreateRiskForm}
        onFormCreate={() => onCreateRiskFormClosed(true)}
        onFormCancel={() => onCreateRiskFormClosed(false)}
      />
    </>
  )
}

export default RisksGrid
