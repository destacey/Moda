'use client'

import { MarkdownEditor } from '@/src/components/common/markdown'
import { EmployeeSelect } from '@/src/components/common/organizations'
import { useMessage } from '@/src/components/contexts/messaging'
import { useModalForm } from '@/src/hooks'
import { UpdateStrategicInitiativeRequest } from '@/src/services/moda-api'
import { useGetEmployeeOptionsQuery } from '@/src/store/features/organizations/employee-api'
import {
  useGetStrategicInitiativeQuery,
  useUpdateStrategicInitiativeMutation,
} from '@/src/store/features/ppm/strategic-initiatives-api'
import { toFormErrors } from '@/src/utils'
import { DatePicker, Form, Modal } from 'antd'
import TextArea from 'antd/es/input/TextArea'
import dayjs from 'dayjs'
import { useCallback, useEffect } from 'react'

const { Item } = Form

export interface EditStrategicInitiativeFormProps {
  strategicInitiativeKey: number
  onFormComplete: () => void
  onFormCancel: () => void
}

interface EditStrategicInitiativeFormValues {
  name: string
  description: string
  start: Date
  end: Date
  sponsorIds: string[]
  ownerIds: string[]
}

const mapToRequestValues = (
  values: EditStrategicInitiativeFormValues,
  strategicInitiativeId: string,
): UpdateStrategicInitiativeRequest => {
  return {
    id: strategicInitiativeId,
    name: values.name,
    description: values.description,
    start: (values.start as any).format('YYYY-MM-DD'),
    end: (values.end as any).format('YYYY-MM-DD'),
    sponsorIds: values.sponsorIds,
    ownerIds: values.ownerIds,
  }
}

const EditStrategicInitiativeForm = ({
  strategicInitiativeKey,
  onFormComplete,
  onFormCancel,
}: EditStrategicInitiativeFormProps) => {
  const messageApi = useMessage()

  const [updateStrategicInitiative] = useUpdateStrategicInitiativeMutation()

  const {
    data: strategicInitiativeData,
    isLoading,
    error,
  } = useGetStrategicInitiativeQuery(strategicInitiativeKey)

  const { data: employeeData, error: employeeOptionsError } =
    useGetEmployeeOptionsQuery(false)

  const { form, isOpen, isValid, isSaving, handleOk, handleCancel } =
    useModalForm<EditStrategicInitiativeFormValues>({
      onSubmit: useCallback(
        async (values: EditStrategicInitiativeFormValues, form) => {
          try {
            const request = mapToRequestValues(
              values,
              strategicInitiativeData.id,
            )
            const response = await updateStrategicInitiative({
              request,
              cacheKey: strategicInitiativeData.key,
            })
            if (response.error) throw response.error

            messageApi.success('Strategic initiative updated successfully.')
            return true
          } catch (error) {
            if (error.status === 422 && error.errors) {
              const formErrors = toFormErrors(error.errors)
              form.setFields(formErrors)
              messageApi.error('Correct the validation error(s) to continue.')
            } else {
              messageApi.error(
                error.detail ??
                  'An error occurred while updating the strategic initiative. Please try again.',
              )
            }
            return false
          }
        },
        [updateStrategicInitiative, strategicInitiativeData, messageApi],
      ),
      onComplete: onFormComplete,
      onCancel: onFormCancel,
      errorMessage:
        'An error occurred while updating the strategic initiative. Please try again.',
      permission: 'Permissions.StrategicInitiatives.Update',
    })

  // Initialize form values when data is loaded
  useEffect(() => {
    if (!strategicInitiativeData) return

    form.setFieldsValue({
      name: strategicInitiativeData.name,
      description: strategicInitiativeData.description,
      start: dayjs(strategicInitiativeData.start),
      end: dayjs(strategicInitiativeData.end),
      sponsorIds: strategicInitiativeData.strategicInitiativeSponsors.map(
        (s) => s.id,
      ),
      ownerIds: strategicInitiativeData.strategicInitiativeOwners.map(
        (o) => o.id,
      ),
    })
  }, [strategicInitiativeData, form])

  useEffect(() => {
    if (error || employeeOptionsError) {
      console.error(error || employeeOptionsError)
      messageApi.error(
        error?.detail ||
          employeeOptionsError?.detail ||
          'An error occurred while loading form data.',
      )
    }
  }, [employeeOptionsError, error, messageApi])

  return (
    <Modal
      title="Edit Strategic Initiative"
      open={isOpen}
      width={'60vw'}
      onOk={handleOk}
      okButtonProps={{ disabled: !isValid }}
      okText="Save"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false} // disable esc key to close modal
      destroyOnHidden
    >
      <Form
        form={form}
        size="small"
        layout="vertical"
        name="edit-strategic-initiative-form"
      >
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
            { required: true, message: 'Description is required' },
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

export default EditStrategicInitiativeForm
