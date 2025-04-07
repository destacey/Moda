'use client'

import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { RoadmapDetailsDto } from '@/src/services/moda-api'
import { useDeleteRoadmapMutation } from '@/src/store/features/planning/roadmaps-api'
import { Modal } from 'antd'
import { useEffect, useState } from 'react'

export interface DeleteRoadmapFormProps {
  roadmap: RoadmapDetailsDto
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
}

const DeleteRoadmapForm = (props: DeleteRoadmapFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)

  const messageApi = useMessage()

  const [deleteRoadmapMutation, { error: mutationError }] =
    useDeleteRoadmapMutation()

  const { hasPermissionClaim } = useAuth()
  const canDeleteRoadmap = hasPermissionClaim('Permissions.Roadmaps.Delete')

  const deleteRoadmap = async (roadmap: RoadmapDetailsDto) => {
    try {
      await deleteRoadmapMutation(roadmap.id)
      return true
    } catch (error) {
      messageApi.error(
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
        // TODO: not working because the parent page is gone
        messageApi.success('Successfully deleted Roadmap.')
        props.onFormComplete()
        setIsOpen(false)
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
      messageApi.error(
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
      messageApi.error('You do not have permission to delete roadmaps.')
    }
  }, [canDeleteRoadmap, messageApi, props])

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
