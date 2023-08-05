import { ProgramIncrementObjectiveListDto } from '@/src/services/moda-api'
import Link from 'next/link'
import { useCallback, useMemo, useState } from 'react'
import ModaGrid from '../moda-grid'
import { Button, Progress, Space, Switch } from 'antd'
import { ItemType } from 'antd/es/menu/hooks/useItems'
import useAuth from '../../contexts/auth'
import CreateProgramIncrementObjectiveForm from '@/src/app/planning/program-increments/[id]/create-program-increment-objective-form'
import EditProgramIncrementObjectiveForm from '@/src/app/planning/program-increments/[id]/edit-program-increment-objective-form'
import { EditOutlined } from '@ant-design/icons'

export interface ProgramIncrementObjectivesGridProps {
  getObjectives: (id: string) => Promise<ProgramIncrementObjectiveListDto[]>
  programIncrementId: string
  hideProgramIncrementColumn?: boolean
  hideTeamColumn?: boolean
  newObjectivesAllowed?: boolean
}

const ProgramIncrementObjectiveLinkCellRenderer = ({ value, data }) => {
  return (
    <Link
      href={`/planning/program-increments/${data.programIncrement?.localId}/objectives/${data.localId}`}
    >
      {value}
    </Link>
  )
}

const ProgramIncrementLinkCellRenderer = ({ value, data }) => {
  return (
    <Link
      href={`/planning/program-increments/${data.programIncrement?.localId}`}
    >
      {value}
    </Link>
  )
}

const TeamLinkCellRenderer = ({ value, data }) => {
  const teamLink =
    data.team?.type === 'Team'
      ? `/organizations/teams/${data.team?.localId}`
      : `/organizations/team-of-teams/${data.team?.localId}`
  return <Link href={teamLink}>{value}</Link>
}

const ProgressCellRenderer = ({ value, data }) => {
  const progressStatus =
    data.status?.name === 'Canceled' ? 'exception' : undefined
  return (
    <Progress
      percent={value}
      size="small"
      status={progressStatus}
      style={{ marginLeft: '5px', marginRight: '5px' }}
    />
  )
}

const ProgramIncrementObjectivesGrid = ({
  getObjectives,
  programIncrementId,
  hideProgramIncrementColumn = false,
  hideTeamColumn = false,
  newObjectivesAllowed = false,
}: ProgramIncrementObjectivesGridProps) => {
  const [objectives, setObjectives] =
    useState<ProgramIncrementObjectiveListDto[]>()
  const [hideProgramIncrement, setHideProgramIncrement] = useState<boolean>(
    hideProgramIncrementColumn
  )
  const [hideTeam, setHideTeam] = useState<boolean>(hideTeamColumn)
  const [openCreateObjectiveModal, setOpenCreateObjectiveModal] =
    useState<boolean>(false)
  const [openUpdateObjectiveForm, setOpenUpdateObjectiveForm] =
    useState<boolean>(false)
  const [editObjectiveId, setEditObjectiveId] = useState<string | null>(null)

  const { hasClaim } = useAuth()
  const canManageObjectives = hasClaim(
    'Permission',
    'Permissions.ProgramIncrementObjectives.Manage'
  )
  const canCreateObjectives =
    newObjectivesAllowed && programIncrementId && canManageObjectives
  const showActions = canCreateObjectives

  const loadObjectives = useCallback(async () => {
    const objectives = await getObjectives(programIncrementId)
    setObjectives(objectives)
  }, [getObjectives, programIncrementId])

  const editObjectiveButtonClicked = useCallback(
    (id: string) => {
      setEditObjectiveId(id)
      setOpenUpdateObjectiveForm(true)
    },
    [setOpenUpdateObjectiveForm]
  )

  const columnDefs = useMemo(
    () => [
      {
        field: 'actions',
        headerName: '',
        width: 80,
        filter: false,
        sortable: false,
        hide: !canManageObjectives,
        cellRenderer: (params) => {
          return (
            canManageObjectives && (
              <Button
                type="text"
                size="small"
                icon={<EditOutlined />}
                onClick={() => editObjectiveButtonClicked(params.data.id)}
              />
            )
          )
        },
      },
      { field: 'id', hide: true },
      { field: 'localId', headerName: '#', width: 90 },
      {
        field: 'name',
        width: 400,
        cellRenderer: ProgramIncrementObjectiveLinkCellRenderer,
      },
      {
        field: 'programIncrement.name',
        cellRenderer: ProgramIncrementLinkCellRenderer,
        hide: hideProgramIncrement,
      },
      { field: 'status.name' },
      {
        field: 'team.name',
        cellRenderer: TeamLinkCellRenderer,
        hide: hideTeam,
      },
      { field: 'progress', width: 250, cellRenderer: ProgressCellRenderer },
      { field: 'startDate' },
      { field: 'targetDate' },
      { field: 'isStretch' },
    ],
    [
      canManageObjectives,
      editObjectiveButtonClicked,
      hideProgramIncrement,
      hideTeam,
    ]
  )

  const onHideProgramIncrementChange = (checked: boolean) => {
    setHideProgramIncrement(checked)
  }

  const onHideTeamChange = (checked: boolean) => {
    setHideTeam(checked)
  }

  const Actions = () => {
    return (
      <>
        {canCreateObjectives && (
          <Button onClick={() => setOpenCreateObjectiveModal(true)}>
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
    setOpenCreateObjectiveModal(false)
    if (wasCreated) {
      loadObjectives()
    }
  }

  const onEditObjectiveFormClosed = (wasSaved: boolean) => {
    setOpenUpdateObjectiveForm(false)
    if (wasSaved) {
      loadObjectives()
    }
  }

  return (
    <>
      {/* TODO:  setup dynamic height */}
      <ModaGrid
        height={550}
        columnDefs={columnDefs}
        rowData={objectives}
        loadData={loadObjectives}
        actions={showActions && <Actions />}
        gridControlMenuItems={controlItems}
      />
      {canManageObjectives && (
        <CreateProgramIncrementObjectiveForm
          programIncrementId={programIncrementId}
          showForm={openCreateObjectiveModal}
          onFormCreate={() => onCreateObjectiveFormClosed(true)}
          onFormCancel={() => onCreateObjectiveFormClosed(false)}
        />
      )}
      {canManageObjectives && (
        <EditProgramIncrementObjectiveForm
          showForm={openUpdateObjectiveForm}
          objectiveId={editObjectiveId}
          programIncrementId={programIncrementId}
          onFormSave={() => onEditObjectiveFormClosed(true)}
          onFormCancel={() => onEditObjectiveFormClosed(false)}
        />
      )}
    </>
  )
}

export default ProgramIncrementObjectivesGrid
