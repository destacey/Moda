import { ModaEmpty } from '@/src/app/components/common'
import { WorkTypeLevelDto, WorkTypeTierDto } from '@/src/services/moda-api'
import { Card, List, Typography } from 'antd'
import { useEffect, useState } from 'react'
import WorkTypeLevelCard from './work-type-level-card'

const { Text } = Typography

interface WorkTypeTierCardProps {
  tier: WorkTypeTierDto
  levels: WorkTypeLevelDto[]
  canCreateWorkTypeLevels: boolean
  canUpdateWorkTypeLevels: boolean
}

const WorkTypeTierCard = (props: WorkTypeTierCardProps) => {
  const [orderedLevels, setOrderedLevels] = useState<WorkTypeLevelDto[]>([])

  const canCreateNew = props.tier.name === 'Portfolio'

  useEffect(() => {
    if (!props.levels) return
    setOrderedLevels(props.levels.sort((a, b) => a.order - b.order))
  }, [props.levels])

  return (
    <Card size="small" title={props.tier.name}>
      <Text>{props.tier.description}</Text>
      <List
        size="small"
        dataSource={orderedLevels}
        locale={{
          emptyText: <ModaEmpty message="No work type levels" />,
        }}
        renderItem={(level) => (
          <WorkTypeLevelCard
            level={level}
            canUpdateWorkTypeLevels={props.canUpdateWorkTypeLevels}
          />
        )}
      />
    </Card>
  )
}

export default WorkTypeTierCard
