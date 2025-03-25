'use client'

import { MarkdownEditor } from '@/src/components/common/markdown'
import { EmployeeSelect } from '@/src/components/common/organizations'
import { authorizeForm } from '@/src/components/hoc'
import {
  StrategicInitiativeDetailsDto,
  UpdateStrategicInitiativeRequest,
} from '@/src/services/moda-api'
import { useGetEmployeeOptionsQuery } from '@/src/store/features/organizations/employee-api'
import {
  useGetStrategicInitiativeQuery,
  useUpdateStrategicInitiativeMutation,
} from '@/src/store/features/ppm/strategic-initiatives-api'
import { toFormErrors } from '@/src/utils'
import { DatePicker, Form, Modal } from 'antd'
import TextArea from 'antd/es/input/TextArea'
import { MessageInstance } from 'antd/es/message/interface'
import { useCallback, useEffect, useState } from 'react'
import dayjs from 'dayjs'
import useAuth from '@/src/components/contexts/auth'

const { Item } = Form

export interface EditStrategicInitiativeFormProps {
  strategicInitiativeKey: number
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
  messageApi: MessageInstance
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

const EditStrategicInitiativeForm = (
  props: EditStrategicInitiativeFormProps,
) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<EditStrategicInitiativeFormValues>()
  const formValues = Form.useWatch([], form)

  const {
    strategicInitiativeKey,
    showForm,
    onFormComplete,
    onFormCancel,
    messageApi,
  } = props

  const { hasPermissionClaim } = useAuth()
  const canUpdateStrategicInitiative = hasPermissionClaim(
    'Permissions.StrategicInitiatives.Update',
  )

  const [updateStrategicInitiative, { error: mutationError }] =
    useUpdateStrategicInitiativeMutation()

  const {
    data: strategicInitiativeData,
    isLoading,
    error,
  } = useGetStrategicInitiativeQuery(strategicInitiativeKey)

  const {
    data: employeeData,
    isLoading: employeeOptionsIsLoading,
    error: employeeOptionsError,
  } = useGetEmployeeOptionsQuery(false)

  const mapToFormValues = useCallback(
    (strategicInitiative: StrategicInitiativeDetailsDto) => {
      if (!strategicInitiative) {
        throw new Error('Strategic initiative not found')
      }
      form.setFieldsValue({
        name: strategicInitiative.name,
        description: strategicInitiative.description,
        start: dayjs(strategicInitiative.start),
        end: dayjs(strategicInitiative.end),
        sponsorIds: strategicInitiative.strategicInitiativeSponsors.map(
          (s) => s.id,
        ),
        ownerIds: strategicInitiative.strategicInitiativeOwners.map(
          (o) => o.id,
        ),
      })
    },
    [form],
  )

  const update = async (
    values: EditStrategicInitiativeFormValues,
    strategicInitiative: StrategicInitiativeDetailsDto,
  ) => {
    try {
      const request = mapToRequestValues(values, strategicInitiative.id)
      const response = await updateStrategicInitiative({
        request,
        cacheKey: strategicInitiative.key,
      })

      if (response.error) {
        throw response.error
      }

      messageApi.success(`Strategic initiative updated successfully.`)

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
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (await update(values, strategicInitiativeData)) {
        setIsOpen(false)
        form.resetFields()
        onFormComplete()
      }
    } catch (error) {
      console.error('handleOk error', error)
      messageApi.error(
        'An error occurred while creating the strategic initiative. Please try again.',
      )
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = useCallback(() => {
    setIsOpen(false)
    form.resetFields()
    onFormCancel()
  }, [form, onFormCancel])

  useEffect(() => {
    if (!strategicInitiativeData) return
    if (canUpdateStrategicInitiative) {
      setIsOpen(showForm)
      if (showForm) {
        mapToFormValues(strategicInitiativeData)
      }
    }
  }, [
    canUpdateStrategicInitiative,
    mapToFormValues,
    showForm,
    strategicInitiativeData,
  ])

  useEffect(() => {
    if (error || employeeOptionsError) {
      console.error(error || employeeOptionsError)
      messageApi.error(
        error?.detail ||
          employeeOptionsError?.detail ||
          'An error occurred while loading form data.',
      )
      onFormCancel()
    }
  }, [employeeOptionsError, error, messageApi, onFormCancel])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  return (
    <>
      <Modal
        title="Edit Strategic Initiative"
        open={isOpen}
        width={'60vw'}
        onOk={handleOk}
        okButtonProps={{ disabled: !isValid }}
        okText="Save"
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
          name="create-strategic-initiative-form"
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
    </>
  )
}

export default EditStrategicInitiativeForm
