import { WorkTypeLevelDto } from '@/src/services/moda-api'
import { List } from 'antd'

const { Item } = List
const { Meta } = Item

interface WorkTypeLevelCardProps {
  level: WorkTypeLevelDto
  canUpdateWorkTypeLevels: boolean
}

const WorkTypeLevelCard = (props: WorkTypeLevelCardProps) => {
  return (
    <Item key={props.level.id}>
      <Meta title={props.level.name} description={props.level.description} />
    </Item>
  )
}

export default WorkTypeLevelCard
