import { ModaColorPicker } from '@/src/app/components/common'
import useAuth from '@/src/app/components/contexts/auth'
import {
  RoadmapTimeboxDetailsDto,
  RoadmapTimeboxListDto,
  UpdateRoadmapTimeboxRequest,
} from '@/src/services/moda-api'
import {
  useGetRoadmapActivitiesQuery,
  useGetRoadmapItemQuery,
  useUpdateRoadmapItemMutation,
} from '@/src/store/features/planning/roadmaps-api'
import { toFormErrors } from '@/src/utils'
import { DatePicker, Form, Input, Modal, TreeSelect } from 'antd'
import { MessageInstance } from 'antd/es/message/interface'
import { useCallback, useEffect, useState } from 'react'
import dayjs from 'dayjs'
import { MarkdownEditor } from '@/src/app/components/common/markdown'

const { Item } = Form
const { TextArea } = Input

export interface EditRoadmapTimeboxFormProps {
  showForm: boolean
  timeboxId: string
  roadmapId: string
  onFormComplete: () => void
  onFormCancel: () => void
  messageApi: MessageInstance
}

interface EditRoadmapTimeboxFormValues {
  parentActivityId?: string
  name: string
  description?: string
  start: Date
  end: Date
  color?: string
}

const mapToRequestValues = (
  values: EditRoadmapTimeboxFormValues,
  timeboxId: string,
  roadmapId: string,
): UpdateRoadmapTimeboxRequest => {
  return {
    $type: 'timebox',
    roadmapId: roadmapId,
    itemId: timeboxId,
    parentId: values.parentActivityId,
    name: values.name,
    description: values.description,
    start: (values.start as any)?.format('YYYY-MM-DD'),
    end: (values.end as any)?.format('YYYY-MM-DD'),
    color: values.color,
  } as UpdateRoadmapTimeboxRequest
}

const EditRoadmapTimeboxForm = (props: EditRoadmapTimeboxFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<EditRoadmapTimeboxFormValues>()
  const formValues = Form.useWatch([], form)
  const [activitiesTree, setActivitiesTree] = useState<RoadmapTimeboxListDto[]>(
    [],
  )

  const {
    data: timeboxData,
    isLoading: timeboxDataIsLoading,
    error: timeboxDataError,
  } = useGetRoadmapItemQuery({
    roadmapId: props.roadmapId,
    itemId: props.timeboxId,
  })

  const {
    data: activities,
    isLoading: activitiesIsLoading,
    error: activitiesError,
  } = useGetRoadmapActivitiesQuery(props.roadmapId)

  const [updateRoadmapTimebox, { error: mutationError }] =
    useUpdateRoadmapItemMutation()

  const { hasPermissionClaim } = useAuth()
  const canManageRoadmapItems = hasPermissionClaim(
    'Permissions.Roadmaps.Update',
  )

  const mapToFormValues = useCallback(
    (activity: RoadmapTimeboxDetailsDto) => {
      if (!activity) return

      form.setFieldsValue({
        parentActivityId: activity.parent?.id,
        name: activity.name,
        description: activity.description || '',
        start: dayjs(activity.start),
        end: dayjs(activity.end),
        color: activity.color,
      })
    },
    [form],
  )

  const update = async (
    values: EditRoadmapTimeboxFormValues,
    itemId: string,
    roadmapId: string,
  ) => {
    try {
      const request = mapToRequestValues(values, itemId, roadmapId)
      const response = await updateRoadmapTimebox(request)
      if (response.error) {
        throw response.error
      }

      props.messageApi.success('Roadmap Timebox updated successfully.')

      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        props.messageApi.error('Correct the validation error(s) to continue.')
      } else {
        props.messageApi.error(
          error.detail ??
            'An error occurred while updating the roadmap timebox. Please try again.',
        )
      }
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (await update(values, props.timeboxId, props.roadmapId)) {
        setIsOpen(false)
        form.resetFields()
        props.onFormComplete()
      }
    } catch (error) {
      console.error('handleOk error', error)
      props.messageApi.error(
        'An error occurred while updating the roadmap timebox. Please try again.',
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
    if (!timeboxData || !activities || activitiesIsLoading) return

    setActivitiesTree(activities)

    if (canManageRoadmapItems) {
      setIsOpen(props.showForm)
      if (props.showForm) {
        mapToFormValues(timeboxData)
      }
    } else {
      props.onFormCancel()
      props.messageApi.error(
        'You do not have permission to update roadmap items.',
      )
    }
  }, [
    activities,
    activitiesIsLoading,
    timeboxData,
    canManageRoadmapItems,
    mapToFormValues,
    props,
  ])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  useEffect(() => {
    if (timeboxDataError) {
      props.messageApi.error(
        timeboxDataError.supportMessage ??
          'An error occurred while loading roadmap timebox. Please try again.',
      )
    }
    if (activitiesError) {
      props.messageApi.error(
        activitiesError.supportMessage ??
          'An error occurred while loading roadmap activities. Please try again.',
      )
    }
  }, [activitiesError, timeboxDataError, props.messageApi])

  const onColorChange = (color: string) => {
    form.setFieldsValue({ color })
  }

  return (
    <>
      <Modal
        title="Edit Roadmap Timebox"
        open={isOpen}
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
          name="update-roadmap-timebox-form"
        >
          <Item name="parentActivityId" label="Parent Activity">
            <TreeSelect
              //showSearch // TODO: not working
              loading={activitiesIsLoading}
              treeLine={true}
              placeholder="Select parent activity"
              allowClear
              treeDefaultExpandAll
              treeData={activitiesTree}
              fieldNames={{ label: 'name', value: 'id', children: 'children' }}
              value={form.getFieldValue('parentActivityId')}
            />
          </Item>
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

export default EditRoadmapTimeboxForm
