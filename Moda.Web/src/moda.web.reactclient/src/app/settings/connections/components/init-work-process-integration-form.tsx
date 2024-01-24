import useAuth from '@/src/app/components/contexts/auth'
import { InitWorkProcessIntegrationRequest } from '@/src/services/moda-api'
import { useInitAzdoBoardsConnectionWorkProcessMutation } from '@/src/services/queries/app-integration-queries'
import { Modal, Typography, message } from 'antd'
import { useEffect, useState } from 'react'

const { Text } = Typography

export interface InitWorkProcessIntegrationFormProps {
  showForm: boolean
  connectionId: string
  externalId: string
  onFormSave: () => void
  onFormCancel: () => void
}

interface InitWorkProcessIntegrationFormValues {}

// const mapToRequestValues = (values: InitWorkProcessIntegrationFormValues) => {
//   return {
//     workspaceKey: values.workspaceKey,
//   } as InitWorkProcessIntegrationRequest
// }

const InitWorkProcessIntegrationForm = (
  props: InitWorkProcessIntegrationFormProps,
) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  //const [isValid, setIsValid] = useState(false)
  // const [form] = Form.useForm<InitWorkProcessIntegrationFormValues>()
  // const formValues = Form.useWatch([], form)
  const [messageApi, contextHolder] = message.useMessage()

  const { hasClaim } = useAuth()
  const canUpdateConnection = hasClaim(
    'Permission',
    'Permissions.Connections.Update',
  )

  const initWorkProcessMutation =
    useInitAzdoBoardsConnectionWorkProcessMutation()

  const init = async (): Promise<boolean> => {
    try {
      // const request = mapToRequestValues(values)
      // request.id = props.connectionId
      // request.externalId = props.externalId

      const request = {
        id: props.connectionId,
        externalId: props.externalId,
      } as InitWorkProcessIntegrationRequest

      await initWorkProcessMutation.mutateAsync(request)
      messageApi.success('Successfully initialized work process.')
      return true
    } catch (error) {
      // if (error.status === 422 && error.errors) {
      //   const formErrors = toFormErrors(error.errors)
      //   form.setFields(formErrors)
      //   messageApi.error('Correct the validation error(s) to continue.')
      // } else {
      messageApi.error(
        `Failed to initialize work process. Error: ${error.supportMessage}`,
      )
      console.error(error)
      // }
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      //const values = await form.validateFields()
      if (await init()) {
        setIsOpen(false)
        //form.resetFields()
        props.onFormSave()
        messageApi.success('Successfully initialized work process.')
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
    //form.resetFields()
  }

  useEffect(() => {
    if (canUpdateConnection) {
      setIsOpen(props.showForm)
    } else {
      props.onFormCancel()
      messageApi.error(
        'You do not have permission to initialize work processes.',
      )
    }
  }, [canUpdateConnection, messageApi, props])

  // useEffect(() => {
  //   form.validateFields({ validateOnly: true }).then(
  //     () => setIsValid(true && form.isFieldsTouched()),
  //     () => setIsValid(false),
  //   )
  // }, [form, formValues])

  return (
    <>
      {contextHolder}
      <Modal
        title="Initialize Work Process"
        open={isOpen}
        onOk={handleOk}
        //okButtonProps={{ disabled: !isValid }}
        okText="Init"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnClose={true}
      >
        <Text>
          You will be redirected to the initialized work process page upon
          success.
        </Text>
        {/* <Form
          form={form}
          size="small"
          layout="vertical"
          name="init-work-process-form"
        >
        </Form> */}
      </Modal>
    </>
  )
}

export default InitWorkProcessIntegrationForm
