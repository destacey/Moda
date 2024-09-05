import useAuth from '@/src/app/components/contexts/auth'
import { RoadmapDetailsDto } from '@/src/services/moda-api'
import { useDeleteRoadmapMutation } from '@/src/store/features/planning/roadmaps-api'
import { Modal } from 'antd'
import { MessageInstance } from 'antd/es/message/interface'
import { useEffect, useState } from 'react'

interface DeleteRoadmapFormProps {
  roadmap: RoadmapDetailsDto
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
  messageApi: MessageInstance
}

const DeleteRoadmapForm = (props: DeleteRoadmapFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)

  const [deleteRoadmapMutation, { error: mutationError }] =
    useDeleteRoadmapMutation()

  const { hasPermissionClaim } = useAuth()
  const canDeleteRoadmap = hasPermissionClaim('Permissions.Roadmaps.Delete')

  const deleteRoadmap = async (roadmap: RoadmapDetailsDto) => {
    try {
      await deleteRoadmapMutation({ id: roadmap.id, cacheKey: roadmap.key })
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
      if (await deleteRoadmap(props.roadmap)) {
        props.messageApi.success('Successfully deleted Roadmap.')
        setIsOpen(false)
        props.onFormComplete()
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
      props.messageApi.error(
        'An unexpected error occurred while deleting the roadmap.',
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
    if (!props.roadmap) return
    if (canDeleteRoadmap) {
      setIsOpen(props.showForm)
    } else {
      props.onFormCancel()
      props.messageApi.error('You do not have permission to delete roadmaps.')
    }
  }, [canDeleteRoadmap, props])

  return (
    <>
      <Modal
        title="Are you sure you want to delete this Roadmap?"
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
        {props.roadmap?.key} - {props.roadmap?.name}
      </Modal>
    </>
  )
}

export default DeleteRoadmapForm
