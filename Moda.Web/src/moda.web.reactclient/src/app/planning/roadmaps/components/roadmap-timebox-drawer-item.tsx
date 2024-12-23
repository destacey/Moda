import { MarkdownRenderer } from '@/src/app/components/common/markdown'
import { RoadmapTimeboxDetailsDto } from '@/src/services/moda-api'
import { Descriptions, Space } from 'antd'
import dayjs from 'dayjs'

const { Item: DescriptionsItem } = Descriptions

interface RoadmapTimeboxDrawerItemProps {
  timebox: RoadmapTimeboxDetailsDto
  openRoadmapItemDrawer: (itemId: string) => void
}

const RoadmapTimeboxDrawerItem: React.FC<RoadmapTimeboxDrawerItemProps> = (
  props: RoadmapTimeboxDrawerItemProps,
) => {
  const { timebox } = props

  return (
    <Space direction="vertical">
      <Descriptions column={1}>
        <DescriptionsItem label="Name">{timebox.name}</DescriptionsItem>
        <DescriptionsItem label="Type">{timebox.type.name}</DescriptionsItem>
        <DescriptionsItem label="Parent">
          {timebox.parent && (
            <a onClick={() => props.openRoadmapItemDrawer(timebox.parent?.id)}>
              {timebox.parent?.name}
            </a>
          )}
        </DescriptionsItem>
        <DescriptionsItem label="Start">
          {dayjs(timebox.start).format('MMM D, YYYY')}
        </DescriptionsItem>
        <DescriptionsItem label="End">
          {dayjs(timebox.end).format('MMM D, YYYY')}
        </DescriptionsItem>
      </Descriptions>
      <Descriptions column={1} layout="vertical" style={{ paddingTop: 8 }}>
        <DescriptionsItem label="Description">
          <MarkdownRenderer markdown={timebox?.description} />
        </DescriptionsItem>
      </Descriptions>
    </Space>
  )
}

export default RoadmapTimeboxDrawerItem
