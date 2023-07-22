import { RiskListDto } from '@/src/services/moda-api'
import { PlusOutlined } from '@ant-design/icons'
import { Button, Card, Empty, List } from 'antd'
import RiskListItem from './risk-list-item'

export interface TeamRisksListCardProps {
  risks: RiskListDto[]
  programIncrementId: string
  teamId: string
}

const TeamRisksListCard = ({
  risks,
  programIncrementId,
  teamId,
}: TeamRisksListCardProps) => {
  const RisksList = () => {
    if (!risks || risks.length === 0) {
      return (
        <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="No risks" />
      )
    }

    const sortedRisks = risks.sort((a, b) => {
      const categoryOrder = ['Owned', 'Accepted', 'Mitigated', 'Resolved']
      const aIndex = categoryOrder.indexOf(a.category)
      const bIndex = categoryOrder.indexOf(b.category)
      return aIndex - bIndex
    })

    return (
      <List
        size="small"
        dataSource={sortedRisks}
        renderItem={(risk) => <RiskListItem risk={risk} />}
      />
    )
  }
  return (
    <Card
      size="small"
      title="Risks"
      extra={<Button type="text" icon={<PlusOutlined />} />}
    >
      <RisksList />
    </Card>
  )
}

export default TeamRisksListCard
