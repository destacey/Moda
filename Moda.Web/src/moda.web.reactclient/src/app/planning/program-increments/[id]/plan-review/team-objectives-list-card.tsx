import { ProgramIncrementObjectiveListDto } from '@/src/services/moda-api'
import { PlusOutlined } from '@ant-design/icons'
import { Button, Card, Empty, List, Space } from 'antd'
import ObjectiveListItem from './objective-list-item'

export interface TeamObjectivesListCardProps {
  objectives: ProgramIncrementObjectiveListDto[]
  teamId: string
}

const TeamObjectivesListCard = ({
  objectives,
  teamId,
}: TeamObjectivesListCardProps) => {
  const ObjectivesList = () => {
    if (!objectives || objectives.length === 0) {
      return (
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description="No objectives"
        />
      )
    }

    const sortedObjectives = objectives.sort((a, b) => {
      if (a.isStretch && !b.isStretch) {
        return 1
      } else if (!a.isStretch && b.isStretch) {
        return -1
      } else {
        return a.name.localeCompare(b.name)
      }
    })

    return (
      <List
        size="small"
        dataSource={sortedObjectives}
        renderItem={(objective) => (
          <ObjectiveListItem
            objective={objective}
            piLocalId={objective.programIncrement.localId}
          />
        )}
      />
    )
  }

  return (
    <Card
      size="small"
      title="Objectives"
      extra={<Button type="text" icon={<PlusOutlined />} />}
    >
      <ObjectivesList />
    </Card>
  )
}

export default TeamObjectivesListCard
