import { MarkdownRenderer } from '@/src/components/common/markdown'
import { RoadmapActivityDetailsDto } from '@/src/services/moda-api'
import { ColorPicker, Descriptions, Space } from 'antd'
import dayjs from 'dayjs'

const { Item: DescriptionsItem } = Descriptions

interface RoadmapActivityDrawerItemProps {
  activity: RoadmapActivityDetailsDto
  openRoadmapItemDrawer: (itemId: string) => void
}

const RoadmapActivityDrawerItem: React.FC<RoadmapActivityDrawerItemProps> = (
  props: RoadmapActivityDrawerItemProps,
) => {
  const { activity } = props

  return (
    <Space direction="vertical">
      <Descriptions column={1}>
        <DescriptionsItem label="Name">{activity.name}</DescriptionsItem>
        <DescriptionsItem label="Type">{activity.type.name}</DescriptionsItem>
        <DescriptionsItem label="Parent">
          {activity.parent && (
            <a onClick={() => props.openRoadmapItemDrawer(activity.parent?.id)}>
              {activity.parent?.name}
            </a>
          )}
        </DescriptionsItem>
        <DescriptionsItem label="Color">
          {activity?.color && (
            <ColorPicker
              defaultValue={activity.color}
              size="small"
              showText
              disabled
            />
          )}
        </DescriptionsItem>
        <DescriptionsItem label="Start">
          {dayjs(activity.start).format('MMM D, YYYY')}
        </DescriptionsItem>
        <DescriptionsItem label="End">
          {dayjs(activity.end).format('MMM D, YYYY')}
        </DescriptionsItem>
      </Descriptions>
      <Descriptions column={1} layout="vertical" style={{ paddingTop: 8 }}>
        <DescriptionsItem label="Description">
          <MarkdownRenderer markdown={activity?.description} />
        </DescriptionsItem>
      </Descriptions>
    </Space>
  )
}

export default RoadmapActivityDrawerItem
