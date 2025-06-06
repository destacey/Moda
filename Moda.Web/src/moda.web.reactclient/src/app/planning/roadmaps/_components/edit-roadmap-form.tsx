'use client'

import useAuth from '@/src/components/contexts/auth'
import {
  RoadmapDetailsDto,
  UpdateRoadmapRequest,
} from '@/src/services/moda-api'
import {
  useUpdateRoadmapMutation,
  useGetVisibilityOptionsQuery,
  useGetRoadmapQuery,
} from '@/src/store/features/planning/roadmaps-api'
import { toFormErrors } from '@/src/utils'
import { DatePicker, Form, Input, Modal, Radio } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import dayjs from 'dayjs'
import { useGetEmployeeOptionsQuery } from '@/src/store/features/organizations/employee-api'
import { useGetInternalEmployeeIdQuery } from '@/src/store/features/user-management/profile-api'
import { MarkdownEditor } from '@/src/components/common/markdown'
import { EmployeeSelect } from '@/src/components/common/organizations'
import { useMessage } from '@/src/components/contexts/messaging'

const { Item } = Form
const { TextArea } = Input
const { Group: RadioGroup } = Radio

export interface EditRoadmapFormProps {
  roadmapKey: number
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
}

interface EditRoadmapFormValues {
  name: string
  description?: string
  start: Date
  end: Date
  roadmapManagerIds: string[]
  visibilityId: number
}

const mapToRequestValues = (
  values: EditRoadmapFormValues,
  objectiveId: string,
): UpdateRoadmapRequest => {
  return {
    id: objectiveId,
    name: values.name,
    description: values.description,
    start: (values.start as any)?.format('YYYY-MM-DD'),
    end: (values.end as any)?.format('YYYY-MM-DD'),
    roadmapManagerIds: values.roadmapManagerIds,
    visibilityId: values.visibilityId,
  } as UpdateRoadmapRequest
}

const EditRoadmapForm = (props: EditRoadmapFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<EditRoadmapFormValues>()
  const formValues = Form.useWatch([], form)

  const messageApi = useMessage()

  const {
    data: roadmapData,
    isLoading,
    error,
  } = useGetRoadmapQuery(props.roadmapKey.toString())

  const {
    data: visibilityData,
    isLoading: visibilityLoading,
    error: visibilityError,
  } = useGetVisibilityOptionsQuery()
  const [updateRoadmap, { error: mutationError }] = useUpdateRoadmapMutation()

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
  const canUpdateRoadmap = hasPermissionClaim('Permissions.Roadmaps.Update')

  const mapToFormValues = useCallback(
    (roadmap: RoadmapDetailsDto) => {
      if (!roadmap) {
        throw new Error('Roadmap not found.')
      }
      form.setFieldsValue({
        name: roadmap.name,
        description: roadmap.description || '', // Ensure an empty string if description is undefined
        start: dayjs(roadmap.start),
        end: dayjs(roadmap.end),
        visibilityId: roadmap.visibility.id,
        roadmapManagerIds: roadmap.roadmapManagers.map((rm) => rm.id),
      })
    },
    [form],
  )

  const update = async (
    values: EditRoadmapFormValues,
    roadmap: RoadmapDetailsDto,
  ) => {
    try {
      const request = mapToRequestValues(values, roadmap.id)
      const response = await updateRoadmap({
        request,
        cacheKey: roadmap.key,
      })
      if (response.error) {
        throw response.error
      }
      messageApi.success(`Roadmap updated successfully.`)
      return true
    } catch (error) {
      console.error('update error', error)
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.detail ??
            'An error occurred while updating the roadmap. Please try again.',
        )
      }
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (await update(values, roadmapData)) {
        setIsOpen(false)
        form.resetFields()
        props.onFormComplete()
      }
    } catch (error) {
      console.error('handleOk error', error)
      messageApi.error(
        'An error occurred while updating the roadmap. Please try again.',
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
    if (!roadmapData || !visibilityData) return
    if (canUpdateRoadmap) {
      setIsOpen(props.showForm)
      if (props.showForm) {
        mapToFormValues(roadmapData)
      }
    } else {
      props.onFormCancel()
      messageApi.error('You do not have permission to update roadmaps.')
    }
  }, [
    canUpdateRoadmap,
    mapToFormValues,
    messageApi,
    props,
    roadmapData,
    visibilityData,
  ])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  useEffect(() => {
    if (error) {
      messageApi.error(
        error.detail ??
          'An error occurred while loading the roadmap. Please try again.',
      )
    }
    if (visibilityError) {
      messageApi.error(
        visibilityError.supportMessage ??
          'An error occurred while loading visibility options. Please try again.',
      )
    }
    if (employeeOptionsError) {
      messageApi.error(
        employeeOptionsError.supportMessage ??
          'An error occurred while loading employee options. Please try again.',
      )
    }
    if (currentUserInternalEmployeeIdError) {
      messageApi.error(
        currentUserInternalEmployeeIdError.supportMessage ??
          'An error occurred while loading current user profile data. Please try again.',
      )
    }
  }, [
    currentUserInternalEmployeeIdError,
    employeeOptionsError,
    error,
    messageApi,
    props,
    visibilityError,
  ])

  return (
    <>
      <Modal
        title="Edit Roadmap"
        open={isOpen}
        width={'60vw'}
        onOk={handleOk}
        okButtonProps={{ disabled: !isValid }}
        okText="Save"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnHidden={true}
      >
        <Form
          form={form}
          size="small"
          layout="vertical"
          name="update-roadmap-form"
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
                        'You must also be a roadmap manager to update this roadmap',
                      ),
                    )
                  }
                  return Promise.resolve()
                },
              },
            ]}
          >
            <EmployeeSelect
              employees={employeeData ?? []}
              allowMultiple={true}
              placeholder="Select one or more roadmap managers"
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

export default EditRoadmapForm
