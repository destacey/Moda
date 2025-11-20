'use client'

import { MarkdownEditor } from '@/src/components/common/markdown'
import { EmployeeSelect } from '@/src/components/common/organizations'
import useAuth from '@/src/components/contexts/auth'
import {
  ProgramDetailsDto,
  UpdateProgramRequest,
} from '@/src/services/moda-api'
import { useGetEmployeeOptionsQuery } from '@/src/store/features/organizations/employee-api'
import {
  useGetProgramQuery,
  useUpdateProgramMutation,
} from '@/src/store/features/ppm/programs-api'
import { useGetStrategicThemeOptionsQuery } from '@/src/store/features/strategic-management/strategic-themes-api'
import { DatePicker, Form, Modal, Select } from 'antd'
import TextArea from 'antd/es/input/TextArea'
import { useCallback, useEffect, useState } from 'react'
import dayjs from 'dayjs'
import { toFormErrors } from '@/src/utils'
import { useMessage } from '@/src/components/contexts/messaging'

const { Item } = Form

export interface EditProgramFormProps {
  programKey: number
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
}

interface EditProgramFormValues {
  name: string
  description: string
  start?: Date
  end?: Date
  sponsorIds: string[]
  ownerIds: string[]
  managerIds: string[]
  strategicThemeIds: string[]
}

const mapToRequestValues = (
  values: EditProgramFormValues,
  programId: string,
): UpdateProgramRequest => {
  return {
    id: programId,
    name: values.name,
    description: values.description,
    start: (values.start as any)?.format('YYYY-MM-DD'),
    end: (values.end as any)?.format('YYYY-MM-DD'),
    sponsorIds: values.sponsorIds,
    ownerIds: values.ownerIds,
    managerIds: values.managerIds,
    strategicThemeIds: values.strategicThemeIds,
  } as UpdateProgramRequest
}

const EditProgramForm = (props: EditProgramFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<EditProgramFormValues>()
  const formValues = Form.useWatch([], form)

  const messageApi = useMessage()

  const [updateProgram, { error: mutationError }] = useUpdateProgramMutation()

  const {
    data: programData,
    isLoading,
    error,
  } = useGetProgramQuery(props.programKey)

  const {
    data: employeeData,
    isLoading: employeeOptionsIsLoading,
    error: employeeOptionsError,
  } = useGetEmployeeOptionsQuery(true)

  const {
    data: strategicThemeData,
    isLoading: strategicThemeOptionsIsLoading,
    error: strategicThemeOptionsError,
  } = useGetStrategicThemeOptionsQuery(false)

  const { hasPermissionClaim } = useAuth()
  const canUpdateProgram = hasPermissionClaim('Permissions.Programs.Update')

  const mapToFormValues = useCallback(
    (program: ProgramDetailsDto) => {
      if (!program) {
        throw new Error('Program not found')
      }
      form.setFieldsValue({
        name: program.name,
        description: program.description,
        start: program.start ? dayjs(program.start) : undefined,
        end: program.end ? dayjs(program.end) : undefined,
        sponsorIds: program.programSponsors.map((s) => s.id),
        ownerIds: program.programOwners.map((o) => o.id),
        managerIds: program.programManagers.map((m) => m.id),
        strategicThemeIds: program.strategicThemes.map((t) => t.id),
      })
    },
    [form],
  )

  const update = async (
    values: EditProgramFormValues,
    program: ProgramDetailsDto,
  ) => {
    try {
      const request = mapToRequestValues(values, program.id)
      const response = await updateProgram({
        request,
        cacheKey: program.key,
      })
      if (response.error) {
        throw response.error
      }
      messageApi.success(`Program updated successfully.`)
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
            'An error occurred while updating the program. Please try again.',
        )
      }
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (await update(values, programData)) {
        setIsOpen(false)
        props.onFormComplete()
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
      messageApi.error(
        'An error occurred while updating the program. Please try again.',
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
    if (canUpdateProgram && programData) {
      setIsOpen(props.showForm)
      mapToFormValues(programData)
    } else if (!canUpdateProgram) {
      props.onFormCancel()
    }
  }, [canUpdateProgram, mapToFormValues, programData, props])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true),
      () => setIsValid(false),
    )
  }, [form, formValues])

  return (
    <>
      <Modal
        title="Edit Program"
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
          name="edit-program-form"
        >
          <Item
            label="Name"
            name="name"
            rules={[
              { required: true, message: 'Name is required' },
              { max: 128 },
            ]}
          >
            <TextArea
              autoSize={{ minRows: 1, maxRows: 2 }}
              showCount
              maxLength={128}
            />
          </Item>
          <Item
            name="description"
            label="Description"
            rules={[
              { required: true, message: 'Description is required' },
              { max: 2048 },
            ]}
          >
            <MarkdownEditor maxLength={2048} />
          </Item>
          <Item name="start" label="Start">
            <DatePicker />
          </Item>
          <Item
            name="end"
            label="End"
            dependencies={['start']}
            rules={[
              ({ getFieldValue }) => ({
                validator(_, value) {
                  const start = getFieldValue('start')
                  if ((!start && !value) || (start && start <= value)) {
                    return Promise.resolve()
                  } else if ((!start && value) || (start && !value)) {
                    return Promise.reject(
                      new Error(
                        'Start and end date must be selected together or both left empty',
                      ),
                    )
                  }
                  return Promise.reject(
                    new Error('End date must be on or after start date'),
                  )
                },
              }),
            ]}
          >
            <DatePicker />
          </Item>
          <Item name="sponsorIds" label="Sponsors">
            <EmployeeSelect
              employees={employeeData ?? []}
              allowMultiple={true}
              placeholder="Select Sponsors"
            />
          </Item>
          <Item name="ownerIds" label="Owners">
            <EmployeeSelect
              employees={employeeData ?? []}
              allowMultiple={true}
              placeholder="Select Owners"
            />
          </Item>
          <Item name="managerIds" label="Managers">
            <EmployeeSelect
              employees={employeeData ?? []}
              allowMultiple={true}
              placeholder="Select Managers"
            />
          </Item>
          <Item name="strategicThemeIds" label="Strategic Themes">
            <Select
              mode="multiple"
              allowClear
              options={strategicThemeData ?? []}
              placeholder="Select Strategic Themes"
              optionFilterProp="label"
              filterOption={(input, option) =>
                (option?.label?.toLowerCase() ?? '').includes(
                  input.toLowerCase(),
                )
              }
            />
          </Item>
        </Form>
      </Modal>
    </>
  )
}

export default EditProgramForm
