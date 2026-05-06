'use client'

import { Alert, Form, Input, Modal, Typography } from 'antd'
import { toFormErrors } from '@/src/utils'
import { useStageTenantMigrationMutation } from '@/src/store/features/user-management/users-api'
import { useMessage } from '@/src/components/contexts/messaging'
import { useModalForm } from '@/src/hooks'

const { Item } = Form
const { Text } = Typography

const GUID_PATTERN =
  /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/

export interface StageTenantMigrationFormProps {
  userId: string
  userName: string
  currentPendingTenantId?: string | null
  onFormComplete: () => void
  onFormCancel: () => void
}

interface StageTenantMigrationFormValues {
  targetTenantId: string
}

const StageTenantMigrationForm = ({
  userId,
  userName,
  currentPendingTenantId,
  onFormComplete,
  onFormCancel,
}: StageTenantMigrationFormProps) => {
  const messageApi = useMessage()
  const [stageMigration] = useStageTenantMigrationMutation()

  const { form, isOpen, isValid, isSaving, handleOk, handleCancel } =
    useModalForm<StageTenantMigrationFormValues>({
      onSubmit: async (values, form) => {
        try {
          const response = await stageMigration({
            userId,
            targetTenantId: values.targetTenantId.trim(),
          })

          if (response.error) {
            throw response.error
          }

          messageApi.success(
            'Tenant migration staged. Will complete on the user’s next sign-in from the target tenant.',
          )
          return true
        } catch (error: any) {
          if (error.status === 422 && error.errors) {
            const formErrors = toFormErrors(error.errors)
            form.setFields(formErrors)
            messageApi.error('Correct the validation error(s) to continue.')
          } else {
            messageApi.error(
              error.data?.detail ??
                error.detail ??
                'An unexpected error occurred while staging the migration.',
            )
          }
          return false
        }
      },
      onComplete: onFormComplete,
      onCancel: onFormCancel,
      errorMessage:
        'An unexpected error occurred while staging the migration.',
      permission: 'Permissions.Users.Update',
    })

  return (
    <Modal
      title={`Stage Tenant Migration — ${userName}`}
      open={isOpen}
      onOk={handleOk}
      okButtonProps={{ disabled: !isValid }}
      okText={currentPendingTenantId ? 'Replace Pending Migration' : 'Stage Migration'}
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false}
      destroyOnHidden
    >
      <Form
        form={form}
        size="small"
        layout="vertical"
        name="stage-tenant-migration-form"
      >
        {currentPendingTenantId && (
          <Alert
            type="warning"
            showIcon
            title="Migration already pending"
            description={
              <>
                A migration is already staged for tenant{' '}
                <Text code>{currentPendingTenantId}</Text>. Submitting will
                replace it.
              </>
            }
            style={{ marginBottom: 16 }}
          />
        )}
        <Alert
          type="info"
          showIcon
          description="The rebind happens automatically the next time the user signs in from the target tenant. Their UserId, permissions, and links to other records are preserved."
          style={{ marginBottom: 16 }}
        />
        <Item
          label="Target Entra Tenant ID"
          name="targetTenantId"
          extra="The new tenant's directory (tenant) ID — a GUID from Microsoft Entra."
          rules={[
            { required: true, message: 'Target tenant ID is required.' },
            {
              pattern: GUID_PATTERN,
              message: 'Target tenant ID must be a valid GUID.',
            },
          ]}
        >
          <Input placeholder="00000000-0000-0000-0000-000000000000" />
        </Item>
      </Form>
    </Modal>
  )
}

export default StageTenantMigrationForm
