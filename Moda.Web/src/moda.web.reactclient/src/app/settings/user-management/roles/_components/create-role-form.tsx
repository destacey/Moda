'use client'

import { Form, Input, Modal, Select } from 'antd'
import { useEffect, useState } from 'react'
import useAuth from '@/src/components/contexts/auth'
import { toFormErrors } from '@/src/utils'
import { RoleDto, UpdateRolePermissionsRequest } from '@/src/services/moda-api'
import {
  useGetRoleQuery,
  useUpdatePermissionsMutation,
  useUpsertRoleMutation,
} from '@/src/store/features/user-management/roles-api'
import { useMessage } from '@/src/components/contexts/messaging'

const { Item } = Form
const { TextArea } = Input

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
  const [roleIdToCopyPermissions, setRoleIdToCopyPermissions] = useState<
    string | null
  >(null)
  const [form] = Form.useForm<CreateRoleFormValues>()
  const formValues = Form.useWatch([], form)
  const messageApi = useMessage()

  const { hasClaim } = useAuth()
  const canCreate = hasClaim('Permission', 'Permissions.Roles.Create')

  const { data: roleData } = useGetRoleQuery(roleIdToCopyPermissions, {
    skip: !roleIdToCopyPermissions,
  })

  const [upsertRole] = useUpsertRoleMutation()
  const [updatePermissions] = useUpdatePermissionsMutation()

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

  const create = async (
    values: CreateRoleFormValues,
  ): Promise<string | null> => {
    try {
      const response = await upsertRole({
        name: values.name,
        description: values.description,
      })

      if (response.error) {
        throw response.error
      }

      if (roleIdToCopyPermissions && roleData.permissions) {
        const request: UpdateRolePermissionsRequest = {
          roleId: response.data,
          permissions: roleData.permissions,
        }
        const updatePermissionsResponse = await updatePermissions(request)
        if (response.error) {
          console.error(
            `Role created but an error was thrown while saving the permissions. Error: ${updatePermissionsResponse.error}`,
          )
        }
      }

      return response.data
    } catch (error: any) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          'An unexpected error occurred while creating the role.',
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
    <Modal
      title="Create Role"
      open={isOpen}
      onOk={handleOk}
      okButtonProps={{ disabled: !isValid }}
      okText="Create"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false}
      destroyOnHidden={true}
    >
      <Form form={form} size="small" layout="vertical" name="create-role-form">
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

        <Item name="isDefault" label="Copy Permissions From">
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
        </Item>
      </Form>
    </Modal>
  )
}

export default CreateRoleForm

