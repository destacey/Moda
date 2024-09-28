import useAuth from '@/src/app/components/contexts/auth'
import { CreateOrUpdateRoleRequest, RoleDto } from '@/src/services/moda-api'
import {
  useDeleteRoleMutation,
  useCreateRoleMutation,
} from '@/src/services/queries/user-management-queries'
import { toFormErrors } from '@/src/utils'
import { Button, Form, Input, Popconfirm, Space, message } from 'antd'
import { useRouter } from 'next/navigation'
import React, { useEffect, useState } from 'react'

const { Item } = Form
const { TextArea } = Input

interface RolesDetailProps {
  role: RoleDto
}

const RoleDetails = (props: RolesDetailProps) => {
  const [role, setRole] = useState<RoleDto>(props.role)
  const [form] = Form.useForm<CreateOrUpdateRoleRequest>()
  const router = useRouter()
  const { hasClaim } = useAuth()

  const useDeleteRole = useDeleteRoleMutation()
  const useCreateRole = useCreateRoleMutation()
  const canDelete = hasClaim('Permission', 'Permissions.Roles.Delete')
  const [messageApi, contextHolder] = message.useMessage()

  useEffect(() => setRole(props.role), [props.role])

  const confirmDelete = async () => {
    try {
      await useDeleteRole.mutateAsync(role.id)
      messageApi.success('Role deleted successfully')

      // Allow delay for message to show
      setTimeout(() => {
        router.push('/settings/roles')
      }, 1500)
    } catch (error) {
      if (error.statusCode === 409 && error.supportMessage) {
        messageApi.error(error.supportMessage)
      } else {
        messageApi.error(error?.messages?.join() ?? 'Failed to delete role')
      }
    }
  }

  const onFinish = async (values: any) => {
    try {
      await useCreateRole.mutateAsync({
        id: role?.id,
        name: values.name,
        description: values.description,
      })

      messageApi.success('Role saved successfully')
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else if (error.statusCode === 409 && error.exception) {
        messageApi.error(error.exception)
      } else {
        messageApi.error(
          'An unexpected error occurred while updating the Risk.',
        )
      }
    }
  }

  const onFinishFailed = () => {
    messageApi.error(`Failed to save role`)
  }

  return (
    <div>
      {contextHolder}
      {role && (
        <Form
          form={form}
          labelCol={{ span: 8 }}
          wrapperCol={{ span: 16 }}
          style={{ maxWidth: 600 }}
          onFinish={onFinish}
          onFinishFailed={onFinishFailed}
          autoComplete="off"
        >
          <Item
            label="Name"
            name="name"
            initialValue={role?.name}
            rules={[{ required: true }]}
          >
            <TextArea
              autoSize={{ minRows: 1, maxRows: 4 }}
              showCount
              maxLength={256}
            />
          </Item>

          <Item
            label="Description"
            name="description"
            initialValue={role?.description}
          >
            <TextArea
              autoSize={{ minRows: 6, maxRows: 10 }}
              showCount
              maxLength={1024}
            />
          </Item>
          <Item wrapperCol={{ offset: 8, span: 16 }}>
            <Space style={{ display: 'flex', justifyContent: 'space-between' }}>
              <Button type="primary" htmlType="submit">
                Save
              </Button>

              {canDelete && (
                <Popconfirm
                  title="Delete Role"
                  description="Are you sure to delete this role?"
                  onConfirm={confirmDelete}
                  okText="Yes"
                  cancelText="No"
                >
                  <Button type="text" danger>
                    Delete
                  </Button>
                </Popconfirm>
              )}
            </Space>
          </Item>
        </Form>
      )}
    </div>
  )
}

export default RoleDetails
