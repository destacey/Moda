'use client'

import { ModaColorPicker } from '@/src/components/common'
import useAuth from '@/src/components/contexts/auth'
import {
  RoadmapActivityDetailsDto,
  RoadmapActivityListDto,
  UpdateRoadmapActivityRequest,
} from '@/src/services/moda-api'
import {
  useGetRoadmapActivitiesQuery,
  useGetRoadmapItemQuery,
  useUpdateRoadmapItemMutation,
} from '@/src/store/features/planning/roadmaps-api'
import { toFormErrors } from '@/src/utils'
import { DatePicker, Form, Input, Modal, TreeSelect } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import dayjs from 'dayjs'
import { MarkdownEditor } from '@/src/components/common/markdown'
import { useMessage } from '@/src/components/contexts/messaging'

const { Item } = Form
const { TextArea } = Input

export interface EditRoadmapActivityFormProps {
  showForm: boolean
  activityId: string
  roadmapId: string
  onFormComplete: () => void
  onFormCancel: () => void
}

interface EditRoadmapActivityFormValues {
  parentActivityId?: string
  name: string
  description?: string
  start: Date
  end: Date
  color?: string
}

const mapToRequestValues = (
  values: EditRoadmapActivityFormValues,
  activityId: string,
  roadmapId: string,
): UpdateRoadmapActivityRequest => {
  return {
    $type: 'activity',
    roadmapId: roadmapId,
    itemId: activityId,
    parentId: values.parentActivityId,
    name: values.name,
    description: values.description,
    start: (values.start as any)?.format('YYYY-MM-DD'),
    end: (values.end as any)?.format('YYYY-MM-DD'),
    color: values.color,
  } satisfies UpdateRoadmapActivityRequest
}

const filterActivities = (activities: RoadmapActivityListDto[], activityId) => {
  return activities
    .filter((a) => a.id !== activityId)
    .map((a) => ({
      ...a,
      children: a.children ? filterActivities(a.children, activityId) : [],
    }))
}

const EditRoadmapActivityForm = (props: EditRoadmapActivityFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<EditRoadmapActivityFormValues>()
  const formValues = Form.useWatch([], form)
  const [activitiesTree, setActivitiesTree] = useState<
    RoadmapActivityListDto[]
  >([])

  const messageApi = useMessage()

  const {
    data: activityData,
    isLoading: activityDataIsLoading,
    error: activityDataError,
  } = useGetRoadmapItemQuery({
    roadmapId: props.roadmapId,
    itemId: props.activityId,
  })

  const {
    data: activities,
    isLoading: activitiesIsLoading,
    error: activitiesError,
  } = useGetRoadmapActivitiesQuery(props.roadmapId)

  const [updateRoadmapActivity, { error: mutationError }] =
    useUpdateRoadmapItemMutation()

  const { hasPermissionClaim } = useAuth()
  const canManageRoadmapItems = hasPermissionClaim(
    'Permissions.Roadmaps.Update',
  )

  const mapToFormValues = useCallback(
    (activity: RoadmapActivityDetailsDto) => {
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
    values: EditRoadmapActivityFormValues,
    activityId: string,
    roadmapId: string,
  ) => {
    try {
      const request = mapToRequestValues(values, activityId, roadmapId)
      const response = await updateRoadmapActivity(request)
      if (response.error) {
        throw response.error
      }

      messageApi.success('Roadmap Activity updated successfully.')

      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.detail ??
            'An error occurred while updating the roadmap activity. Please try again.',
        )
      }
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (await update(values, props.activityId, props.roadmapId)) {
        setIsOpen(false)
        form.resetFields()
        props.onFormComplete()
      }
    } catch (error) {
      console.error('handleOk error', error)
      messageApi.error(
        'An error occurred while updating the roadmap activity. Please try again.',
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
    if (!activityData || !activities || activitiesIsLoading) return

    const filteredActivities = filterActivities(activities, props.activityId)
    setActivitiesTree(filteredActivities)

    if (canManageRoadmapItems) {
      setIsOpen(props.showForm)
      if (props.showForm) {
        mapToFormValues(activityData)
      }
    } else {
      props.onFormCancel()
      messageApi.error('You do not have permission to update roadmap items.')
    }
  }, [
    activities,
    activitiesIsLoading,
    activityData,
    canManageRoadmapItems,
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
    if (activityDataError) {
      messageApi.error(
        activityDataError.supportMessage ??
          'An error occurred while loading roadmap activity. Please try again.',
      )
    }
    if (activitiesError) {
      messageApi.error(
        activitiesError.supportMessage ??
          'An error occurred while loading roadmap activities. Please try again.',
      )
    }
  }, [activitiesError, activityDataError, messageApi])

  return (
    <>
      <Modal
        title="Edit Roadmap Activity"
        open={isOpen}
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
          name="update-roadmap-activity-form"
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
          <Item name="color" label="Color">
            <ModaColorPicker />
          </Item>
        </Form>
      </Modal>
    </>
  )
}

export default EditRoadmapActivityForm
