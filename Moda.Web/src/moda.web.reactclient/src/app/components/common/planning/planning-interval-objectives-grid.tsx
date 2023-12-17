'use client'

import { PlanningIntervalObjectiveListDto } from '@/src/services/moda-api'
import Link from 'next/link'
import { useCallback, useMemo, useState } from 'react'
import ModaGrid from '../moda-grid'
import { Button, MenuProps, Progress, Space, Switch } from 'antd'
import { ItemType } from 'antd/es/menu/hooks/useItems'
import useAuth from '../../contexts/auth'
import CreatePlanningIntervalObjectiveForm from '@/src/app/planning/planning-intervals/[key]/create-planning-interval-objective-form'
import EditPlanningIntervalObjectiveForm from '@/src/app/planning/planning-intervals/[key]/edit-planning-interval-objective-form'
import { UseQueryResult } from 'react-query'
import dayjs from 'dayjs'
import CreateHealthCheckForm from '../health-check/create-health-check-form'
import { SystemContext } from '../../constants'
import { useAppDispatch, useAppSelector } from '@/src/app/hooks'
import { beginHealthCheckCreate } from '@/src/store/health-check-slice'
import {
  HealthCheckStatusCellRenderer,
  PlanningTeamLinkCellRenderer,
  PlanningIntervalLinkCellRenderer,
  PlanningIntervalObjectiveLinkCellRenderer,
  RowMenuCellRenderer,
} from '../moda-grid-cell-renderers'

export interface PlanningIntervalObjectivesGridProps {
  objectivesQuery: UseQueryResult<PlanningIntervalObjectiveListDto[], unknown>
  planningIntervalId?: string
  hidePlanningIntervalColumn?: boolean
  hideTeamColumn?: boolean
  newObjectivesAllowed?: boolean
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
    !props.canManageObjectives ||
    !props.canCreateHealthChecks ||
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
  newObjectivesAllowed = false,
  viewSelector,
}: PlanningIntervalObjectivesGridProps) => {
  const [hidePlanningInterval, setHidePlanningInterval] = useState<boolean>(
    hidePlanningIntervalColumn,
  )
  const [hideTeam, setHideTeam] = useState<boolean>(hideTeamColumn)
  const [openCreateObjectiveForm, setOpenCreateObjectiveForm] =
    useState<boolean>(false)
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
  const canCreateObjectives =
    newObjectivesAllowed && planningIntervalId && canManageObjectives
  const canCreateHealthChecks =
    !!canManageObjectives &&
    hasClaim('Permission', 'Permissions.HealthChecks.Create')
  const showActions = canCreateObjectives

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

  const createObjectiveButtonClicked = useCallback(() => {
    setOpenCreateObjectiveForm(true)
  }, [])

  const columnDefs = useMemo(
    () => [
      {
        field: 'actions',
        headerName: '',
        width: 50,
        filter: false,
        sortable: false,
        hide: !canManageObjectives,
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
        width: 400,
        cellRenderer: PlanningIntervalObjectiveLinkCellRenderer,
      },
      { field: 'isStretch', width: 100 },
      {
        field: 'planningInterval',
        valueFormatter: (params) => params.value.name,
        cellRenderer: PlanningIntervalLinkCellRenderer,
        hide: hidePlanningInterval,
      },
      { field: 'status.name', headerName: 'Status', width: 125 },
      {
        field: 'team',
        valueFormatter: (params) => params.value.name,
        cellRenderer: PlanningTeamLinkCellRenderer,
        hide: hideTeam,
      },
      {
        field: 'healthCheck',
        headerName: 'Health',
        width: 125,
        valueFormatter: (params) => params.value?.status.name,
        cellRenderer: HealthCheckStatusCellRenderer,
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

  const actions = () => {
    return (
      <>
        {canCreateObjectives && (
          <Button type="link" onClick={createObjectiveButtonClicked}>
            Create Objective
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

  const onCreateObjectiveFormClosed = (wasCreated: boolean) => {
    setOpenCreateObjectiveForm(false)
  }

  const onEditObjectiveFormClosed = (wasSaved: boolean) => {
    setOpenUpdateObjectiveForm(false)
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
        height={550}
        columnDefs={columnDefs}
        rowData={objectivesQuery.data}
        loadData={refresh}
        actions={showActions && actions()}
        gridControlMenuItems={controlItems}
        toolbarActions={viewSelector}
      />
      {openCreateObjectiveForm && (
        <CreatePlanningIntervalObjectiveForm
          showForm={openCreateObjectiveForm}
          planningIntervalId={planningIntervalId}
          onFormCreate={() => onCreateObjectiveFormClosed(true)}
          onFormCancel={() => onCreateObjectiveFormClosed(false)}
        />
      )}
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
