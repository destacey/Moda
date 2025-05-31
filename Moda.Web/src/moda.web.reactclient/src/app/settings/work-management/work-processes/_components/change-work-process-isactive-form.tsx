import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { useChangeWorkProcessIsActiveMutation } from '@/src/store/features/work-management/work-process-api'
import { Modal, Typography } from 'antd'
import { useEffect, useState } from 'react'

const { Text } = Typography

export interface ChangeWorkProcessIsActiveFormProps {
  showForm: boolean
  workProcessId: string
  workProcessName: string
  isActive: boolean
  onFormSave: () => void
  onFormCancel: () => void
}

const ChangeWorkProcessIsActiveForm = (
  props: ChangeWorkProcessIsActiveFormProps,
) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const messageApi = useMessage()

  const action = props.isActive ? 'Deactivate' : 'Activate'
  const actionLowerCase = action.toLowerCase()

  const { hasClaim } = useAuth()
  const canUpdateConnection = hasClaim(
    'Permission',
    'Permissions.WorkProcesses.Update',
  )

  const [changeWorkProcessIsActive] = useChangeWorkProcessIsActiveMutation()

  const init = async (): Promise<boolean> => {
    try {
      const request = {
        id: props.workProcessId,
        isActive: !props.isActive,
      }
      const response = await changeWorkProcessIsActive(request)
      if (response.error) {
        throw response.error
      }
      return true
    } catch (error) {
      messageApi.error(
        `Failed to ${actionLowerCase} work process. Error: ${error.detail}`,
      )
      console.error(error)
      // }
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      if (await init()) {
        setIsOpen(false)
        props.onFormSave()
        messageApi.success(`Successfully ${actionLowerCase}d work process.`)
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = () => {
    setIsOpen(false)
    props.onFormCancel()
  }

  useEffect(() => {
    if (canUpdateConnection) {
      setIsOpen(props.showForm)
    } else {
      props.onFormCancel()
      messageApi.error(
        `You do not have permission to ${actionLowerCase} work processes.`,
      )
    }
  }, [actionLowerCase, canUpdateConnection, messageApi, props])

  if (props.isActive === undefined || props.isActive === null) return null

  return (
    <Modal
      title={`${action} Work Process`}
      open={isOpen}
      onOk={handleOk}
      okText={action}
      confirmLoading={isSaving}
      onCancel={handleCancel}
      maskClosable={false}
      keyboard={false} // disable esc key to close modal
      destroyOnHidden={true}
    >
      <Text>
        {`Are you sure you want to ${actionLowerCase} the work process ${props.workProcessName}?`}
      </Text>
    </Modal>
  )
}

export default ChangeWorkProcessIsActiveForm
