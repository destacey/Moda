import useAuth from '@/src/app/components/contexts/auth'
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
import { MessageInstance } from 'antd/es/message/interface'
import { useCallback, useEffect, useState } from 'react'
import dayjs from 'dayjs'

const { Item } = Form
const { TextArea } = Input
const { Group: RadioGroup } = Radio

export interface EditRoadmapFormProps {
  roadmapId: string
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
  messageApi: MessageInstance
}

interface EditRoadmapFormValues {
  name: string
  description?: string
  start: Date
  end: Date
  visibilityId: number
}

const mapToRequestValues = (
  values: EditRoadmapFormValues,
  objectiveId: string,
): UpdateRoadmapRequest => {
  console.log('values', values)
  return {
    id: objectiveId,
    name: values.name,
    description: values.description,
    start: (values.start as any)?.format('YYYY-MM-DD'),
    end: (values.end as any)?.format('YYYY-MM-DD'),
    visibilityId: values.visibilityId,
  } as UpdateRoadmapRequest
}

const EditRoadmapForm = (props: EditRoadmapFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<EditRoadmapFormValues>()
  const formValues = Form.useWatch([], form)

  const {
    data: roadmapData,
    isLoading,
    error,
    refetch,
  } = useGetRoadmapQuery(props.roadmapId)
  const {
    data: visibilityData,
    isLoading: visibilityLoading,
    error: visibilityError,
  } = useGetVisibilityOptionsQuery()
  const [updateRoadmap, { error: mutationError }] = useUpdateRoadmapMutation()

  const { hasPermissionClaim } = useAuth()
  const canUpdateRoadmap = hasPermissionClaim('Permissions.Roadmaps.Update')

  const mapToFormValues = useCallback(
    (roadmap: RoadmapDetailsDto) => {
      if (!roadmap) {
        throw new Error('Roadmap not found.')
      }
      form.setFieldsValue({
        name: roadmap.name,
        description: roadmap.description,
        start: dayjs(roadmap.start),
        end: dayjs(roadmap.end),
        visibilityId: roadmap.visibility.id,
      })
    },
    [form],
  )

  const create = async (
    values: EditRoadmapFormValues,
    roadmap: RoadmapDetailsDto,
  ) => {
    try {
      const request = mapToRequestValues(values, roadmap.id)
      await updateRoadmap({ roadmapKey: roadmap.key, request })
        .unwrap()
        .then((response) => {
          props.messageApi.success(`Roadmap updated successfully.`)
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
      if (await create(values, roadmapData)) {
        setIsOpen(false)
        form.resetFields()
        props.onFormComplete()
      }
    } catch (error) {
      console.error('handleOk error', error)
      props.messageApi.error(
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
      props.messageApi.error('You do not have permission to update roadmaps.')
    }
  }, [canUpdateRoadmap, mapToFormValues, props, roadmapData, visibilityData])

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
          'An error occurred while loading the roadmap. Please try again.',
      )
    }
    if (visibilityError) {
      props.messageApi.error(
        visibilityError.supportMessage ??
          'An error occurred while loading visibility options. Please try again.',
      )
    }
  }, [error, props, visibilityError])

  return (
    <>
      <Modal
        title="Edit Roadmap"
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
          name="update-roadmap-form"
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

export default EditRoadmapForm
