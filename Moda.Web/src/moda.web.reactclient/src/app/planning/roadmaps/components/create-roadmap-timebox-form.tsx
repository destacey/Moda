import { ModaColorPicker } from '@/src/app/components/common'
import useAuth from '@/src/app/components/contexts/auth'
import { CreateRoadmapTimeboxRequest } from '@/src/services/moda-api'
import {
  useCreateRoadmapItemMutation,
  useGetRoadmapActivitiesQuery,
} from '@/src/store/features/planning/roadmaps-api'
import { toFormErrors } from '@/src/utils'
import { DatePicker, Form, Input, Modal, Radio, Select, TreeSelect } from 'antd'
import { MessageInstance } from 'antd/es/message/interface'
import { useCallback, useEffect, useState } from 'react'

const { Item } = Form
const { TextArea } = Input

export interface CreateRoadmapTimeboxFormProps {
  showForm: boolean
  roadmapId: string
  onFormComplete: () => void
  onFormCancel: () => void
  messageApi: MessageInstance
}

interface CreateRoadmapTimeboxFormValues {
  parentId?: string
  name: string
  description?: string
  start: Date
  end: Date
  color?: string
}

const mapToRequestValues = (
  values: CreateRoadmapTimeboxFormValues,
  roadmapId: string,
): CreateRoadmapTimeboxRequest => {
  return {
    $type: 'timebox',
    roadmapId: roadmapId,
    parentId: values.parentId,
    name: values.name,
    description: values.description,
    start: (values.start as any)?.format('YYYY-MM-DD'),
    end: (values.end as any)?.format('YYYY-MM-DD'),
    color: values.color,
  } as CreateRoadmapTimeboxRequest
}

const CreateRoadmapTimeboxForm = (props: CreateRoadmapTimeboxFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreateRoadmapTimeboxFormValues>()
  const formValues = Form.useWatch([], form)

  const {
    data: activities,
    isLoading: activitiesIsLoading,
    error: activitiesError,
  } = useGetRoadmapActivitiesQuery(props.roadmapId)

  const [createRoadmapTimebox, { error: mutationError }] =
    useCreateRoadmapItemMutation()

  const { hasPermissionClaim } = useAuth()
  const canManageRoadmapItems = hasPermissionClaim(
    'Permissions.Roadmaps.Update',
  )

  const create = async (values: CreateRoadmapTimeboxFormValues, roadmapId) => {
    try {
      const request = mapToRequestValues(values, roadmapId)
      const response = await createRoadmapTimebox(request)
      if (response.error) {
        throw response.error
      }

      props.messageApi.success('Roadmap Timebox created successfully.')

      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        props.messageApi.error('Correct the validation error(s) to continue.')
      } else {
        props.messageApi.error(
          error.supportMessage ??
            'An error occurred while creating the roadmap timebox. Please try again.',
        )
      }
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (await create(values, props.roadmapId)) {
        setIsOpen(false)
        form.resetFields()
        props.onFormComplete()
      }
    } catch (error) {
      console.error('handleOk error', error)
      props.messageApi.error(
        'An error occurred while creating the roadmap timebox. Please try again.',
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
    if (canManageRoadmapItems) {
      setIsOpen(props.showForm)
    } else {
      props.onFormCancel()
      props.messageApi.error(
        'You do not have permission to create roadmap items.',
      )
    }
  }, [canManageRoadmapItems, props])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  useEffect(() => {
    if (activitiesError) {
      props.messageApi.error(
        activitiesError.supportMessage ??
          'An error occurred while loading roadmap activities. Please try again.',
      )
    }
  }, [activitiesError, props.messageApi])

  const onColorChange = (color: string) => {
    form.setFieldsValue({ color })
  }

  return (
    <>
      <Modal
        title="Create Roadmap Timebox"
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
          name="create-roadmap-timebox-form"
        >
          <Item
            name="parentId"
            label="Parent Activity"
            hidden={activities?.length < 1}
          >
            <TreeSelect
              //showSearch // TODO: not working
              treeLine={true}
              placeholder="Please select parent activity"
              allowClear
              treeDefaultExpandAll
              treeData={activities}
              fieldNames={{ label: 'name', value: 'id', children: 'children' }}
            />
          </Item>
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
          {/* Hide for now */}
          {/* <Item name="color" label="Color">
            <ModaColorPicker
              color={form.getFieldValue('color')}
              onChange={onColorChange}
            />
          </Item> */}
        </Form>
      </Modal>
    </>
  )
}

export default CreateRoadmapTimeboxForm
