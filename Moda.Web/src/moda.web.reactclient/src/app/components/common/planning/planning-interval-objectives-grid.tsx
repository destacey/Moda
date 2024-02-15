'use client'

import { PlanningIntervalObjectiveListDto } from '@/src/services/moda-api'
import Link from 'next/link'
import { useCallback, useMemo, useState } from 'react'
import ModaGrid from '../moda-grid'
import { MenuProps, Progress, Space, Switch } from 'antd'
import { ItemType } from 'antd/es/menu/hooks/useItems'
import useAuth from '../../contexts/auth'
import EditPlanningIntervalObjectiveForm from '@/src/app/planning/planning-intervals/[key]/edit-planning-interval-objective-form'
import { UseQueryResult } from 'react-query'
import dayjs from 'dayjs'
import CreateHealthCheckForm from '../health-check/create-health-check-form'
import { SystemContext } from '../../constants'
import { useAppDispatch, useAppSelector } from '@/src/app/hooks'
import { beginHealthCheckCreate } from '@/src/store/health-check-slice'
import {
  NestedHealthCheckStatusCellRenderer,
  PlanningIntervalObjectiveLinkCellRenderer,
  RowMenuCellRenderer,
  NestedTeamNameLinkCellRenderer,
  NestedPlanningIntervalLinkCellRenderer,
} from '../moda-grid-cell-renderers'
import { ColDef } from 'ag-grid-community'

export interface PlanningIntervalObjectivesGridProps {
  objectivesQuery: UseQueryResult<PlanningIntervalObjectiveListDto[], unknown>
  planningIntervalId?: string
  hidePlanningIntervalColumn?: boolean
  hideTeamColumn?: boolean
  viewSelector?: React.ReactNode
}

const ProgressCellRenderer = ({ value, data }) => {
  const progressStatus = ['Canceled', 'Missed'].includes(data.status?.name)
    ? 'exception'
    : undefined
  return (
    <Progress
      percent={value}
      size="small"
      status={progressStatus}
      style={{ marginLeft: '5px', marginRight: '5px' }}
    />
  )
}

interface RowMenuProps extends MenuProps {
  objectiveId: string
  planningIntervalKey: string
  objectiveKey: string
  canManageObjectives: boolean
  canCreateHealthChecks: boolean
  onEditObjectiveMenuClicked: (id: string) => void
  onCreateHealthCheckMenuClicked: (id: string) => void
}

const getRowMenuItems = (props: RowMenuProps) => {
  if (
    (!props.canManageObjectives && !props.canCreateHealthChecks) ||
    !props.objectiveId ||
    !props.onEditObjectiveMenuClicked ||
    !props.onCreateHealthCheckMenuClicked
  ) {
    return null
  }
  return [
    {
      key: 'editObjective',
      label: 'Edit Objective',
      disabled: !props.canManageObjectives,
      onClick: () => props.onEditObjectiveMenuClicked(props.objectiveId),
    },
    {
      key: 'createHealthCheck',
      label: 'Create Health Check',
      disabled: !props.canCreateHealthChecks,
      onClick: () => props.onCreateHealthCheckMenuClicked(props.objectiveId),
    },
    {
      key: 'healthReport',
      label: (
        <Link
          href={`/planning/planning-intervals/${props.planningIntervalKey}/objectives/${props.objectiveKey}/health-report`}
        >
          Health Report
        </Link>
      ),
    },
  ] as ItemType[]
}

const PlanningIntervalObjectivesGrid = ({
  objectivesQuery,
  planningIntervalId,
  hidePlanningIntervalColumn = false,
  hideTeamColumn = false,
  viewSelector,
}: PlanningIntervalObjectivesGridProps) => {
  const [hidePlanningInterval, setHidePlanningInterval] = useState<boolean>(
    hidePlanningIntervalColumn,
  )
  const [hideTeam, setHideTeam] = useState<boolean>(hideTeamColumn)
  const [openUpdateObjectiveForm, setOpenUpdateObjectiveForm] =
    useState<boolean>(false)
  const [selectedObjectiveId, setSelectedObjectiveId] = useState<string | null>(
    null,
  )

  const dispatch = useAppDispatch()
  const editingObjectiveId = useAppSelector(
    (state) => state.healthCheck.createContext.objectId,
  )

  const { hasClaim } = useAuth()
  const canManageObjectives = hasClaim(
    'Permission',
    'Permissions.PlanningIntervalObjectives.Manage',
  )
  const canCreateHealthChecks =
    !!canManageObjectives &&
    hasClaim('Permission', 'Permissions.HealthChecks.Create')

  const onEditObjectiveMenuClicked = useCallback((id: string) => {
    setSelectedObjectiveId(id)
    setOpenUpdateObjectiveForm(true)
  }, [])

  const onCreateHealthCheckMenuClicked = useCallback(
    (id: string) => {
      setSelectedObjectiveId(id)
      dispatch(
        beginHealthCheckCreate({
          objectId: id,
          contextId: SystemContext.PlanningPlanningIntervalObjective,
        }),
      )
    },
    [dispatch],
  )

  const refresh = useCallback(async () => {
    objectivesQuery.refetch()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const columnDefs = useMemo<ColDef<PlanningIntervalObjectiveListDto>[]>(
    () => [
      {
        headerName: 'Actions',
        width: 50,
        filter: false,
        sortable: false,
        hide: !canManageObjectives,
        suppressMenu: true,
        cellRenderer: (params) => {
          const menuItems = getRowMenuItems({
            objectiveId: params.data.id,
            planningIntervalKey: params.data.planningInterval?.key,
            objectiveKey: params.data.key,
            canManageObjectives,
            canCreateHealthChecks,
            onEditObjectiveMenuClicked,
            onCreateHealthCheckMenuClicked,
          })

          return RowMenuCellRenderer({ menuItems })
        },
      },
      { field: 'id', hide: true },
      { field: 'key', width: 90 },
      {
        field: 'name',
        width: 500,
        cellRenderer: PlanningIntervalObjectiveLinkCellRenderer,
      },
      { field: 'isStretch', width: 100 },
      {
        field: 'planningInterval.name',
        headerName: 'Planning Interval',
        cellRenderer: NestedPlanningIntervalLinkCellRenderer,
        hide: hidePlanningInterval,
      },
      { field: 'status.name', headerName: 'Status', width: 125 },
      {
        field: 'team.name',
        headerName: 'Team',
        cellRenderer: NestedTeamNameLinkCellRenderer,
        hide: hideTeam,
      },
      {
        field: 'healthCheck.status.name',
        headerName: 'Health',
        width: 125,
        cellRenderer: NestedHealthCheckStatusCellRenderer,
      },
      { field: 'progress', width: 250, cellRenderer: ProgressCellRenderer },
      {
        field: 'startDate',
        valueGetter: (params) =>
          params.data.startDate
            ? dayjs(params.data.startDate).format('M/D/YYYY')
            : null,
      },
      {
        field: 'targetDate',
        valueGetter: (params) =>
          params.data.targetDate
            ? dayjs(params.data.targetDate).format('M/D/YYYY')
            : null,
      },
    ],
    [
      canCreateHealthChecks,
      canManageObjectives,
      hidePlanningInterval,
      hideTeam,
      onCreateHealthCheckMenuClicked,
      onEditObjectiveMenuClicked,
    ],
  )

  const onHidePlanningIntervalChange = (checked: boolean) => {
    setHidePlanningInterval(checked)
  }

  const onHideTeamChange = (checked: boolean) => {
    setHideTeam(checked)
  }

  const controlItems: ItemType[] = [
    {
      label: (
        <>
          <Space direction="vertical" size="small">
            <Space>
              <Switch
                size="small"
                checked={hidePlanningInterval}
                onChange={onHidePlanningIntervalChange}
              />
              Hide PI
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

  const onEditObjectiveFormClosed = (wasSaved: boolean) => {
    setOpenUpdateObjectiveForm(false)
    setSelectedObjectiveId(null)
  }

  const onCreateHealthCheckFormClosed = (wasSaved: boolean) => {
    if (wasSaved) {
      refresh()
    }
  }

  return (
    <>
      {/* TODO:  setup dynamic height */}
      <ModaGrid
        height={650}
        columnDefs={columnDefs}
        rowData={objectivesQuery.data}
        loadData={refresh}
        gridControlMenuItems={controlItems}
        toolbarActions={viewSelector}
      />
      {openUpdateObjectiveForm && (
        <EditPlanningIntervalObjectiveForm
          showForm={openUpdateObjectiveForm}
          objectiveId={selectedObjectiveId}
          planningIntervalId={planningIntervalId}
          onFormSave={() => onEditObjectiveFormClosed(true)}
          onFormCancel={() => onEditObjectiveFormClosed(false)}
        />
      )}
      {editingObjectiveId == selectedObjectiveId && (
        <CreateHealthCheckForm onClose={onCreateHealthCheckFormClosed} />
      )}
    </>
  )
}

export default PlanningIntervalObjectivesGrid
