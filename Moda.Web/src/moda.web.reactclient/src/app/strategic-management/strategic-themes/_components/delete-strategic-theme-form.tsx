'use client'

import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { StrategicThemeDetailsDto } from '@/src/services/moda-api'
import { useDeleteStrategicThemeMutation } from '@/src/store/features/strategic-management/strategic-themes-api'
import { Modal } from 'antd'
import { useEffect, useState } from 'react'

export interface DeleteStrategicThemeFormProps {
  strategicTheme: StrategicThemeDetailsDto
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
}

const DeleteStrategicThemeForm = (props: DeleteStrategicThemeFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)

  const messageApi = useMessage()

  const [deleteStrategicThemeMutation, { error: mutationError }] =
    useDeleteStrategicThemeMutation()

  const { hasPermissionClaim } = useAuth()
  const canDeleteStrategicTheme = hasPermissionClaim(
    'Permissions.StrategicThemes.Delete',
  )

  const deleteStrategicTheme = async (
    strategicTheme: StrategicThemeDetailsDto,
  ) => {
    try {
      const response = await deleteStrategicThemeMutation({
        strategicThemeId: strategicTheme.id,
        cacheKey: strategicTheme.key,
      })

      if (response.error) {
        throw response.error
      }

      return true
    } catch (error) {
      messageApi.error(
        error.detail ??
          'An unexpected error occurred while deleting the strategic theme.',
      )
      console.log(error)
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      if (await deleteStrategicTheme(props.strategicTheme)) {
        // TODO: not working because the parent page is gone
        messageApi.success('Successfully deleted Strategic Theme.')
        props.onFormComplete()
        setIsOpen(false)
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
      messageApi.error(
        'An unexpected error occurred while deleting the strategic theme.',
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
    if (!props.strategicTheme) return
    if (canDeleteStrategicTheme) {
      setIsOpen(props.showForm)
    } else {
      props.onFormCancel()
    }
  }, [canDeleteStrategicTheme, props])

  return (
    <>
      <Modal
        title="Are you sure you want to delete this Strategic Theme?"
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
        {props.strategicTheme?.key} - {props.strategicTheme?.name}
      </Modal>
    </>
  )
}

export default DeleteStrategicThemeForm
