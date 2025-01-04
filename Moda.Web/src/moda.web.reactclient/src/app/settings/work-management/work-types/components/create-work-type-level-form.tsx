import { MarkdownEditor } from '@/src/components/common/markdown'
import useAuth from '@/src/components/contexts/auth'
import { CreateWorkTypeLevelRequest } from '@/src/services/moda-api'
import { useCreateWorkTypeLevelMutation } from '@/src/store/features/work-management/work-type-level-api'
import { toFormErrors } from '@/src/utils'
import { Form, Input, Modal, message } from 'antd'
import { useCallback, useEffect, useState } from 'react'

const { Item } = Form
const { TextArea } = Input

export interface CreateWorkTypeLevelFormProps {
  showForm: boolean
  onFormSave: () => void
  onFormCancel: () => void
}

interface CreateWorkTypeLevelFormValues {
  name: string
  description?: string
}

const mapToRequestValues = (
  values: CreateWorkTypeLevelFormValues,
): CreateWorkTypeLevelRequest => {
  return {
    name: values.name,
    description: values.description,
  }
}

const CreateWorkTypeLevelForm = ({
  showForm,
  onFormSave,
  onFormCancel,
}: CreateWorkTypeLevelFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreateWorkTypeLevelFormValues>()
  const formValues = Form.useWatch([], form)
  const [messageApi, contextHolder] = message.useMessage()

  const [createWorkTypeLevelMutation] = useCreateWorkTypeLevelMutation()

  const { hasClaim } = useAuth()
  const canCreateWorkTypeLevel = hasClaim(
    'Permission',
    'Permissions.WorkTypeLevels.Create',
  )

  const create = async (
    values: CreateWorkTypeLevelFormValues,
  ): Promise<boolean> => {
    try {
      const request = mapToRequestValues(values)
      const response = await createWorkTypeLevelMutation(request)
      if (response.error) {
        throw response.error
      }
      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          'An unexpected error occurred while creating the work type level.',
        )
        console.error(error)
      }
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (await create(values)) {
        setIsOpen(false)
        form.resetFields()
        onFormSave()
        messageApi.success('Successfully created work type level.')
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = useCallback(() => {
    setIsOpen(false)
    form.resetFields()
    onFormCancel()
  }, [form, onFormCancel])

  useEffect(() => {
    if (canCreateWorkTypeLevel) {
      setIsOpen(showForm)
    } else {
      onFormCancel()
      messageApi.error('You do not have permission to create work type levels.')
    }
  }, [canCreateWorkTypeLevel, onFormCancel, showForm, messageApi])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  return (
    <>
      {contextHolder}
      <Modal
        title="Create Work Type Level"
        open={isOpen}
        onOk={handleOk}
        okButtonProps={{ disabled: !isValid }}
        okText="Create"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnClose={true}
      >
        <Form
          form={form}
          size="small"
          layout="vertical"
          name="create-work-type-level-form"
        >
          <Item label="Name" name="name" rules={[{ required: true }]}>
            <TextArea
              autoSize={{ minRows: 1, maxRows: 2 }}
              showCount
              maxLength={128}
            />
          </Item>
          <Item
            name="description"
            label="Description"
            initialValue=""
            rules={[{ max: 1024 }]}
          >
            <MarkdownEditor
              value={form.getFieldValue('description')}
              onChange={(value) =>
                form.setFieldValue('description', value || '')
              }
              maxLength={1024}
            />
          </Item>
        </Form>
      </Modal>
    </>
  )
}

export default CreateWorkTypeLevelForm
