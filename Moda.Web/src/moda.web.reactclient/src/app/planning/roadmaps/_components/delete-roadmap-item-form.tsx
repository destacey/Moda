import useAuth from '@/src/components/contexts/auth'
import { RoadmapItemListDto } from '@/src/services/moda-api'
import {
  useDeleteRoadmapItemMutation,
  useGetRoadmapItemQuery,
} from '@/src/store/features/planning/roadmaps-api'
import { Modal } from 'antd'
import { MessageInstance } from 'antd/es/message/interface'
import { useEffect, useState } from 'react'

export interface DeleteRoadmapItemFormProps {
  roadmapId: string
  roadmapItemId: string
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
  messageApi: MessageInstance
}

const DeleteRoadmapItemForm = (props: DeleteRoadmapItemFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)

  const [deleteRoadmapItemMutation, { error: mutationError }] =
    useDeleteRoadmapItemMutation()

  const { hasPermissionClaim } = useAuth()
  const canDeleteRoadmapItem = hasPermissionClaim('Permissions.Roadmaps.Update')

  const {
    data: itemData,
    isLoading: itemDataIsLoading,
    error: itemDataError,
  } = useGetRoadmapItemQuery({
    roadmapId: props.roadmapId,
    itemId: props.roadmapItemId,
  })

  const deleteRoadmapItem = async (roadmapItem: RoadmapItemListDto) => {
    try {
      await deleteRoadmapItemMutation({
        roadmapId: roadmapItem.roadmapId,
        itemId: roadmapItem.id,
      })
      return true
    } catch (error) {
      props.messageApi.error(
        'An unexpected error occurred while deleting the roadmap.',
      )
      console.log(error)
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      if (await deleteRoadmapItem(itemData)) {
        props.messageApi.success('Successfully deleted Roadmap Item.')
        setIsOpen(false)
        props.onFormComplete()
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
      props.messageApi.error(
        'An unexpected error occurred while deleting the roadmap item.',
      )
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = () => {
    setIsOpen(false)
    props.onFormCancel()
  }

  useEffect(() => {
    if (!itemData || itemDataIsLoading) return
    if (canDeleteRoadmapItem) {
      setIsOpen(props.showForm)
    } else {
      props.onFormCancel()
      props.messageApi.error(
        'You do not have permission to delete roadmap items.',
      )
    }
  }, [canDeleteRoadmapItem, itemData, itemDataIsLoading, props])

  useEffect(() => {
    if (itemDataError) {
      props.messageApi.error(
        itemDataError.supportMessage ??
          'An error occurred while loading roadmap item. Please try again.',
      )
    }
  }, [itemDataError, props.messageApi])

  return (
    <>
      <Modal
        title="Are you sure you want to delete this Roadmap Item?"
        open={isOpen}
        onOk={handleOk}
        okText="Delete"
        okType="danger"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnClose={true}
      >
        {itemData?.name}
      </Modal>
    </>
  )
}

export default DeleteRoadmapItemForm
