'use client'

import { Form, Input, Modal, Select, message } from 'antd'
import { useEffect, useState } from 'react'
import useAuth from '../../components/contexts/auth'
import { toFormErrors } from '@/src/utils'
import { RoleDto } from '@/src/services/moda-api'
import {
  useCreateRoleMutation,
  useGetRoleById,
  useUpdatePermissionsMutation,
} from '@/src/services/queries/user-management-queries'
import { useQueryClient } from 'react-query'

export interface CreateRoleFormProps {
  showForm: boolean
  roles: RoleDto[]
  onFormCreate: (id: string) => void
  onFormCancel: () => void
}

interface CreateRoleFormValues {
  name: string
  description: string
}

const CreateRoleForm = ({
  showForm,
  roles,
  onFormCreate,
  onFormCancel,
}: CreateRoleFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [roleIdToCopyPermissions, setRoleIdToCopyPermissions] =
    useState<string>('')
  const [form] = Form.useForm<CreateRoleFormValues>()
  const formValues = Form.useWatch([], form)
  const [messageApi, contextHolder] = message.useMessage()

  const { hasClaim } = useAuth()
  const roleData = useGetRoleById(roleIdToCopyPermissions)
  const canCreate = hasClaim('Permission', 'Permissions.Roles.Create')

  const queryClient = useQueryClient()
  const createRole = useCreateRoleMutation(queryClient)
  const updatePermissions = useUpdatePermissionsMutation(queryClient)

  useEffect(() => {
    if (canCreate) {
      setIsOpen(showForm)
    } else {
      onFormCancel()
      messageApi.error('You do not have permission to create role.')
    }
  }, [canCreate, onFormCancel, showForm, messageApi])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  const create = async (values: CreateRoleFormValues): Promise<string> => {
    try {
      var id = await createRole.mutateAsync({
        name: values.name,
        description: values.description,
      })

      if (roleIdToCopyPermissions && roleData.data.permissions) {
        await updatePermissions.mutateAsync({
          roleId: id,
          permissions: roleData.data.permissions,
        })
      }

      return id
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          `An unexpected error occurred while creating the role.`,
        )
      }

      return null
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      const id = await create(values)

      if (id) {
        setIsOpen(false)
        setIsSaving(false)
        form.resetFields()
        onFormCreate(id)
        messageApi.success(`Successfully created Role.`)
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
    <>
      {contextHolder}
      <Modal
        title="Create Role"
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
          name="create-role-form"
        >
          <Form.Item label="Name" name="name" rules={[{ required: true }]}>
            <Input.TextArea
              autoSize={{ minRows: 1, maxRows: 4 }}
              showCount
              maxLength={256}
            />
          </Form.Item>

          <Form.Item name="description" label="Description">
            <Input.TextArea
              autoSize={{ minRows: 6, maxRows: 10 }}
              showCount
              maxLength={1024}
            />
          </Form.Item>

          <Form.Item name="isDefault" label="Copy Permissions From">
            {roles && (
              <Select
                allowClear
                showSearch
                placeholder="Select a Role"
                optionFilterProp="children"
                onChange={setRoleIdToCopyPermissions}
                filterOption={(input, option) =>
                  (option?.label ?? '')
                    .toLowerCase()
                    .includes(input.toLowerCase())
                }
                options={
                  roles?.map((role) => ({
                    value: role.id,
                    label: role.name,
                  })) ?? []
                }
              />
            )}
          </Form.Item>
        </Form>
      </Modal>
    </>
  )
}

export default CreateRoleForm
