import { ProgramIncrementObjectiveListDto } from '@/src/services/moda-api'
import { Button, List, Progress, Typography } from 'antd'
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
  const description = () => {
    const content = `Status: ${objective.status?.name} | Stretch?: ${objective.isStretch}`
    const startDate = objective.startDate
      ? ` | Start: ${
          objective.startDate
            ? dayjs(objective.startDate)?.format('M/D/YYYY')
            : ''
        }`
      : null
    const targetDate = objective.targetDate
      ? ` | Target: ${
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
        <Typography.Text>
          {content}
          {startDate}
          {targetDate}
        </Typography.Text>
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
      <EditProgramIncrementObjectiveForm
        showForm={openUpdateObjectiveForm}
        objectiveId={objective?.id}
        programIncrementId={objective?.programIncrement?.id}
        onFormSave={() => onEditObjectiveFormClosed(true)}
        onFormCancel={() => onEditObjectiveFormClosed(false)}
      />
    </>
  )
}

export default ObjectiveListItem
