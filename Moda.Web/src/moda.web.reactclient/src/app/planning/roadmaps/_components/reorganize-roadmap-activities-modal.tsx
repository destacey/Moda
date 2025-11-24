import {
  DraggableTree,
  NodeChangedCallback,
} from '@/src/components/common/draggable-tree'
import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import {
  RoadmapActivityListDto,
  RoadmapItemListDto,
} from '@/src/services/moda-api'
import { useReorganizeRoadmapActivityMutation } from '@/src/store/features/planning/roadmaps-api'
import { Button, Modal, TreeDataNode } from 'antd'
import { useEffect, useMemo, useState } from 'react'

interface RoadmapActivityReorganizeModalProps {
  showModal: boolean
  roadmapId: string
  roadmapItems: RoadmapItemListDto[]
  onClose: () => void
}

interface RoadmpaActivityTreeData extends TreeDataNode {
  id: string
}

const MapRoadmapActivity = (
  item: RoadmapItemListDto,
): RoadmpaActivityTreeData => {
  if (item.$type !== 'activity') return null

  const activity = item as RoadmapActivityListDto

  const children: RoadmapActivityListDto[] = activity.children?.filter(
    (child) => child.$type === 'activity',
  )

  return {
    id: activity.id,
    key: activity.id,
    title: activity.name,
    children: children
      .sort((a, b) => a.order - b.order)
      .map((child) => MapRoadmapActivity(child)),
  }
}

const ReorganizeRoadmapActivitiesModal: React.FC<
  RoadmapActivityReorganizeModalProps
> = (props) => {
  const messageApi = useMessage()

  const { hasPermissionClaim } = useAuth()
  const canManageRoadmapItems = hasPermissionClaim(
    'Permissions.Roadmaps.Update',
  )

  const [reorganizeActivity, { error: reorganizeError }] =
    useReorganizeRoadmapActivityMutation()

  // Derive tree data from props
  const activityTreeData = useMemo(() => {
    if (!props.roadmapItems) return []

    const activities: RoadmapActivityListDto[] = props.roadmapItems.filter(
      (child) => child.$type === 'activity',
    )

    return activities
      .sort((a, b) => a.order - b.order)
      .map((item: RoadmapItemListDto) => MapRoadmapActivity(item))
  }, [props.roadmapItems])

  // Derive modal open state from props and authorization
  const isOpen = props.showModal && canManageRoadmapItems

  // Handle authorization failure - side effect only
  useEffect(() => {
    if (props.showModal && !canManageRoadmapItems) {
      props.onClose()
      messageApi.error('You do not have permission to update roadmap items.')
    }
  }, [props.showModal, canManageRoadmapItems, props, messageApi])

  useEffect(() => {
    if (reorganizeError) {
      console.error('reorganizeActivity error', reorganizeError)
      messageApi.error(
        'An error occurred while reorganizing the roadmap activities. Please refresh and try again.',
      )
    }
  }, [reorganizeError, messageApi])

  const onActivityChanged: NodeChangedCallback = async (
    changedKey,
    parentId,
    index,
  ) => {
    try {
      await reorganizeActivity({
        request: {
          roadmapId: props.roadmapId,
          parentActivityId: parentId?.toString(),
          activityId: changedKey.toString(),
          order: index + 1,
        },
      })

      messageApi.success('Roadmap activities reorganized successfully.')
    } catch (error) {
      console.error('reorganizeActivity error', error)
      messageApi.error(
        'An error occurred while reorganizing the roadmap activities. Please refresh and try again.',
      )
    }
  }

  return (
    <Modal
      title="Reorganize Roadmap Activities"
      open={isOpen}
      footer={[
        <Button key="ok" type="primary" onClick={props.onClose}>
          Close
        </Button>,
      ]}
      onCancel={props.onClose} // X button on top right
      width={'70vw'}
      maskClosable={false}
      keyboard={false} // disable esc key to close modal
      destroyOnHidden={true}
    >
      <DraggableTree
        data={activityTreeData}
        onNodeChanged={onActivityChanged}
      />
    </Modal>
  )
}

export default ReorganizeRoadmapActivitiesModal
