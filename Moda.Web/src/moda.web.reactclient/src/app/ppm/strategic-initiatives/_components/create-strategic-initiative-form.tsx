'use client'

import { MarkdownEditor } from '@/src/components/common/markdown'
import { EmployeeSelect } from '@/src/components/common/organizations'
import { useMessage } from '@/src/components/contexts/messaging'
import { useModalForm } from '@/src/hooks'
import { CreateStrategicInitiativeRequest } from '@/src/services/moda-api'
import { useGetEmployeeOptionsQuery } from '@/src/store/features/organizations/employee-api'
import { useGetPortfolioOptionsQuery } from '@/src/store/features/ppm/portfolios-api'
import { useCreateStrategicInitiativeMutation } from '@/src/store/features/ppm/strategic-initiatives-api'
import { toFormErrors } from '@/src/utils'
import { DatePicker, Form, Modal, Select } from 'antd'
import TextArea from 'antd/es/input/TextArea'
import { useCallback, useEffect } from 'react'

const { Item } = Form

export interface CreateStrategicInitiativeFormProps {
  onFormComplete: () => void
  onFormCancel: () => void
}

interface CreateStrategicInitiativeFormValues {
  portfolioId: string
  name: string
  description: string
  start: Date
  end: Date
  sponsorIds: string[]
  ownerIds: string[]
}

const mapToRequestValues = (
  values: CreateStrategicInitiativeFormValues,
): CreateStrategicInitiativeRequest => {
  return {
    name: values.name,
    description: values.description,
    start: (values.start as any).format('YYYY-MM-DD'),
    end: (values.end as any).format('YYYY-MM-DD'),
    portfolioId: values.portfolioId,
    sponsorIds: values.sponsorIds,
    ownerIds: values.ownerIds,
  }
}

const CreateStrategicInitiativeForm = ({
  onFormComplete,
  onFormCancel,
}: CreateStrategicInitiativeFormProps) => {
  const messageApi = useMessage()

  const [createStrategicInitiative] =
    useCreateStrategicInitiativeMutation()

  const {
    data: portfolioData,
    error: portfolioOptionsError,
  } = useGetPortfolioOptionsQuery()

  const {
    data: employeeData,
    error: employeeOptionsError,
  } = useGetEmployeeOptionsQuery(false)

  const { form, isOpen, isValid, isSaving, handleOk, handleCancel } =
    useModalForm<CreateStrategicInitiativeFormValues>({
      onSubmit: useCallback(
        async (values: CreateStrategicInitiativeFormValues, form) => {
          try {
            const request = mapToRequestValues(values)
            const response = await createStrategicInitiative(request)
            if (response.error) throw response.error

            messageApi.success(
              'Strategic initiative created successfully. Strategic initiative key: ' +
                response.data.key,
            )
            return true
          } catch (error) {
            if (error.status === 422 && error.errors) {
              const formErrors = toFormErrors(error.errors)
              form.setFields(formErrors)
              messageApi.error('Correct the validation error(s) to continue.')
            } else {
              messageApi.error(
                error.detail ??
                  'An error occurred while creating the strategic initiative. Please try again.',
              )
            }
            return false
          }
        },
        [createStrategicInitiative, messageApi],
      ),
      onComplete: onFormComplete,
      onCancel: onFormCancel,
      errorMessage:
        'An error occurred while creating the strategic initiative. Please try again.',
      permission: 'Permissions.StrategicInitiatives.Create',
    })

  useEffect(() => {
    if (portfolioOptionsError || employeeOptionsError) {
      console.error(portfolioOptionsError || employeeOptionsError)
      messageApi.error(
        portfolioOptionsError.detail ||
          employeeOptionsError.detail ||
          'An error occurred while loading form data.',
      )
    }
  }, [portfolioOptionsError, employeeOptionsError, messageApi])

  return (
    <Modal
      title="Create Strategic Initiative"
      open={isOpen}
      width={'60vw'}
      onOk={handleOk}
      okButtonProps={{ disabled: !isValid }}
      okText="Create"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false} // disable esc key to close modal
      destroyOnHidden
    >
      <Form
        form={form}
        size="small"
        layout="vertical"
        name="create-strategic-initiative-form"
      >
        <Item
          name="portfolioId"
          label="Portfolio"
          rules={[{ required: true, message: 'Portfolio is required' }]}
        >
          <Select
            allowClear
            options={portfolioData ?? []}
            placeholder="Select Portfolio"
          />
        </Item>
        <Item
          label="Name"
          name="name"
          rules={[
            { required: true, message: 'Name is required' },
            { max: 128 },
          ]}
        >
          <TextArea
            autoSize={{ minRows: 1, maxRows: 2 }}
            showCount
            maxLength={128}
          />
        </Item>
        <Item
          name="description"
          label="Description"
          rules={[
            { required: true, message: 'Name is required' },
            { max: 2048 },
          ]}
        >
          <MarkdownEditor maxLength={2048} />
        </Item>
        <Item name="start" label="Start" rules={[{ required: true }]}>
          <DatePicker />
        </Item>
        <Item
          name="end"
          label="End"
          dependencies={['start']}
          rules={[
            { required: true },
            ({ getFieldValue }) => ({
              validator(_, value) {
                const start = getFieldValue('start')
                if (!value || !start || start < value) {
                  return Promise.resolve()
                }
                return Promise.reject(
                  new Error('End date must be after start date'),
                )
              },
            }),
          ]}
        >
          <DatePicker />
        </Item>
        <Item name="sponsorIds" label="Sponsors">
          <EmployeeSelect
            employees={employeeData ?? []}
            allowMultiple={true}
            placeholder="Select Sponsors"
          />
        </Item>
        <Item name="ownerIds" label="Owners">
          <EmployeeSelect
            employees={employeeData ?? []}
            allowMultiple={true}
            placeholder="Select Owners"
          />
        </Item>
      </Form>
    </Modal>
  )
}

export default CreateStrategicInitiativeForm
