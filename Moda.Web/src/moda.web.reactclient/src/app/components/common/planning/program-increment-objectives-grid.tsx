'use client'

import { ProgramIncrementObjectiveListDto } from '@/src/services/moda-api'
import Link from 'next/link'
import { useCallback, useMemo, useState } from 'react'
import ModaGrid from '../moda-grid'
import { Button, Dropdown, MenuProps, Progress, Space, Switch } from 'antd'
import { ItemType } from 'antd/es/menu/hooks/useItems'
import useAuth from '../../contexts/auth'
import CreateProgramIncrementObjectiveForm from '@/src/app/planning/program-increments/[key]/create-program-increment-objective-form'
import EditProgramIncrementObjectiveForm from '@/src/app/planning/program-increments/[key]/edit-program-increment-objective-form'
import { MenuOutlined } from '@ant-design/icons'
import { UseQueryResult } from 'react-query'
import dayjs from 'dayjs'
import CreateHealthCheckForm from '../health-check/create-health-check-form'
import { SystemContext } from '../../constants'
import { useAppDispatch, useAppSelector } from '@/src/app/hooks'
import { beginHealthCheckCreate } from '@/src/store/health-check-slice'

export interface ProgramIncrementObjectivesGridProps {
  objectivesQuery: UseQueryResult<ProgramIncrementObjectiveListDto[], unknown>
  programIncrementId?: string
  hideProgramIncrementColumn?: boolean
  hideTeamColumn?: boolean
  newObjectivesAllowed?: boolean
  viewSelector?: React.ReactNode
}

const programIncrementObjectiveLinkCellRenderer = ({ value, data }) => {
  return (
    <Link
      href={`/planning/program-increments/${data.programIncrement?.key}/objectives/${data.key}`}
    >
      {value}
    </Link>
  )
}

const programIncrementLinkCellRenderer = ({ value, data }) => {
  return (
    <Link href={`/planning/program-increments/${data.programIncrement?.key}`}>
      {value}
    </Link>
  )
}

const teamLinkCellRenderer = ({ value, data }) => {
  const teamLink =
    data.team?.type === 'Team'
      ? `/organizations/teams/${data.team?.key}`
      : `/organizations/team-of-teams/${data.team?.key}`
  return <Link href={teamLink}>{value}</Link>
}

const progressCellRenderer = ({ value, data }) => {
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

interface ActionsMenuCellRendererProps {
  objectiveId: string
  canManageObjectives: boolean
  canCreateHealthChecks: boolean
  onEditObjectiveMenuClicked: (id: string) => void
  onCreateHealthCheckMenuClicked: (id: string) => void
}

const actionsMenuCellRenderer = (props: ActionsMenuCellRendererProps) => {
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
  const items: ItemType[] = [
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
  ]

  return (
    <Dropdown menu={{ items }} trigger={['click']}>
      <Button type="text" size="small" icon={<MenuOutlined />} />
    </Dropdown>
  )
}

const ProgramIncrementObjectivesGrid = ({
  objectivesQuery,
  programIncrementId,
  hideProgramIncrementColumn = false,
  hideTeamColumn = false,
  newObjectivesAllowed = false,
  viewSelector,
}: ProgramIncrementObjectivesGridProps) => {
  const [hideProgramIncrement, setHideProgramIncrement] = useState<boolean>(
    hideProgramIncrementColumn,
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
    'Permissions.ProgramIncrementObjectives.Manage',
  )
  const canCreateObjectives =
    newObjectivesAllowed && programIncrementId && canManageObjectives
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
          contextId: SystemContext.PlanningProgramIncrementObjective,
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
          const objectiveId = params.data.id
          return actionsMenuCellRenderer({
            objectiveId,
            canManageObjectives,
            canCreateHealthChecks,
            onEditObjectiveMenuClicked,
            onCreateHealthCheckMenuClicked,
          })
        },
      },
      { field: 'id', hide: true },
      { field: 'key', width: 90 },
      {
        field: 'name',
        width: 400,
        cellRenderer: programIncrementObjectiveLinkCellRenderer,
      },
      {
        field: 'programIncrement.name',
        cellRenderer: programIncrementLinkCellRenderer,
        hide: hideProgramIncrement,
      },
      { field: 'status.name', width: 125 },
      {
        field: 'team.name',
        cellRenderer: teamLinkCellRenderer,
        hide: hideTeam,
      },
      { field: 'healthCheck.status.name', headerName: 'Health', width: 125 },
      { field: 'progress', width: 250, cellRenderer: progressCellRenderer },
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
      { field: 'isStretch', width: 100 },
    ],
    [
      canCreateHealthChecks,
      canManageObjectives,
      hideProgramIncrement,
      hideTeam,
      onCreateHealthCheckMenuClicked,
      onEditObjectiveMenuClicked,
    ],
  )

  const onHideProgramIncrementChange = (checked: boolean) => {
    setHideProgramIncrement(checked)
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
                checked={hideProgramIncrement}
                onChange={onHideProgramIncrementChange}
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
        <CreateProgramIncrementObjectiveForm
          showForm={openCreateObjectiveForm}
          programIncrementId={programIncrementId}
          onFormCreate={() => onCreateObjectiveFormClosed(true)}
          onFormCancel={() => onCreateObjectiveFormClosed(false)}
        />
      )}
      {openUpdateObjectiveForm && (
        <EditProgramIncrementObjectiveForm
          showForm={openUpdateObjectiveForm}
          objectiveId={selectedObjectiveId}
          programIncrementId={programIncrementId}
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

export default ProgramIncrementObjectivesGrid
