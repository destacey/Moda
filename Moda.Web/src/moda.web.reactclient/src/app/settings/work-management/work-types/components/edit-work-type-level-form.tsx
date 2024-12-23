import { MarkdownEditor } from '@/src/app/components/common/markdown'
import useAuth from '@/src/app/components/contexts/auth'
import {
  UpdateWorkTypeLevelRequest,
  WorkTypeLevelDto,
} from '@/src/services/moda-api'
import {
  useGetWorkTypeLevelQuery,
  useUpdateWorkTypeLevelMutation,
} from '@/src/store/features/work-management/work-type-level-api'
import { toFormErrors } from '@/src/utils'
import { Form, Input, Modal, message } from 'antd'
import { useCallback, useEffect, useState } from 'react'

const { Item } = Form
const { TextArea } = Input

export interface EditWorkTypeLevelFormProps {
  levelId: number
  showForm: boolean
  onFormSave: () => void
  onFormCancel: () => void
}

interface EditWorkTypeLevelFormValues {
  name: string
  description?: string
}

const mapToRequestValues = (
  levelId: number,
  values: EditWorkTypeLevelFormValues,
): UpdateWorkTypeLevelRequest => {
  return {
    id: levelId,
    name: values.name,
    description: values.description,
  }
}

const EditWorkTypeLevelForm = (props: EditWorkTypeLevelFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<EditWorkTypeLevelFormValues>()
  const formValues = Form.useWatch([], form)
  const [messageApi, contextHolder] = message.useMessage()

  const { data: workTypeLevelData } = useGetWorkTypeLevelQuery(props.levelId)
  const [updateLevelMutation] = useUpdateWorkTypeLevelMutation()

  const { hasClaim } = useAuth()
  const canUpdateWorkTypeLevel = hasClaim(
    'Permission',
    'Permissions.WorkTypeLevels.Update',
  )

  const mapToFormValues = useCallback(
    (level: WorkTypeLevelDto) => {
      form.setFieldsValue({
        name: level.name,
        description: level.description || '',
      })
    },
    [form],
  )

  const update = async (
    values: EditWorkTypeLevelFormValues,
  ): Promise<boolean> => {
    try {
      const request = mapToRequestValues(props.levelId, values)
      const response = await updateLevelMutation(request)
      if (response.error) {
        throw response.error
      }
      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          'An unexpected error occurred while updating the work type level.',
        )
        console.error(error)
      }
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (await update(values)) {
        setIsOpen(false)
        form.resetFields()
        props.onFormSave()
        messageApi.success('Successfully updated work type level.')
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
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
    if (!workTypeLevelData) return

    if (canUpdateWorkTypeLevel) {
      setIsOpen(props.showForm)
      if (props.showForm) {
        try {
          mapToFormValues(workTypeLevelData)
          setIsValid(true)
        } catch (error) {
          props.onFormCancel()
          messageApi.error(
            'An unexpected error occurred while loading form data.',
          )
          console.error(error)
        }
      }
    } else {
      props.onFormCancel()
      messageApi.error('You do not have permission to edit work type levels.')
    }
  }, [
    canUpdateWorkTypeLevel,
    messageApi,
    workTypeLevelData,
    props,
    mapToFormValues,
  ])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  return (
    <>
      {contextHolder}
      <Modal
        title="Edit Work Type Level"
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
          name="edit-work-type-level-form"
        >
          <Item label="Name" name="name" rules={[{ required: true }]}>
            <TextArea
              autoSize={{ minRows: 1, maxRows: 2 }}
              showCount
              maxLength={128}
            />
          </Item>
          <Item name="description" label="Description" rules={[{ max: 1024 }]}>
            <MarkdownEditor
              value={form.getFieldValue('description')}
              onChange={(value) =>
                form.setFieldValue('description', value || '')
              }
              maxLength={1024}
            />
          </Item>
        </Form>
      </Modal>
    </>
  )
}

export default EditWorkTypeLevelForm
