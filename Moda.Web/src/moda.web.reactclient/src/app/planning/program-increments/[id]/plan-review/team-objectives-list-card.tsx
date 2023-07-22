import { ProgramIncrementObjectiveListDto } from '@/src/services/moda-api'
import { PlusOutlined } from '@ant-design/icons'
import { Button, Card, Empty, List, Space } from 'antd'
import ObjectiveListItem from './objective-list-item'
import ModaEmpty from '@/src/app/components/common/moda-empty'

export interface TeamObjectivesListCardProps {
  objectives: ProgramIncrementObjectiveListDto[]
  teamId: string
}

const TeamObjectivesListCard = ({
  objectives,
  teamId,
}: TeamObjectivesListCardProps) => {
  const cardTitle = () => {
    let title = `Objectives`
    if (objectives?.length > 0) {
      title += ` (${objectives.length})`
    }
    return title
  }

  const ObjectivesList = () => {
    if (!objectives || objectives.length === 0) {
      return <ModaEmpty message="No objectives" />
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
      title={cardTitle()}
      extra={<Button type="text" icon={<PlusOutlined />} />}
    >
      <ObjectivesList />
    </Card>
  )
}

export default TeamObjectivesListCard
