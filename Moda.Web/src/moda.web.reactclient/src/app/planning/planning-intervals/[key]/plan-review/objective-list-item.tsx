import { PlanningIntervalObjectiveListDto } from '@/src/services/moda-api'
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
import { MoreOutlined } from '@ant-design/icons'
import HealthCheckTag from '@/src/app/components/common/health-check/health-check-tag'
import { ItemType } from 'antd/es/menu/hooks/useItems'
import CreateHealthCheckForm from '@/src/app/components/common/health-check/create-health-check-form'
import { SystemContext } from '@/src/app/components/constants'
import { useAppDispatch, useAppSelector } from '@/src/app/hooks'
import { beginHealthCheckCreate } from '@/src/store/features/health-check-slice'
import { EditPlanningIntervalObjectiveForm } from '../../../components'
import { useSortable } from '@dnd-kit/sortable'
import { CSS } from '@dnd-kit/utilities'

const { Item } = List
const { Meta } = Item
const { Text } = Typography

export interface ObjectiveListItemProps {
  objective: PlanningIntervalObjectiveListDto
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

  const { attributes, listeners, setNodeRef, transform, transition } =
    useSortable({ id: objective.id })

  const dispatch = useAppDispatch()
  const editingObjectiveId = useAppSelector(
    (state) => state.healthCheck.createContext.objectId,
  )

  const title = () => {
    return (
      <Link
        href={`/planning/planning-intervals/${piKey}/objectives/${objective.key}`}
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
          <Text>
            {startDate}
            {startDate && targetDate && ' - '}
            {targetDate}
          </Text>
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
                contextId: SystemContext.PlanningPlanningIntervalObjective,
              }),
            ),
        },
        {
          key: 'healthReport',
          label: (
            <Link
              href={`/planning/planning-intervals/${objective.planningInterval.key}/objectives/${objective.key}/health-report`}
            >
              Health Report
            </Link>
          ),
        },
      )
    }
    return items
  }, [
    canUpdateObjectives,
    canCreateHealthChecks,
    objective.planningInterval.key,
    objective.key,
    objective.id,
    dispatch,
  ])

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

  const sortableStyle = {
    transition: transition,
    transform: CSS.Transform.toString(transform),
    touchAction: 'none',
  }

  return (
    <>
      <Item
        key={objective.key}
        ref={setNodeRef}
        {...attributes}
        {...listeners}
        style={sortableStyle}
      >
        <Meta title={title()} description={description()} />
        {canUpdateObjectives && (
          <Dropdown menu={{ items: menuItems }}>
            <Button type="text" size="small" icon={<MoreOutlined />} />
          </Dropdown>
        )}
      </Item>
      {openUpdateObjectiveForm && (
        <EditPlanningIntervalObjectiveForm
          showForm={openUpdateObjectiveForm}
          objectiveId={objective?.id}
          planningIntervalId={objective?.planningInterval?.id}
          onFormSave={() => onEditObjectiveFormClosed(true)}
          onFormCancel={() => onEditObjectiveFormClosed(false)}
        />
      )}
      {editingObjectiveId == objective?.id && (
        <CreateHealthCheckForm onClose={onCreateHealthCheckFormClosed} />
      )}
    </>
  )
}

export default ObjectiveListItem
