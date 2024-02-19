'use client'

import { CreateLinkRequest } from '@/src/services/moda-api'
import { Form, Input, Modal, message } from 'antd'
import { useEffect, useState } from 'react'
import useAuth from '../../contexts/auth'
import { useCreateLinkMutation } from '@/src/services/queries/link-queries'
import { toFormErrors } from '@/src/utils'

const { Item } = Form
const { TextArea } = Input

export interface CreateLinkFormProps {
  showForm: boolean
  objectId: string
  onFormCreate: () => void
  onFormCancel: () => void
}

interface CreateLinkFormValues {
  objectId: string
  name: string
  url: string
}

const mapToRequestValues = (values: CreateLinkFormValues) => {
  return {
    name: values.name,
    url: values.url,
  } as CreateLinkRequest
}

const CreateLinkForm = ({
  showForm,
  objectId,
  onFormCreate,
  onFormCancel,
}: CreateLinkFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreateLinkFormValues>()
  const formValues = Form.useWatch([], form)
  const [messageApi, contextHolder] = message.useMessage()

  const { hasClaim } = useAuth()
  const canCreateLinks = hasClaim('Permission', 'Permissions.Links.Create')

  const createLink = useCreateLinkMutation()

  const create = async (values: CreateLinkFormValues): Promise<boolean> => {
    try {
      const request = mapToRequestValues(values)
      request.objectId = objectId
      await createLink.mutateAsync(request)
      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          'An unexpected error occurred while creating the link.',
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
        onFormCreate()
        messageApi.success('Successfully created link.')
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = () => {
    setIsOpen(false)
    onFormCancel()
    form.resetFields()
  }

  useEffect(() => {
    if (canCreateLinks) {
      setIsOpen(showForm)
    } else {
      onFormCancel()
      messageApi.error('You do not have permission to create links.')
    }
  }, [canCreateLinks, onFormCancel, showForm, messageApi])

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
        title="Create Link"
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
          name="create-link-form"
        >
          <Item label="Name" name="name" rules={[{ required: true }]}>
            <TextArea
              autoSize={{ minRows: 1, maxRows: 2 }}
              showCount
              maxLength={128}
            />
          </Item>
          <Item
            name="url"
            label="URL"
            rules={[{ required: true }, { type: 'url' }]}
          >
            <TextArea autoSize={{ minRows: 6, maxRows: 10 }} />
          </Item>
        </Form>
      </Modal>
    </>
  )
}

export default CreateLinkForm
