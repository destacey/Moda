'use client'

import { useMessage } from '@/src/components/contexts/messaging'
import { authorizeForm } from '@/src/components/hoc'
import { StrategicInitiativeKpiListDto } from '@/src/services/moda-api'
import { useDeleteStrategicInitiativeKpiMutation } from '@/src/store/features/ppm/strategic-initiatives-api'
import { Modal } from 'antd'
import { FC, useEffect, useState } from 'react'

export interface DeleteStrategicInitiativeKpiFormProps {
  strategicInitiativeId: string
  kpi: StrategicInitiativeKpiListDto
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
}

const DeleteStrategicInitiativeKpiForm = (
  props: DeleteStrategicInitiativeKpiFormProps,
) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)

  const messageApi = useMessage()

  const [deleteKpiMutation, { error: mutationError }] =
    useDeleteStrategicInitiativeKpiMutation()

  const formAction = async (kpi: StrategicInitiativeKpiListDto) => {
    try {
      const response = await deleteKpiMutation({
        strategicInitiativeId: props.strategicInitiativeId,
        kpiId: kpi.id,
      })

      if (response.error) {
        throw response.error
      }

      return true
    } catch (error) {
      messageApi.error(
        error.detail ?? 'An unexpected error occurred while deleting the KPI.',
      )
      console.log(error)
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      if (await formAction(props.kpi)) {
        // TODO: not working because the parent page is gone
        messageApi.success('Successfully deleted KPI.')
        props.onFormComplete()
        setIsOpen(false)
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
      messageApi.error('An unexpected error occurred while deleting the KPI.')
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = () => {
    setIsOpen(false)
    props.onFormCancel()
  }

  useEffect(() => {
    if (!props.kpi) return

    setIsOpen(props.showForm)
  }, [props.showForm, props.kpi])

  return (
    <>
      <Modal
        title="Are you sure you want to delete this KPI?"
        open={isOpen}
        onOk={handleOk}
        okText="Delete"
        okType="danger"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnHidden={true}
      >
        {props.kpi?.key} - {props.kpi?.name}
      </Modal>
    </>
  )
}

const AuthorizedDeleteStrategicInitiativeKpiForm: FC<
  DeleteStrategicInitiativeKpiFormProps
> = (props) => {
  const AuthorizedForm = authorizeForm(
    DeleteStrategicInitiativeKpiForm,
    props.onFormCancel,
    'Permission',
    'Permissions.StrategicInitiatives.Update',
  )

  return <AuthorizedForm {...props} />
}

export default AuthorizedDeleteStrategicInitiativeKpiForm
