import useAuth from '@/src/app/components/contexts/auth'
import { CreateRoadmapRequest } from '@/src/services/moda-api'
import { useCreateRoadmapMutation } from '@/src/store/features/planning/roadmaps-api'
import { toFormErrors } from '@/src/utils'
import { DatePicker, Form, Input, message, Modal, Switch } from 'antd'
import { MessageInstance } from 'antd/es/message/interface'
import { useCallback, useEffect, useState } from 'react'

const { Item } = Form
const { TextArea } = Input

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
  isPublic: boolean
}

const mapToRequestValues = (
  values: CreateRoadmapFormValues,
): CreateRoadmapRequest => {
  return {
    name: values.name,
    description: values.description,
    start: (values.start as any)?.format('YYYY-MM-DD'),
    end: (values.end as any)?.format('YYYY-MM-DD'),
    isPublic: values.isPublic,
  } as CreateRoadmapRequest
}

const CreateRoadmapForm = (props: CreateRoadmapFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreateRoadmapFormValues>()
  const formValues = Form.useWatch([], form)

  const [createRoadmap, { error: mutationError }] = useCreateRoadmapMutation()

  const { hasPermissionClaim } = useAuth()
  const canCreateRoadmap = hasPermissionClaim('Permissions.Roadmaps.Create')

  const create = async (values: CreateRoadmapFormValues) => {
    try {
      const request = mapToRequestValues(values)
      await createRoadmap(request)
        .unwrap()
        .then((response) => {
          props.messageApi.success(
            `Roadmap created successfully. Roadmap Key ${response.key}`,
          )
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
          initialValues={{ isPublic: true }} // used to set default value for switch
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
          <Item label="Start" name="start" rules={[{ required: true }]}>
            <DatePicker />
          </Item>
          <Item label="End" name="end" rules={[{ required: true }]}>
            <DatePicker />
          </Item>
          <Item name="isPublic" label="Access Level" valuePropName="checked">
            <Switch checkedChildren="Public" unCheckedChildren="Private" />
          </Item>
        </Form>
      </Modal>
    </>
  )
}

export default CreateRoadmapForm
