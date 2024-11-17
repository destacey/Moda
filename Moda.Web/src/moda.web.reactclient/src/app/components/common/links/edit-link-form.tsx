'use client'

import { LinkDto, UpdateLinkRequest } from '@/src/services/moda-api'
import { Form, Input, Modal, message } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import useAuth from '../../contexts/auth'
import { toFormErrors } from '@/src/utils'
import {
  useGetLinkQuery,
  useUpdateLinkMutation,
} from '@/src/store/features/common/links-api'

const { Item } = Form
const { TextArea } = Input

export interface EditLinkFormProps {
  showForm: boolean
  id: string
  onFormUpdate: () => void
  onFormCancel: () => void
}

interface EditLinkFormValues {
  objectId: string
  name: string
  url: string
}

const mapToRequestValues = (values: EditLinkFormValues) => {
  return {
    name: values.name,
    url: values.url,
  } as UpdateLinkRequest
}

const EditLinkForm = ({
  showForm,
  id,
  onFormUpdate,
  onFormCancel,
}: EditLinkFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<EditLinkFormValues>()
  const formValues = Form.useWatch([], form)
  const [messageApi, contextHolder] = message.useMessage()

  const { hasPermissionClaim } = useAuth()
  const canUpdateLinks = hasPermissionClaim('Permissions.Links.Update')

  const {
    data: linkData,
    isLoading,
    error,
    refetch,
  } = useGetLinkQuery(id, { skip: !id })
  const [updateLink, { error: updateLinkError }] = useUpdateLinkMutation()

  const mapToFormValues = useCallback(
    (link: LinkDto) => {
      form.setFieldsValue({
        name: link.name,
        url: link.url,
      })
    },
    [form],
  )

  const update = async (values: EditLinkFormValues): Promise<boolean> => {
    try {
      const request = mapToRequestValues(values)
      request.id = id
      const response = await updateLink({ request, objectId: id })
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
          'An unexpected error occurred while updating the link.',
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
      if (await update(values)) {
        messageApi.success('Successfully updated link.')
        setIsOpen(false)
        form.resetFields()
        onFormUpdate()
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = useCallback(() => {
    setIsOpen(false)
    onFormCancel()
    form.resetFields()
  }, [onFormCancel, form])

  const loadData = useCallback(async () => {
    try {
      mapToFormValues(linkData)
      setIsValid(true)
    } catch (error) {
      handleCancel()
      messageApi.error('An unexpected error occurred while loading form data.')
      console.error(error)
    }
  }, [handleCancel, mapToFormValues, messageApi, linkData])

  useEffect(() => {
    if (!linkData) return
    if (canUpdateLinks) {
      setIsOpen(showForm)
      if (showForm) {
        loadData()
      }
    } else {
      onFormCancel()
      messageApi.error('You do not have permission to update links.')
    }
  }, [canUpdateLinks, onFormCancel, showForm, messageApi, linkData, loadData])

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
        title="Edit Link"
        open={isOpen}
        onOk={handleOk}
        okButtonProps={{ disabled: !isValid }}
        okText="Save"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnClose={true}
      >
        <Form form={form} size="small" layout="vertical" name="edit-link-form">
          <Item label="Name" name="name" rules={[{ required: true }]}>
            <TextArea
              autoSize={{ minRows: 1, maxRows: 2 }}
              showCount
              maxLength={128}
            />
          </Item>
          <Item name="url" label="URL">
            <TextArea autoSize={{ minRows: 6, maxRows: 10 }} />
          </Item>
        </Form>
      </Modal>
    </>
  )
}

export default EditLinkForm
