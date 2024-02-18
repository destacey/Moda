import useAuth from '@/src/app/components/contexts/auth'
import { useChangeWorkProcessIsActiveMutation } from '@/src/services/queries/work-management-queries'
import { Modal, Typography, message } from 'antd'
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
  const [messageApi, contextHolder] = message.useMessage()

  const action = props.isActive ? 'Deactivate' : 'Activate'
  const actionLowerCase = action.toLowerCase()

  const { hasClaim } = useAuth()
  const canUpdateConnection = hasClaim(
    'Permission',
    'Permissions.WorkProcesses.Update',
  )

  const changeWorkProcessIsActiveMutation =
    useChangeWorkProcessIsActiveMutation()

  const init = async (): Promise<boolean> => {
    try {
      const request = {
        id: props.workProcessId,
        isActive: !props.isActive,
      }
      await changeWorkProcessIsActiveMutation.mutateAsync(request)
      messageApi.success('Successfully initialized work process.')
      return true
    } catch (error) {
      messageApi.error(
        `Failed to ${actionLowerCase} work process. Error: ${error.supportMessage}`,
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
    <>
      {contextHolder}
      <Modal
        title={`${action} Work Process`}
        open={isOpen}
        onOk={handleOk}
        okText={action}
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnClose={true}
      >
        <Text>
          {`Are you sure you want to ${actionLowerCase} the work process ${props.workProcessName}?`}
        </Text>
      </Modal>
    </>
  )
}

export default ChangeWorkProcessIsActiveForm
