import { ProgramIncrementObjectiveListDto } from '@/src/services/moda-api'
import { Button, List, Progress, Space, Tag, Typography } from 'antd'
import dayjs from 'dayjs'
import Link from 'next/link'
import { useState } from 'react'
import EditProgramIncrementObjectiveForm from '../edit-program-increment-objective-form'
import { EditOutlined } from '@ant-design/icons'
import HealthCheckTag from '@/src/app/components/common/health-check/health-check-tag'

export interface ObjectiveListItemProps {
  objective: ProgramIncrementObjectiveListDto
  piKey: number
  canUpdateObjectives: boolean
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
  refreshObjectives,
}: ObjectiveListItemProps) => {
  const [openUpdateObjectiveForm, setOpenUpdateObjectiveForm] =
    useState<boolean>(false)
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

  const onEditObjectiveFormClosed = (wasSaved: boolean) => {
    setOpenUpdateObjectiveForm(false)
    if (wasSaved) {
      refreshObjectives()
    }
  }

  return (
    <>
      <List.Item key={objective.key}>
        <List.Item.Meta title={title()} description={description()} />
        {canUpdateObjectives && (
          <Button
            type="text"
            size="small"
            icon={<EditOutlined />}
            onClick={() => setOpenUpdateObjectiveForm(true)}
          />
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
    </>
  )
}

export default ObjectiveListItem
