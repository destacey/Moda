import {
  RoadmapActivityDetailsDto,
  RoadmapMilestoneDetailsDto,
  RoadmapTimeboxDetailsDto,
} from '@/src/services/moda-api'
import { useGetRoadmapItemQuery } from '@/src/store/features/planning/roadmaps-api'
import { getDrawerWidthPercentage } from '@/src/utils/window-utils'
import { Button, Drawer, Spin } from 'antd'
import {
  EditRoadmapActivityForm,
  EditRoadmapTimeboxForm,
  RoadmapActivityDrawerItem,
  RoadmapMilestoneDrawerItem,
  RoadmapTimeboxDrawerItem,
} from '.'
import { FC, useEffect, useState } from 'react'
import useAuth from '@/src/components/contexts/auth'

interface RoadmapItemDrawerProps {
  roadmapId: string
  roadmapItemId: string
  drawerOpen: boolean
  onDrawerClose: () => void
  openRoadmapItemDrawer: (itemId: string) => void
}

const RoadmapItemDrawer: FC<RoadmapItemDrawerProps> = (
  props: RoadmapItemDrawerProps,
) => {
  const [openEditRoadmapItemForm, setOpenEditRoadmapItemForm] =
    useState<boolean>(false)

  const {
    data: itemData,
    isLoading,
    error,
    refetch: refetchItem,
  } = useGetRoadmapItemQuery({
    roadmapId: props.roadmapId,
    itemId: props.roadmapItemId,
  })

  const { hasPermissionClaim } = useAuth()
  const canUpdateRoadmap = hasPermissionClaim('Permissions.Roadmaps.Update')

  useEffect(() => {
    error && console.error(error)
  }, [error])

  return (
    <>
      <Drawer
        title="Roadmap Item Details"
        placement="right"
        onClose={props.onDrawerClose}
        open={props.drawerOpen}
        destroyOnHidden={true}
        width={getDrawerWidthPercentage()}
        extra={
          canUpdateRoadmap && (
            <Button onClick={() => setOpenEditRoadmapItemForm(true)}>
              Edit
            </Button>
          )
        }
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
      {itemData?.type.name === 'Activity' && openEditRoadmapItemForm && (
        <EditRoadmapActivityForm
          showForm={openEditRoadmapItemForm}
          activityId={itemData.id}
          roadmapId={props.roadmapId}
          onFormComplete={() => setOpenEditRoadmapItemForm(false)}
          onFormCancel={() => setOpenEditRoadmapItemForm(false)}
        />
      )}
      {itemData?.type.name === 'Timebox' && openEditRoadmapItemForm && (
        <EditRoadmapTimeboxForm
          showForm={openEditRoadmapItemForm}
          timeboxId={itemData.id}
          roadmapId={props.roadmapId}
          onFormComplete={() => setOpenEditRoadmapItemForm(false)}
          onFormCancel={() => setOpenEditRoadmapItemForm(false)}
        />
      )}
    </>
  )
}

export default RoadmapItemDrawer
