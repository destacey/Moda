import { ProgramIncrementObjectiveListDto } from '@/src/services/moda-api'
import {
  Button,
  Dropdown,
  List,
  MenuProps,
  Progress,
  Space,
  Tag,
  Typography,
} from 'antd'
import dayjs from 'dayjs'
import Link from 'next/link'
import { useMemo, useState } from 'react'
import EditProgramIncrementObjectiveForm from '../edit-program-increment-objective-form'
import { MenuOutlined } from '@ant-design/icons'
import HealthCheckTag from '@/src/app/components/common/health-check/health-check-tag'
import { ItemType } from 'antd/es/menu/hooks/useItems'
import CreateHealthCheckForm from '@/src/app/components/common/health-check/create-health-check-form'
import { SystemContext } from '@/src/app/components/constants'
import { useAppDispatch, useAppSelector } from '@/src/app/hooks'
import {
  beginHealthCheckCreate,
  selectHealthCheckIsInEditMode,
} from '@/src/store/health-check-slice'

export interface ObjectiveListItemProps {
  objective: ProgramIncrementObjectiveListDto
  piKey: number
  canUpdateObjectives: boolean
  canCreateHealthChecks: boolean
  refreshObjectives: () => void
}

const getColorForStatus = (status: string) => {
  switch (status) {
    case 'In Progress':
      return 'processing'
    case 'Completed':
      return 'success'
    case 'Canceled':
    case 'Missed':
      return 'error'
    default:
      return 'default'
  }
}

const ObjectiveListItem = ({
  objective,
  piKey,
  canUpdateObjectives,
  canCreateHealthChecks,
  refreshObjectives,
}: ObjectiveListItemProps) => {
  const [openUpdateObjectiveForm, setOpenUpdateObjectiveForm] =
    useState<boolean>(false)

  const dispatch = useAppDispatch()
  const editingObjectiveId = useAppSelector(
    (state) => state.healthCheck.createContext.objectId,
  )

  const title = () => {
    return (
      <Link
        href={`/planning/program-increments/${piKey}/objectives/${objective.key}`}
      >
        {objective.key} - {objective.name}
      </Link>
    )
  }

  const description = () => {
    const startDate = objective.startDate
      ? `Start: ${
          objective.startDate
            ? dayjs(objective.startDate)?.format('M/D/YYYY')
            : ''
        }`
      : null
    const targetDate = objective.targetDate
      ? `Target: ${
          objective.targetDate
            ? dayjs(objective.targetDate)?.format('M/D/YYYY')
            : ''
        }`
      : null
    const showProgress = objective.status?.name !== 'Not Started'
    const progressStatus = ['Canceled', 'Missed'].includes(
      objective.status?.name,
    )
      ? 'exception'
      : undefined
    return (
      <>
        <Space wrap>
          <Tag color={getColorForStatus(objective.status.name)}>
            {objective.status.name}
          </Tag>
          {objective.isStretch && <Tag>Stretch</Tag>}
          <HealthCheckTag healthCheck={objective?.healthCheck} />
          <Typography.Text>
            {startDate}
            {startDate && targetDate && ' - '}
            {targetDate}
          </Typography.Text>
        </Space>
        {showProgress && (
          <Progress
            percent={objective.progress}
            status={progressStatus}
            size="small"
          />
        )}
      </>
    )
  }

  // TODO: make a menu component that will render one item with its icon, or a dropdown when there multiple items
  const menuItems: MenuProps['items'] = useMemo(() => {
    const items: ItemType[] = []
    if (canUpdateObjectives || canCreateHealthChecks) {
      items.push(
        {
          key: 'edit',
          label: 'Edit',
          disabled: !canUpdateObjectives,
          onClick: () => setOpenUpdateObjectiveForm(true),
        },
        {
          key: 'createHealthCheck',
          label: 'Create Health Check',
          disabled: !canCreateHealthChecks,
          onClick: () =>
            dispatch(
              beginHealthCheckCreate({
                objectId: objective.id,
                contextId: SystemContext.PlanningProgramIncrementObjective,
              }),
            ),
        },
      )
    }
    return items
  }, [canUpdateObjectives, canCreateHealthChecks, dispatch, objective.id])

  const onEditObjectiveFormClosed = (wasSaved: boolean) => {
    setOpenUpdateObjectiveForm(false)
    if (wasSaved) {
      refreshObjectives()
    }
  }

  const onCreateHealthCheckFormClosed = (wasSaved: boolean) => {
    if (wasSaved) {
      refreshObjectives()
    }
  }

  return (
    <>
      <List.Item key={objective.key}>
        <List.Item.Meta title={title()} description={description()} />
        {canUpdateObjectives && (
          <Dropdown menu={{ items: menuItems }}>
            <Button type="text" size="small" icon={<MenuOutlined />} />
          </Dropdown>
        )}
      </List.Item>
      {openUpdateObjectiveForm && (
        <EditProgramIncrementObjectiveForm
          showForm={openUpdateObjectiveForm}
          objectiveId={objective?.id}
          programIncrementId={objective?.programIncrement?.id}
          onFormSave={() => onEditObjectiveFormClosed(true)}
          onFormCancel={() => onEditObjectiveFormClosed(false)}
        />
      )}
      {editingObjectiveId == objective?.id && (
        <CreateHealthCheckForm onClose={onCreateHealthCheckFormClosed}/>
      )}
    </>
  )
}

export default ObjectiveListItem
