import Link from 'next/link'
import { useCallback, useMemo, useState } from 'react'
import ModaGrid from '../moda-grid'
import { RiskListDto } from '@/src/services/moda-api'
import { ItemType } from 'antd/es/menu/hooks/useItems'
import { Button, Space, Switch } from 'antd'
import dayjs from 'dayjs'
import useAuth from '../../contexts/auth'
import CreateRiskForm from './create-risk-form'
import { EditOutlined } from '@ant-design/icons'
import EditRiskForm from './edit-risk-form'
import { UseQueryResult } from 'react-query'
import {
  NestedTeamNameLinkCellRenderer,
  TeamNameLinkCellRenderer,
} from '../moda-grid-cell-renderers'
import { ColDef } from 'ag-grid-community'

export interface RisksGridProps {
  risksQuery: UseQueryResult<RiskListDto[], unknown>
  updateIncludeClosed: (includeClosed: boolean) => void
  teamId?: string | null
  newRisksAllowed?: boolean
  hideTeamColumn?: boolean
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
  risksQuery,
  updateIncludeClosed,
  teamId,
  newRisksAllowed = false,
  hideTeamColumn = false,
}: RisksGridProps) => {
  const [includeClosed, setIncludeClosed] = useState<boolean>(false)
  const [hideTeam, setHideTeam] = useState<boolean>(hideTeamColumn)
  const [openCreateRiskForm, setOpenCreateRiskForm] = useState<boolean>(false)
  const [openUpdateRiskForm, setOpenUpdateRiskForm] = useState<boolean>(false)
  const [editRiskId, setEditRiskId] = useState<string | null>(null)

  const { hasClaim } = useAuth()
  const canCreateRisks = hasClaim('Permission', 'Permissions.Risks.Create')
  const canUpdateRisks = hasClaim('Permission', 'Permissions.Risks.Update')
  const showActions = newRisksAllowed && canCreateRisks

  const onIncludeClosedChange = (checked: boolean) => {
    setIncludeClosed(checked)
    updateIncludeClosed(checked)
  }

  const onHideTeamChange = (checked: boolean) => {
    setHideTeam(checked)
  }

  const refresh = useCallback(async () => {
    risksQuery.refetch()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

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
      refresh()
    }
  }

  const onCreateRiskFormClosed = (wasCreated: boolean) => {
    setOpenCreateRiskForm(false)
    if (wasCreated) {
      refresh()
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
        <>
          <Space direction="vertical" size="small">
            <Space>
              <Switch
                size="small"
                checked={includeClosed}
                onChange={onIncludeClosedChange}
              />
              Include Closed
            </Space>
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
  const columnDefs = useMemo<ColDef<RiskListDto>[]>(
    () => [
      {
        width: 50,
        filter: false,
        sortable: false,
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
        height={550}
        columnDefs={columnDefs}
        rowData={risksQuery.data}
        loadData={refresh}
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
