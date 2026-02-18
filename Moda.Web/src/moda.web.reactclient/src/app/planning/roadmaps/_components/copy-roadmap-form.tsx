'use client'

import { EmployeeSelect } from '@/src/components/common/organizations'
import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { CopyRoadmapRequest } from '@/src/services/moda-api'
import { useGetEmployeeOptionsQuery } from '@/src/store/features/organizations/employee-api'
import {
  useCopyRoadmapMutation,
  useGetVisibilityOptionsQuery,
} from '@/src/store/features/planning/roadmaps-api'
import { useGetInternalEmployeeIdQuery } from '@/src/store/features/user-management/profile-api'
import { toFormErrors } from '@/src/utils'
import { Form, Input, Modal, Radio } from 'antd'
import { useRouter } from 'next/navigation'
import { useCallback, useEffect, useState } from 'react'

const { Item } = Form
const { TextArea } = Input
const { Group: RadioGroup } = Radio

export interface CopyRoadmapFormProps {
  sourceRoadmapId: string
  sourceRoadmapName: string
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
}

interface CopyRoadmapFormValues {
  name: string
  roadmapManagerIds: string[]
  visibilityId: number
}

const mapToRequestValues = (
  values: CopyRoadmapFormValues,
  sourceRoadmapId: string,
): CopyRoadmapRequest => {
  return {
    sourceRoadmapId: sourceRoadmapId,
    name: values.name,
    roadmapManagerIds: values.roadmapManagerIds,
    visibilityId: values.visibilityId,
  } as CopyRoadmapRequest
}

const CopyRoadmapForm = (props: CopyRoadmapFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CopyRoadmapFormValues>()
  const formValues = Form.useWatch([], form)
  const router = useRouter()

  const messageApi = useMessage()

  const {
    data: visibilityData,
    isLoading,
    error,
  } = useGetVisibilityOptionsQuery()
  const [copyRoadmap, { error: mutationError }] = useCopyRoadmapMutation()

  const {
    data: employeeData,
    isLoading: employeeOptionsIsLoading,
    error: employeeOptionsError,
  } = useGetEmployeeOptionsQuery(false)

  const {
    data: currentUserInternalEmployeeId,
    error: currentUserInternalEmployeeIdError,
  } = useGetInternalEmployeeIdQuery()

  const { hasPermissionClaim } = useAuth()
  const canCreateRoadmap = hasPermissionClaim('Permissions.Roadmaps.Create')

  const mapToFormValues = useCallback(
    (roadmapManagerIds: string[]) => {
      form.setFieldsValue({
        name: `Copy of ${props.sourceRoadmapName}`,
        roadmapManagerIds: roadmapManagerIds,
      })
    },
    [form, props.sourceRoadmapName],
  )

  const copy = async (values: CopyRoadmapFormValues) => {
    try {
      const request = mapToRequestValues(values, props.sourceRoadmapId)
      const response = await copyRoadmap(request)
      if (response.error) {
        throw response.error
      }

      messageApi.success(
        `Roadmap copied successfully. Roadmap key ${response.data.key}`,
      )

      // Navigate to the new roadmap
      router.push(`/planning/roadmaps/${response.data.key}`)

      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.detail ??
            'An error occurred while copying the roadmap. Please try again.',
        )
      }
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (await copy(values)) {
        setIsOpen(false)
        form.resetFields()
        props.onFormComplete()
      }
    } catch (error) {
      console.error('handleOk error', error)
      messageApi.error(
        'An error occurred while copying the roadmap. Please try again.',
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
      messageApi.error('You do not have permission to copy roadmaps.')
    }
  }, [
    canCreateRoadmap,
    currentUserInternalEmployeeId,
    mapToFormValues,
    messageApi,
    props,
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
  ])

  return (
    <>
      <Modal
        title="Copy Roadmap"
        open={isOpen}
        width={'60vw'}
        onOk={handleOk}
        okButtonProps={{ disabled: !isValid }}
        okText="Copy"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        keyboard={false} // disable esc key to close modal
        destroyOnHidden={true}
      >
        <Form
          form={form}
          size="small"
          layout="vertical"
          name="copy-roadmap-form"
        >
          <Item label="Name" name="name" rules={[{ required: true }]}>
            <TextArea
              autoSize={{ minRows: 1, maxRows: 2 }}
              showCount
              maxLength={128}
            />
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
                        'You must also be a roadmap manager to copy this roadmap',
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

export default CopyRoadmapForm
