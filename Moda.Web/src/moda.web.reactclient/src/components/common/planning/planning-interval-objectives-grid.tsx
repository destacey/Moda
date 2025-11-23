'use client'

import { PlanningIntervalObjectiveListDto } from '@/src/services/moda-api'
import Link from 'next/link'
import { useCallback, useMemo, useState } from 'react'
import ModaGrid from '../moda-grid'
import { MenuProps, Progress } from 'antd'
import { ItemType } from 'antd/es/menu/interface'
import useAuth from '../../contexts/auth'
import EditPlanningIntervalObjectiveForm from '@/src/app/planning/planning-intervals/_components/edit-planning-interval-objective-form'
import dayjs from 'dayjs'
import CreateHealthCheckForm from '../health-check/create-health-check-form'
import { SystemContext } from '../../constants'
import { useAppDispatch, useAppSelector } from '@/src/hooks'
import { beginHealthCheckCreate } from '@/src/store/features/health-check-slice'
import {
  NestedHealthCheckStatusCellRenderer,
  PlanningIntervalObjectiveLinkCellRenderer,
  RowMenuCellRenderer,
  NestedTeamNameLinkCellRenderer,
  NestedPlanningIntervalLinkCellRenderer,
} from '../moda-grid-cell-renderers'
import { ColDef } from 'ag-grid-community'
import { ControlItemSwitch } from '../control-items-menu'

export interface PlanningIntervalObjectivesGridProps {
  objectivesData: PlanningIntervalObjectiveListDto[]
  refreshObjectives: () => void
  isLoading: boolean
  planningIntervalKey: number
  hidePlanningIntervalColumn?: boolean
  hideTeamColumn?: boolean
  viewSelector?: React.ReactNode
}

interface SelectedObjective {
  id: string
  key: number
}

const ProgressCellRenderer = ({ value, data }) => {
  const progressStatus = ['Canceled', 'Missed'].includes(data.status?.name)
    ? 'exception'
    : undefined
  return <Progress percent={value} size="small" status={progressStatus} />
}

interface RowMenuProps extends MenuProps {
  planningIntervalKey: number
  objectiveId: string
  objectiveKey: number
  canManageObjectives: boolean
  canCreateHealthChecks: boolean
  onEditObjectiveMenuClicked: (id: string, key: number) => void
  onCreateHealthCheckMenuClicked: (id: string, key: number) => void
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
      onClick: () =>
        props.onEditObjectiveMenuClicked(props.objectiveId, props.objectiveKey),
    },
    {
      key: 'createHealthCheck',
      label: 'Create Health Check',
      disabled: !props.canCreateHealthChecks,
      onClick: () =>
        props.onCreateHealthCheckMenuClicked(
          props.objectiveId,
          props.objectiveKey,
        ),
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
  objectivesData,
  refreshObjectives,
  isLoading,
  planningIntervalKey,
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
  const [selectedObjective, setSelectedObjective] =
    useState<SelectedObjective | null>(null)

  const dispatch = useAppDispatch()
  const editingObjectiveId = useAppSelector(
    (state) => state.healthCheck.createContext.objectId,
  )

  const { hasPermissionClaim } = useAuth()
  const canManageObjectives = hasPermissionClaim(
    'Permissions.PlanningIntervalObjectives.Manage',
  )
  const canCreateHealthChecks =
    !!canManageObjectives &&
    hasPermissionClaim('Permissions.HealthChecks.Create')

  const onEditObjectiveMenuClicked = useCallback((id: string, key: number) => {
    setSelectedObjective({ id, key })
    setOpenUpdateObjectiveForm(true)
  }, [])

  const onCreateHealthCheckMenuClicked = useCallback(
    (id: string, key: number) => {
      setSelectedObjective({ id, key })
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
    refreshObjectives()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const columnDefs = useMemo<ColDef<PlanningIntervalObjectiveListDto>[]>(
    () => [
      {
        width: 50,
        filter: false,
        sortable: false,
        resizable: false,
        hide: !canManageObjectives,
        cellRenderer: (params) => {
          const menuItems = getRowMenuItems({
            planningIntervalKey: planningIntervalKey,
            objectiveId: params.data.id,
            objectiveKey: params.data.key,
            canManageObjectives,
            canCreateHealthChecks,
            onEditObjectiveMenuClicked,
            onCreateHealthCheckMenuClicked,
          })

          return RowMenuCellRenderer({ ...params, menuItems })
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
      {
        field: 'order',
        width: 100,
        comparator: (a, b) => {
          if (!a) return 1 // sort empty at the end
          if (!b) return -1

          return a - b
        },
      },
    ],
    [
      canCreateHealthChecks,
      canManageObjectives,
      hidePlanningInterval,
      hideTeam,
      onCreateHealthCheckMenuClicked,
      onEditObjectiveMenuClicked,
      planningIntervalKey,
    ],
  )

  const onHidePlanningIntervalChange = (checked: boolean) => {
    setHidePlanningInterval(checked)
  }

  const onHideTeamChange = (checked: boolean) => {
    setHideTeam(checked)
  }

  const controlItems = (): ItemType[] => {
    const items: ItemType[] = []

    items.push(
      {
        label: (
          <ControlItemSwitch
            label="Hide PI"
            checked={hidePlanningInterval}
            onChange={onHidePlanningIntervalChange}
          />
        ),
        key: 'hide-planning-interval',
        onClick: () => onHidePlanningIntervalChange(!hidePlanningInterval),
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
    )

    return items
  }

  const onEditObjectiveFormClosed = (wasSaved: boolean) => {
    setOpenUpdateObjectiveForm(false)
    setSelectedObjective(null)
    if (wasSaved) {
      refresh()
    }
  }

  const onCreateHealthCheckFormClosed = (wasSaved: boolean) => {
    if (wasSaved) {
      refresh()
    }
  }

  if (!planningIntervalKey) return null

  return (
    <>
      {/* TODO:  setup dynamic height */}
      <ModaGrid
        height={650}
        columnDefs={columnDefs}
        rowData={objectivesData}
        loading={isLoading}
        loadData={refresh}
        gridControlMenuItems={controlItems()}
        toolbarActions={viewSelector}
      />
      {openUpdateObjectiveForm && (
        <EditPlanningIntervalObjectiveForm
          showForm={openUpdateObjectiveForm}
          objectiveKey={selectedObjective.key}
          planningIntervalKey={planningIntervalKey}
          onFormSave={() => onEditObjectiveFormClosed(true)}
          onFormCancel={() => onEditObjectiveFormClosed(false)}
        />
      )}
      {editingObjectiveId == selectedObjective?.id && (
        <CreateHealthCheckForm onClose={onCreateHealthCheckFormClosed} />
      )}
    </>
  )
}

export default PlanningIntervalObjectivesGrid
