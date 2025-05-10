import Link from 'next/link'
import { useCallback, useMemo, useState } from 'react'
import ModaGrid from '../moda-grid'
import { RiskListDto } from '@/src/services/moda-api'
import { ItemType } from 'antd/es/menu/interface'
import { Button } from 'antd'
import dayjs from 'dayjs'
import useAuth from '../../contexts/auth'
import CreateRiskForm from './create-risk-form'
import { EditOutlined } from '@ant-design/icons'
import EditRiskForm from './edit-risk-form'
import { NestedTeamNameLinkCellRenderer } from '../moda-grid-cell-renderers'
import { ColDef } from 'ag-grid-community'
import { ControlItemSwitch } from '../control-items-menu'

export interface RisksGridProps {
  risks: RiskListDto[]
  updateIncludeClosed: (includeClosed: boolean) => void
  teamId?: string
  isLoadingRisks: boolean
  refreshRisks: () => void
  newRisksAllowed?: boolean
  hideTeamColumn?: boolean
  gridHeight?: number
}

const RiskLinkCellRenderer = ({ value, data }) => {
  return <Link href={`/planning/risks/${data.key}`}>{value}</Link>
}

const AssigneeLinkCellRenderer = ({ value, data }) => {
  return (
    <Link href={`/organizations/employees/${data.assignee?.key}`}>{value}</Link>
  )
}

const RisksGrid = ({
  risks,
  updateIncludeClosed,
  teamId,
  isLoadingRisks,
  refreshRisks,
  newRisksAllowed = false,
  hideTeamColumn = false,
  gridHeight = 550,
}: RisksGridProps) => {
  const [includeClosed, setIncludeClosed] = useState<boolean>(false)
  const [hideTeam, setHideTeam] = useState<boolean>(hideTeamColumn)
  const [openCreateRiskForm, setOpenCreateRiskForm] = useState<boolean>(false)
  const [openUpdateRiskForm, setOpenUpdateRiskForm] = useState<boolean>(false)
  const [editRiskId, setEditRiskId] = useState<string | null>(null)

  const { hasPermissionClaim } = useAuth()
  const canCreateRisks = hasPermissionClaim('Permissions.Risks.Create')
  const canUpdateRisks = hasPermissionClaim('Permissions.Risks.Update')
  const showActions = newRisksAllowed && canCreateRisks

  const onIncludeClosedChange = (checked: boolean) => {
    setIncludeClosed(checked)
    updateIncludeClosed(checked)
  }

  const onHideTeamChange = (checked: boolean) => {
    setHideTeam(checked)
  }

  const editRiskButtonClicked = useCallback(
    (id: string) => {
      setEditRiskId(id)
      setOpenUpdateRiskForm(true)
    },
    [setOpenUpdateRiskForm],
  )

  const onEditRiskFormClosed = (wasSaved: boolean) => {
    setOpenUpdateRiskForm(false)
    setEditRiskId(null)
    if (wasSaved) {
      refreshRisks()
    }
  }

  const onCreateRiskFormClosed = (wasCreated: boolean) => {
    setOpenCreateRiskForm(false)
    if (wasCreated) {
      refreshRisks()
    }
  }

  const actions = () => {
    return (
      <>
        {canCreateRisks && (
          <Button type="link" onClick={() => setOpenCreateRiskForm(true)}>
            Create Risk
          </Button>
        )}
      </>
    )
  }

  const controlItems: ItemType[] = [
    {
      label: (
        <ControlItemSwitch
          label="Include Closed"
          checked={includeClosed}
          onChange={onIncludeClosedChange}
        />
      ),
      key: 'include-closed',
      onClick: () => onIncludeClosedChange(!includeClosed),
    },
    {
      label: (
        <ControlItemSwitch
          label="Hide Team"
          checked={hideTeam}
          onChange={onHideTeamChange}
        />
      ),
      key: 'hide-team',
      onClick: () => onHideTeamChange(!hideTeam),
    },
  ]

  // TODO: dates are formatted correctly and filter, but the filter is string based, not date based
  const columnDefs = useMemo<ColDef<RiskListDto>[]>(
    () => [
      {
        width: 50,
        filter: false,
        sortable: false,
        resizable: false,
        hide: !canUpdateRisks,
        cellRenderer: (params) => {
          return (
            canUpdateRisks && (
              <Button
                type="text"
                size="small"
                icon={<EditOutlined />}
                onClick={() => editRiskButtonClicked(params.data.id)}
              />
            )
          )
        },
      },
      { field: 'id', hide: true },
      { field: 'key', width: 90 },
      { field: 'summary', width: 300, cellRenderer: RiskLinkCellRenderer },
      {
        field: 'team.name',
        headerName: 'Team',
        cellRenderer: NestedTeamNameLinkCellRenderer,
        hide: hideTeam,
      },
      { field: 'status', width: 125, hide: includeClosed === false },
      { field: 'category', width: 125 },
      { field: 'exposure', width: 125 },
      {
        field: 'followUpDate',
        valueGetter: (params) =>
          params.data.followUpDate
            ? dayjs(params.data.followUpDate).format('M/D/YYYY')
            : null,
      },
      {
        field: 'assignee.name',
        headerName: 'Assignee',
        cellRenderer: AssigneeLinkCellRenderer,
      },
      {
        field: 'reportedOn',
        valueGetter: (params) =>
          dayjs(params.data.reportedOn).format('M/D/YYYY'),
      },
    ],
    [canUpdateRisks, editRiskButtonClicked, hideTeam, includeClosed],
  )

  return (
    <>
      {/* TODO:  setup dynamic height */}
      <ModaGrid
        height={gridHeight}
        columnDefs={columnDefs}
        rowData={risks}
        loading={isLoadingRisks}
        loadData={refreshRisks}
        actions={showActions && actions()}
        gridControlMenuItems={controlItems}
      />
      {openCreateRiskForm && (
        <CreateRiskForm
          createForTeamId={teamId}
          showForm={openCreateRiskForm}
          onFormCreate={() => onCreateRiskFormClosed(true)}
          onFormCancel={() => onCreateRiskFormClosed(false)}
        />
      )}
      {openUpdateRiskForm && (
        <EditRiskForm
          showForm={openUpdateRiskForm}
          riskId={editRiskId}
          onFormSave={() => onEditRiskFormClosed(true)}
          onFormCancel={() => onEditRiskFormClosed(false)}
        />
      )}
    </>
  )
}

export default RisksGrid
