import useAuth from '@/src/app/components/contexts/auth'
import { CreateRoadmapRequest } from '@/src/services/moda-api'
import {
  useCreateRoadmapMutation,
  useGetVisibilityOptionsQuery,
} from '@/src/store/features/planning/roadmaps-api'
import { toFormErrors } from '@/src/utils'
import { DatePicker, Form, Input, Modal, Radio } from 'antd'
import { MessageInstance } from 'antd/es/message/interface'
import { useCallback, useEffect, useState } from 'react'

const { Item } = Form
const { TextArea } = Input
const { Group: RadioGroup } = Radio

export interface CreateRoadmapFormProps {
  showForm: boolean
  parentRoadmapId?: string | null
  onFormComplete: () => void
  onFormCancel: () => void
  messageApi: MessageInstance
}

interface CreateRoadmapFormValues {
  name: string
  description?: string
  start: Date
  end: Date
  visibilityId: number
}

const mapToRequestValues = (
  values: CreateRoadmapFormValues,
  parentRoadmapId?: string | null,
): CreateRoadmapRequest => {
  return {
    name: values.name,
    description: values.description,
    start: (values.start as any)?.format('YYYY-MM-DD'),
    end: (values.end as any)?.format('YYYY-MM-DD'),
    visibilityId: values.visibilityId,
    parentId: parentRoadmapId,
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

  const { hasPermissionClaim } = useAuth()
  const canCreateRoadmap = hasPermissionClaim('Permissions.Roadmaps.Create')

  const create = async (values: CreateRoadmapFormValues, parentRoadmapId) => {
    try {
      const request = mapToRequestValues(values, parentRoadmapId)
      await createRoadmap(request)
        .unwrap()
        .then((response) => {
          props.messageApi.success(
            `Roadmap created successfully. Roadmap Key ${response.roadmapIds.key}`,
          )
          if (response.linkToParentError) {
            const message = `Roadmap created successfully, but there was an error linking it to the parent roadmap. Error: ${response.linkToParentError}`
            props.messageApi.error(message)
          }
        })
        .catch((error) => {
          throw error
        })
      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        props.messageApi.error('Correct the validation error(s) to continue.')
      } else {
        props.messageApi.error(
          error.supportMessage ??
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
      if (await create(values, props.parentRoadmapId)) {
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
    } else {
      props.onFormCancel()
      props.messageApi.error('You do not have permission to create roadmaps.')
    }
  }, [canCreateRoadmap, props])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  useEffect(() => {
    if (error) {
      props.messageApi.error(
        error.supportMessage ??
          'An error occurred while loading visibility options. Please try again.',
      )
    }
  }, [error, props])

  return (
    <>
      <Modal
        title="Create Roadmap"
        open={isOpen}
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
          <Item name="description" label="Description" extra="Markdown enabled">
            <TextArea
              autoSize={{ minRows: 6, maxRows: 10 }}
              showCount
              maxLength={2048}
            />
          </Item>
          <Item name="start" label="Start" rules={[{ required: true }]}>
            <DatePicker />
          </Item>
          <Item name="end" label="End" rules={[{ required: true }]}>
            <DatePicker />
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
