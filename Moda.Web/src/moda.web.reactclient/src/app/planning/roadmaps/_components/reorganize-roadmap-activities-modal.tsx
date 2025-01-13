import {
  DraggableTree,
  NodeChangedCallback,
} from '@/src/components/common/draggable-tree'
import useAuth from '@/src/components/contexts/auth'
import {
  RoadmapActivityListDto,
  RoadmapItemListDto,
} from '@/src/services/moda-api'
import { useReorganizeRoadmapActivityMutation } from '@/src/store/features/planning/roadmaps-api'
import { Button, Modal, TreeDataNode } from 'antd'
import { MessageInstance } from 'antd/es/message/interface'
import { useEffect, useState } from 'react'

interface RoadmapActivityReorganizeModalProps {
  showModal: boolean
  roadmapId: string
  roadmapItems: RoadmapItemListDto[]
  onClose: () => void
  messageApi: MessageInstance
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
  const [isOpen, setIsOpen] = useState(false)
  const [activityTreeData, setActivityTreeData] = useState<TreeDataNode[]>([])

  const { hasPermissionClaim } = useAuth()
  const canManageRoadmapItems = hasPermissionClaim(
    'Permissions.Roadmaps.Update',
  )

  const [reorganizeActivity, { error: reorganizeError }] =
    useReorganizeRoadmapActivityMutation()

  useEffect(() => {
    if (!props.showModal) return

    if (!canManageRoadmapItems) {
      setIsOpen(false)
      props.onClose()
      props.messageApi.error(
        'You do not have permission to update roadmap items.',
      )
    }

    const activities: RoadmapActivityListDto[] = props.roadmapItems?.filter(
      (child) => child.$type === 'activity',
    )

    const treeData = activities
      .sort((a, b) => a.order - b.order)
      .map((item: RoadmapItemListDto) => MapRoadmapActivity(item))

    setActivityTreeData(treeData)

    setIsOpen(true)
  }, [canManageRoadmapItems, props])

  useEffect(() => {
    if (reorganizeError) {
      console.error('reorganizeActivity error', reorganizeError)
      props.messageApi.error(
        'An error occurred while reorganizing the roadmap activities. Please refresh and try again.',
      )
    }
  }, [reorganizeError, props.messageApi])

  const onActivityChanged: NodeChangedCallback = async (
    changedKey,
    parentId,
    index,
  ) => {
    // TODO: remove these after a release or two
    console.log('changedKey', changedKey)
    console.log('parentId', parentId)
    console.log('position', index + 1)

    try {
      await reorganizeActivity({
        request: {
          roadmapId: props.roadmapId,
          parentActivityId: parentId?.toString(),
          activityId: changedKey.toString(),
          order: index + 1,
        },
      })

      props.messageApi.success('Roadmap activities reorganized successfully.')
    } catch (error) {
      console.error('reorganizeActivity error', error)
      props.messageApi.error(
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
      destroyOnClose={true}
    >
      <DraggableTree
        data={activityTreeData}
        onNodeChanged={onActivityChanged}
      />
    </Modal>
  )
}

export default ReorganizeRoadmapActivitiesModal
