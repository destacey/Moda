'use client'

import { Form, Input, Modal } from 'antd'
import { useEffect, useState } from 'react'
import useAuth from '@/src/components/contexts/auth'
import { toFormErrors } from '@/src/utils'
import { CreateOrUpdateRoleRequest, RoleDto } from '@/src/services/moda-api'
import { useUpsertRoleMutation } from '@/src/store/features/user-management/roles-api'
import { useMessage } from '@/src/components/contexts/messaging'

const { Item } = Form
const { TextArea } = Input

export interface EditRoleFormProps {
  showForm: boolean
  role: RoleDto
  onFormComplete: () => void
  onFormCancel: () => void
}

const EditRoleForm = ({
  showForm,
  role,
  onFormComplete,
  onFormCancel,
}: EditRoleFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreateOrUpdateRoleRequest>()
  const formValues = Form.useWatch([], form)
  const messageApi = useMessage()

  const { hasClaim } = useAuth()
  const editableRole = role && role.name !== 'Admin' && role.name !== 'Basic'
  const canUpdate =
    hasClaim('Permission', 'Permissions.Roles.Update') && editableRole

  const [upsertRole, { error: upsertRoleError }] = useUpsertRoleMutation()

  useEffect(() => {
    if (!role) return
    if (canUpdate) {
      setIsOpen(showForm)
      form.setFieldsValue({
        id: role.id,
        name: role.name,
        description: role.description,
      })
    } else {
      onFormCancel()
      messageApi.error('You do not have permission to edit role.')
    }
  }, [canUpdate, form, messageApi, onFormCancel, role, showForm])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  const update = async (values: CreateOrUpdateRoleRequest) => {
    try {
      const response = await upsertRole({
        id: role.id,
        name: values.name,
        description: values.description,
      })

      if (response.error) {
        throw response.error
      }

      return true
    } catch (error: any) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else if (error.statusCode === 409 && error.exception) {
        messageApi.error(error.exception)
      } else {
        messageApi.error(
          'An unexpected error occurred while updating the role.',
        )
      }

      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (await update(values)) {
        setIsOpen(false)
        setIsSaving(false)
        form.resetFields()
        onFormComplete()
        messageApi.success('Successfully updated Role.')
      } else {
        setIsSaving(false)
      }
    } catch (errorInfo) {
      setIsSaving(false)
    }
  }

  const handleCancel = () => {
    setIsOpen(false)
    onFormCancel()
    form.resetFields()
  }

  return (
    <Modal
      title="Edit Role"
      open={isOpen}
      onOk={handleOk}
      okButtonProps={{ disabled: !isValid }}
      okText="Save"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      maskClosable={false}
      keyboard={false} // disable esc key to close modal
      destroyOnHidden={true}
    >
      <Form form={form} size="small" layout="vertical" name="edit-role-form">
        <Item label="Name" name="name" rules={[{ required: true }]}>
          <TextArea
            autoSize={{ minRows: 1, maxRows: 4 }}
            showCount
            maxLength={256}
          />
        </Item>

        <Item name="description" label="Description">
          <TextArea
            autoSize={{ minRows: 6, maxRows: 10 }}
            showCount
            maxLength={1024}
          />
        </Item>
      </Form>
    </Modal>
  )
}

export default EditRoleForm
