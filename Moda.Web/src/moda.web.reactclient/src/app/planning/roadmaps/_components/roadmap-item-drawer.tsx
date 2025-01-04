import {
  RoadmapActivityDetailsDto,
  RoadmapMilestoneDetailsDto,
  RoadmapTimeboxDetailsDto,
} from '@/src/services/moda-api'
import { useGetRoadmapItemQuery } from '@/src/store/features/planning/roadmaps-api'
import { getDrawerWidthPercentage } from '@/src/utils/window-utils'
import { Drawer, Spin } from 'antd'
import {
  RoadmapActivityDrawerItem,
  RoadmapMilestoneDrawerItem,
  RoadmapTimeboxDrawerItem,
} from '.'
import { useEffect } from 'react'

interface RoadmapItemDrawerProps {
  roadmapId: string
  roadmapItemId: string
  drawerOpen: boolean
  onDrawerClose: () => void
  openRoadmapItemDrawer: (itemId: string) => void
}

const RoadmapItemDrawer: React.FC<RoadmapItemDrawerProps> = (
  props: RoadmapItemDrawerProps,
) => {
  const {
    data: itemData,
    isLoading,
    error,
    refetch: refetchItem,
  } = useGetRoadmapItemQuery({
    roadmapId: props.roadmapId,
    itemId: props.roadmapItemId,
  })

  useEffect(() => {
    error && console.error(error)
  }, [error])

  return (
    <Drawer
      title="Roadmap Item Details"
      placement="right"
      onClose={props.onDrawerClose}
      open={props.drawerOpen}
      destroyOnClose={true}
      width={getDrawerWidthPercentage()}
    >
      <Spin spinning={isLoading}>
        {itemData?.type.name === 'Activity' && (
          <RoadmapActivityDrawerItem
            activity={itemData as RoadmapActivityDetailsDto}
            openRoadmapItemDrawer={props.openRoadmapItemDrawer}
          />
        )}
        {itemData?.type.name === 'Timebox' && (
          <RoadmapTimeboxDrawerItem
            timebox={itemData as RoadmapTimeboxDetailsDto}
            openRoadmapItemDrawer={props.openRoadmapItemDrawer}
          />
        )}
        {itemData?.type.name === 'Milestone' && (
          <RoadmapMilestoneDrawerItem
            milestone={itemData as RoadmapMilestoneDetailsDto}
            openRoadmapItemDrawer={props.openRoadmapItemDrawer}
          />
        )}
      </Spin>
    </Drawer>
  )
}

export default RoadmapItemDrawer
