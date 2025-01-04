import { MarkdownEditor } from '@/src/components/common/markdown'
import useAuth from '@/src/components/contexts/auth'
import { CreateRoadmapRequest } from '@/src/services/moda-api'
import { useGetEmployeeOptionsQuery } from '@/src/store/features/organizations/employee-api'
import {
  useCreateRoadmapMutation,
  useGetVisibilityOptionsQuery,
} from '@/src/store/features/planning/roadmaps-api'
import { useGetInternalEmployeeIdQuery } from '@/src/store/features/user-management/profile-api'
import { toFormErrors } from '@/src/utils'
import { DatePicker, Form, Input, Modal, Radio, Select } from 'antd'
import { MessageInstance } from 'antd/es/message/interface'
import { useCallback, useEffect, useState } from 'react'

const { Item } = Form
const { TextArea } = Input
const { Group: RadioGroup } = Radio

export interface CreateRoadmapFormProps {
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
  messageApi: MessageInstance
}

interface CreateRoadmapFormValues {
  name: string
  description?: string
  start: Date
  end: Date
  roadmapManagerIds: string[]
  visibilityId: number
}

const mapToRequestValues = (
  values: CreateRoadmapFormValues,
): CreateRoadmapRequest => {
  return {
    name: values.name,
    description: values.description,
    start: (values.start as any)?.format('YYYY-MM-DD'),
    end: (values.end as any)?.format('YYYY-MM-DD'),
    roadmapManagerIds: values.roadmapManagerIds,
    visibilityId: values.visibilityId,
  } as CreateRoadmapRequest
}

const CreateRoadmapForm = (props: CreateRoadmapFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreateRoadmapFormValues>()
  const formValues = Form.useWatch([], form)

  const {
    data: visibilityData,
    isLoading,
    error,
  } = useGetVisibilityOptionsQuery()
  const [createRoadmap, { error: mutationError }] = useCreateRoadmapMutation()

  const {
    data: employeeData,
    isLoading: employeeOptionsIsLoading,
    error: employeeOptionsError,
  } = useGetEmployeeOptionsQuery(true)

  const {
    data: currentUserInternalEmployeeId,
    error: currentUserInternalEmployeeIdError,
  } = useGetInternalEmployeeIdQuery()

  const { hasPermissionClaim } = useAuth()
  const canCreateRoadmap = hasPermissionClaim('Permissions.Roadmaps.Create')

  const mapToFormValues = useCallback(
    (roadmapManagerIds: string[]) => {
      form.setFieldsValue({
        roadmapManagerIds: roadmapManagerIds,
        description: '',
      })
    },
    [form],
  )

  const create = async (values: CreateRoadmapFormValues) => {
    try {
      const request = mapToRequestValues(values)
      const response = await createRoadmap(request)
      if (response.error) {
        throw response.error
      }

      props.messageApi.success(
        `Roadmap created successfully. Roadmap Key ${response.data.key}`,
      )

      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        props.messageApi.error('Correct the validation error(s) to continue.')
      } else {
        props.messageApi.error(
          error.detail ??
            'An error occurred while creating the roadmap. Please try again.',
        )
      }
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (await create(values)) {
        setIsOpen(false)
        form.resetFields()
        props.onFormComplete()
      }
    } catch (error) {
      console.error('handleOk error', error)
      props.messageApi.error(
        'An error occurred while creating the roadmap. Please try again.',
      )
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = useCallback(() => {
    setIsOpen(false)
    form.resetFields()
    props.onFormCancel()
  }, [form, props])

  useEffect(() => {
    if (canCreateRoadmap) {
      setIsOpen(props.showForm)
      if (props.showForm) {
        mapToFormValues([currentUserInternalEmployeeId])
      }
    } else {
      props.onFormCancel()
      props.messageApi.error('You do not have permission to create roadmaps.')
    }
  }, [canCreateRoadmap, currentUserInternalEmployeeId, mapToFormValues, props])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  useEffect(() => {
    if (error) {
      props.messageApi.error(
        error.detail ??
          'An error occurred while loading visibility options. Please try again.',
      )
    }
    if (employeeOptionsError) {
      props.messageApi.error(
        employeeOptionsError.supportMessage ??
          'An error occurred while loading employee options. Please try again.',
      )
    }
    if (currentUserInternalEmployeeIdError) {
      props.messageApi.error(
        currentUserInternalEmployeeIdError.supportMessage ??
          'An error occurred while loading current user profile data. Please try again.',
      )
    }
  }, [currentUserInternalEmployeeIdError, employeeOptionsError, error, props])

  return (
    <>
      <Modal
        title="Create Roadmap"
        open={isOpen}
        width={'60vw'}
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
          name="create-roadmap-form"
        >
          <Item label="Name" name="name" rules={[{ required: true }]}>
            <TextArea
              autoSize={{ minRows: 1, maxRows: 2 }}
              showCount
              maxLength={128}
            />
          </Item>
          <Item name="description" label="Description" rules={[{ max: 2048 }]}>
            <MarkdownEditor
              value={form.getFieldValue('description')}
              onChange={(value) =>
                form.setFieldValue('description', value || '')
              }
              maxLength={2048}
            />
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
          <Item
            name="roadmapManagerIds"
            label="Roadmap Managers"
            rules={[
              {
                required: true,
                message: 'Select at least one roadmap manager',
              },
              {
                validator: async (_, value) => {
                  if (!value.includes(currentUserInternalEmployeeId)) {
                    return Promise.reject(
                      new Error(
                        'You must also be a roadmap manager to create this roadmap',
                      ),
                    )
                  }
                  return Promise.resolve()
                },
              },
            ]}
          >
            <Select
              mode="multiple"
              allowClear
              placeholder="Select one or more roadmap managers"
              optionFilterProp="children"
              filterOption={(input, option) =>
                (option?.label.toLowerCase() ?? '').includes(
                  input.toLowerCase(),
                )
              }
              options={employeeData}
            />
          </Item>
          <Item
            name="visibilityId"
            label="Visibility"
            rules={[{ required: true }]}
          >
            <RadioGroup
              options={visibilityData}
              optionType="button"
              buttonStyle="solid"
            />
          </Item>
        </Form>
      </Modal>
    </>
  )
}

export default CreateRoadmapForm
