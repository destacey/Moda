import { MarkdownRenderer } from '@/src/app/components/common/markdown'
import { RoadmapMilestoneDetailsDto } from '@/src/services/moda-api'
import { Descriptions, Space } from 'antd'
import dayjs from 'dayjs'

const { Item: DescriptionsItem } = Descriptions

interface RoadmapMilestoneDrawerItemProps {
  milestone: RoadmapMilestoneDetailsDto
  openRoadmapItemDrawer: (itemId: string) => void
}

const RoadmapMilestoneDrawerItem: React.FC<RoadmapMilestoneDrawerItemProps> = (
  props: RoadmapMilestoneDrawerItemProps,
) => {
  const { milestone } = props

  return (
    <Space direction="vertical">
      <Descriptions column={1}>
        <DescriptionsItem label="Name">{milestone.name}</DescriptionsItem>
        <DescriptionsItem label="Type">{milestone.type.name}</DescriptionsItem>
        <DescriptionsItem label="Parent">
          {milestone.parent && (
            <a
              onClick={() => props.openRoadmapItemDrawer(milestone.parent?.id)}
            >
              {milestone.parent?.name}
            </a>
          )}
        </DescriptionsItem>
        <DescriptionsItem label="Date">
          {dayjs(milestone.date).format('MMM D, YYYY')}
        </DescriptionsItem>
      </Descriptions>
      <Descriptions column={1} layout="vertical" style={{ paddingTop: 8 }}>
        <DescriptionsItem label="Description">
          <MarkdownRenderer markdown={milestone?.description} />
        </DescriptionsItem>
      </Descriptions>
    </Space>
  )
}

export default RoadmapMilestoneDrawerItem
