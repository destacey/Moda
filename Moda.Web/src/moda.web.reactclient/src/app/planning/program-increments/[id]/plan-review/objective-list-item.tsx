import { ProgramIncrementObjectiveListDto } from '@/src/services/moda-api'
import { List, Typography } from 'antd'
import dayjs from 'dayjs'
import Link from 'next/link'

export interface ObjectiveListItemProps {
  objective: ProgramIncrementObjectiveListDto
  piLocalId: number
}

const ObjectiveListItem = ({
  objective,
  piLocalId,
}: ObjectiveListItemProps) => {
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
    const content = `Stretch?: ${objective.isStretch}`
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
    return (
      <>
        <Typography.Text>
          {content}
          {startDate}
          {targetDate}
        </Typography.Text>
      </>
    )
  }

  return (
    <List.Item key={objective.localId}>
      <List.Item.Meta title={title()} description={description()} />
    </List.Item>
  )
}

export default ObjectiveListItem
