'use client'

import { useMessage } from '@/src/components/contexts/messaging'
import {
  OidcProviderType,
  UpdateOidcProviderRequest,
} from '@/src/services/wayd-api'
import {
  useGetOidcProviderQuery,
  useUpdateOidcProviderMutation,
} from '@/src/store/features/user-management/oidc-providers-api'
import { toFormErrors, isApiError, type ApiError } from '@/src/utils'
import { useModalForm } from '@/src/hooks'
import { Form, Input, InputNumber, Modal, Switch, Typography } from 'antd'
import { useEffect } from 'react'
import TagInput from './tag-input'

const { Item } = Form
const { Text } = Typography

export interface EditOidcProviderFormProps {
  providerId: string
  onFormComplete: () => void
  onFormCancel: () => void
}

interface EditOidcProviderFormValues {
  displayName: string
  authority: string
  clientId: string
  audience: string
  scopes: string[]
  allowedTenantIds?: string[]
  clockSkewSeconds: number
  isEnabled: boolean
}

const EditOidcProviderForm = ({
  providerId,
  onFormComplete,
  onFormCancel,
}: EditOidcProviderFormProps) => {
  const messageApi = useMessage()
  const { data: providerData, isLoading: providerLoading } =
    useGetOidcProviderQuery(providerId)
  const [updateOidcProvider] = useUpdateOidcProviderMutation()

  const isMicrosoftEntraId =
    providerData?.providerType === OidcProviderType.MicrosoftEntraId

  const { form, isOpen, isValid, isSaving, handleOk, handleCancel } =
    useModalForm<EditOidcProviderFormValues>({
      onSubmit: async (values, form) => {
        try {
          const request: UpdateOidcProviderRequest = {
            id: providerId,
            displayName: values.displayName,
            authority: values.authority,
            clientId: values.clientId,
            audience: values.audience,
            scopes: values.scopes ?? [],
            allowedTenantIds: isMicrosoftEntraId
              ? (values.allowedTenantIds ?? [])
              : undefined,
            clockSkewSeconds: values.clockSkewSeconds ?? 60,
            isEnabled: values.isEnabled ?? true,
          }
          const response = await updateOidcProvider(request)
          if (response.error) throw response.error
          messageApi.success('Identity provider updated successfully.')
          return true
        } catch (error) {
          const apiError: ApiError = isApiError(error) ? error : {}
          if (apiError.status === 422 && apiError.errors) {
            form.setFields(toFormErrors(apiError.errors))
            messageApi.error('Correct the validation error(s) to continue.')
          } else {
            messageApi.error(
              apiError.detail ??
                'An error occurred while updating the identity provider.',
            )
          }
          return false
        }
      },
      onComplete: onFormComplete,
      onCancel: onFormCancel,
      permission: 'Permissions.OidcProviders.Update',
    })

  useEffect(() => {
    if (!providerData) return
    form.setFieldsValue({
      displayName: providerData.displayName,
      authority: providerData.authority,
      clientId: providerData.clientId,
      audience: providerData.audience,
      scopes: [...(providerData.scopes ?? [])],
      allowedTenantIds: [...(providerData.allowedTenantIds ?? [])],
      clockSkewSeconds: providerData.clockSkewSeconds,
      isEnabled: providerData.isEnabled,
    })
  }, [providerData, form])

  return (
    <Modal
      title="Edit Identity Provider"
      open={isOpen}
      onOk={handleOk}
      okButtonProps={{ disabled: !isValid }}
      okText="Save"
      loading={providerLoading}
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false}
      destroyOnHidden
      width={640}
    >
      <Form
        form={form}
        size="small"
        layout="vertical"
        name="edit-oidc-provider-form"
      >
        <Item label="Name">
          <Text type="secondary" style={{ fontSize: 12 }}>
            {providerData?.name}
          </Text>
          <Text
            type="secondary"
            style={{ fontSize: 11, display: 'block', marginTop: 2 }}
          >
            Name and provider type are immutable after creation.
          </Text>
        </Item>

        <Item
          label="Display Name"
          name="displayName"
          rules={[
            { required: true, message: 'Display name is required' },
            { max: 128 },
          ]}
        >
          <Input maxLength={128} />
        </Item>

        <Item
          label="Authority"
          name="authority"
          rules={[
            { required: true, message: 'Authority URL is required' },
            { type: 'url', message: 'Must be a valid URL' },
            { max: 256 },
          ]}
        >
          <Input maxLength={256} placeholder="https://..." />
        </Item>

        <Item
          label="Client ID"
          name="clientId"
          rules={[
            { required: true, message: 'Client ID is required' },
            { max: 256 },
          ]}
        >
          <Input maxLength={256} />
        </Item>

        <Item
          label="Audience"
          name="audience"
          rules={[
            { required: true, message: 'Audience is required' },
            { max: 256 },
          ]}
        >
          <Input maxLength={256} />
        </Item>

        <Item label="Scopes" name="scopes">
          <TagInput placeholder="Add scope and press Enter" />
        </Item>

        {isMicrosoftEntraId && (
          <Item
            label="Allowed Tenant IDs"
            name="allowedTenantIds"
            rules={[
              {
                validator: (_, value) =>
                  !value || value.length === 0
                    ? Promise.reject('At least one tenant ID is required')
                    : Promise.resolve(),
              },
            ]}
          >
            <TagInput placeholder="Add tenant ID and press Enter" />
          </Item>
        )}

        <Item
          label="Clock Skew (seconds)"
          name="clockSkewSeconds"
          rules={[{ type: 'number', min: 0, max: 300 }]}
        >
          <InputNumber min={0} max={300} style={{ width: '100%' }} />
        </Item>

        <Item label="Enabled" name="isEnabled" valuePropName="checked">
          <Switch />
        </Item>
      </Form>
    </Modal>
  )
}

export default EditOidcProviderForm
