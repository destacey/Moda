import { ModaColorPicker } from '@/src/components/common'
import { MarkdownEditor } from '@/src/components/common/markdown'
import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { CreateRoadmapActivityRequest } from '@/src/services/moda-api'
import {
  useCreateRoadmapItemMutation,
  useGetRoadmapActivitiesQuery,
} from '@/src/store/features/planning/roadmaps-api'
import { toFormErrors } from '@/src/utils'
import { DatePicker, Form, Input, Modal, TreeSelect } from 'antd'
import { useCallback, useEffect, useState } from 'react'

const { Item } = Form
const { TextArea } = Input

export interface CreateRoadmapActivityFormProps {
  showForm: boolean
  roadmapId: string
  onFormComplete: () => void
  onFormCancel: () => void
}

interface CreateRoadmapActivityFormValues {
  parentId?: string
  name: string
  description?: string
  start: Date
  end: Date
  color?: string
}

const mapToRequestValues = (
  values: CreateRoadmapActivityFormValues,
  roadmapId: string,
): CreateRoadmapActivityRequest => {
  return {
    $type: 'activity',
    roadmapId: roadmapId,
    parentId: values.parentId,
    name: values.name,
    description: values.description,
    start: (values.start as any)?.format('YYYY-MM-DD'),
    end: (values.end as any)?.format('YYYY-MM-DD'),
    color: values.color,
  } as CreateRoadmapActivityRequest
}

const CreateRoadmapActivityForm = (props: CreateRoadmapActivityFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreateRoadmapActivityFormValues>()
  const formValues = Form.useWatch([], form)

  const messageApi = useMessage()

  const {
    data: activities,
    isLoading: activitiesIsLoading,
    error: activitiesError,
  } = useGetRoadmapActivitiesQuery(props.roadmapId)

  const [createRoadmapActivity, { error: mutationError }] =
    useCreateRoadmapItemMutation()

  const { hasPermissionClaim } = useAuth()
  const canManageRoadmapItems = hasPermissionClaim(
    'Permissions.Roadmaps.Update',
  )

  const create = async (values: CreateRoadmapActivityFormValues, roadmapId) => {
    try {
      const request = mapToRequestValues(values, roadmapId)
      const response = await createRoadmapActivity(request)
      if (response.error) {
        throw response.error
      }

      messageApi.success('Roadmap Activity created successfully.')

      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.detail ??
            'An error occurred while creating the roadmap activity. Please try again.',
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
      messageApi.error(
        'An error occurred while creating the roadmap activity. Please try again.',
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
      messageApi.error('You do not have permission to create roadmap items.')
    }
  }, [canManageRoadmapItems, messageApi, props])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  useEffect(() => {
    if (activitiesError) {
      messageApi.error(
        activitiesError.supportMessage ??
          'An error occurred while loading roadmap activities. Please try again.',
      )
    }
  }, [activitiesError, messageApi])

  return (
    <>
      <Modal
        title="Create Roadmap Activity"
        open={isOpen}
        onOk={handleOk}
        okButtonProps={{ disabled: !isValid }}
        okText="Create"
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
          name="create-roadmap-activity-form"
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
          <Item
            name="description"
            label="Description"
            initialValue=""
            rules={[{ max: 2048 }]}
          >
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
          <Item name="color" label="Color">
            <ModaColorPicker />
          </Item>
        </Form>
      </Modal>
    </>
  )
}

export default CreateRoadmapActivityForm
