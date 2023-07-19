import useAuth from '@/src/app/components/contexts/auth'
import { getRolesClient } from '@/src/services/clients'
import { RoleDto } from '@/src/services/moda-api'
import { useDeleteRoleMutation, useCreateRoleMutation } from '@/src/services/query'
import { Button, Checkbox, Form, Input, Popconfirm, message } from 'antd'
import { get } from 'lodash'
import { useRouter } from 'next/navigation'
import React, { useEffect, useState } from 'react'
import { useQueryClient } from 'react-query'

interface RolesDetailProps {
  role: RoleDto
}

const Detail = (props: RolesDetailProps) => {
  const [role, setRole] = useState<RoleDto>(props.role)
  const router = useRouter()
  const { hasClaim } = useAuth()

  const queryClient = useQueryClient();
  const useDeleteRole = useDeleteRoleMutation(queryClient);
  const useCreateRole = useCreateRoleMutation(queryClient);
  const canDelete = hasClaim('Permission', 'Permissions.Roles.Delete')
  const [messageApi, contextHolder] = message.useMessage()

  useEffect(() => setRole(props.role), [props.role])

  const confirmDelete = async (e: React.MouseEvent<HTMLElement>) => {
    try {
      await useDeleteRole.mutateAsync(role.id);
      messageApi.success('Role deleted successfully');

      // Allow delay for message to show
      setTimeout(() => {
        router.push('/settings/roles');
      }, 1500);
    } catch(error) {
      messageApi.error(error?.messages?.join() ?? 'Failed to delete role');
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
      messageApi.error('Finished catch', error.messages.join())
    }
  }

  const onFinishFailed = (errorInfo: any) => {
    messageApi.error(`Failed to save role`)
  }

  return (
    <div>
      {contextHolder}
      {role && (
        <Form
          labelCol={{ span: 8 }}
          wrapperCol={{ span: 16 }}
          style={{ maxWidth: 600 }}
          onFinish={onFinish}
          onFinishFailed={onFinishFailed}
          autoComplete="off"
        >
          <Form.Item
            label="Name"
            name="name"
            initialValue={role?.name}
            rules={[{ required: true, message: 'Please input a name' }]}
          >
            <Input />
          </Form.Item>

          <Form.Item
            label="Description"
            name="description"
            initialValue={role?.description}
            rules={[{ required: false }]}
          >
            <Input.TextArea />
          </Form.Item>
          <Form.Item wrapperCol={{ offset: 8, span: 16 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between' }}>
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
                  <Button type='text' danger>Delete</Button>
                </Popconfirm>
              )}
            </div>
          </Form.Item>
        </Form>
      )}
    </div>
  )
}

export default Detail
