'use client'

import { LinkDto, UpdateLinkRequest } from '@/src/services/moda-api'
import { Form, Input, Modal } from 'antd'
import { useEffect } from 'react'
import { toFormErrors } from '@/src/utils'
import {
  useGetLinkQuery,
  useUpdateLinkMutation,
} from '@/src/store/features/common/links-api'
import { useMessage } from '../../contexts/messaging'
import { useModalForm } from '@/src/hooks'

const { Item } = Form
const { TextArea } = Input

export interface EditLinkFormProps {
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
  id,
  onFormUpdate,
  onFormCancel,
}: EditLinkFormProps) => {
  const messageApi = useMessage()

  const { data: linkData } = useGetLinkQuery(id, { skip: !id })
  const [updateLink] = useUpdateLinkMutation()

  const { form, isOpen, isValid, isSaving, handleOk, handleCancel } =
    useModalForm<EditLinkFormValues>({
      onSubmit: async (values: EditLinkFormValues, form) => {
        try {
          const request = mapToRequestValues(values)
          request.id = id
          const response = await updateLink({ request, objectId: id })
          if (response.error) {
            throw response.error
          }

          messageApi.success('Successfully updated link.')
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
      },
      onComplete: onFormUpdate,
      onCancel: onFormCancel,
      permission: 'Permissions.Links.Update',
    })

  useEffect(() => {
    if (!linkData || !isOpen) return
    const mapToFormValues = (link: LinkDto) => {
      form.setFieldsValue({
        name: link.name,
        url: link.url,
      })
    }
    mapToFormValues(linkData)
  }, [form, linkData, isOpen])

  return (
    <Modal
      title="Edit Link"
      open={isOpen}
      onOk={handleOk}
      okButtonProps={{ disabled: !isValid }}
      okText="Save"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false}
      destroyOnHidden
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
  )
}

export default EditLinkForm
