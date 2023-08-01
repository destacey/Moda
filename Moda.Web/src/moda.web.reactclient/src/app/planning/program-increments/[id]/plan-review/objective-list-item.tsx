import { ProgramIncrementObjectiveListDto } from '@/src/services/moda-api'
import { Button, List, Progress, Space, Tag, Typography } from 'antd'
import dayjs from 'dayjs'
import Link from 'next/link'
import { useState } from 'react'
import EditProgramIncrementObjectiveForm from '../edit-program-increment-objective-form'
import { EditOutlined } from '@ant-design/icons'

export interface ObjectiveListItemProps {
  objective: ProgramIncrementObjectiveListDto
  piLocalId: number
  canUpdateObjectives: boolean
  refreshObjectives: () => void
}

const ObjectiveListItem = ({
  objective,
  piLocalId,
  canUpdateObjectives,
  refreshObjectives,
}: ObjectiveListItemProps) => {
  const [openUpdateObjectiveForm, setOpenUpdateObjectiveForm] =
    useState<boolean>(false)
  const title = () => {
    return (
      <Link
        href={`/planning/program-increments/${piLocalId}/objectives/${objective.localId}`}
      >
        {objective.localId} - {objective.name}
      </Link>
    )
  }

  const getColorForStatus = (status: string) => {
    switch (status) {
      case 'In Progress':
        return 'blue'
      case 'Closed':
        return 'green'
      case 'Canceled':
        return 'red'
      default:
        return 'default'
    }
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
    const progressStatus =
      objective.status?.name === 'Canceled' ? 'exception' : undefined
    return (
      <>
        <Space>
          <Tag color={getColorForStatus(objective.status.name)}>
            {objective.status.name}
          </Tag>
          {objective.isStretch && <Tag>Stretch</Tag>}
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
      <List.Item key={objective.localId}>
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
      {canUpdateObjectives && (
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
