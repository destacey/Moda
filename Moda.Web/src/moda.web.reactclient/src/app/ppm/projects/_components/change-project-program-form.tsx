'use client'

import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import {
  ChangeProjectProgramRequest,
  ProjectDetailsDto,
} from '@/src/services/moda-api'
import { useGetPortfolioProgramOptionsQuery } from '@/src/store/features/ppm/portfolios-api'
import { useChangeProjectProgramMutation } from '@/src/store/features/ppm/projects-api'
import { toFormErrors } from '@/src/utils'
import { Flex, Form, Modal, Select, Typography } from 'antd'
import { useCallback, useEffect, useState } from 'react'

const { Item } = Form
const { Text } = Typography

export interface ChangeProjectProgramFormProps {
  project: ProjectDetailsDto
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
}

interface ChangeProjectProgramFormValues {
  programId?: string
}

const ChangeProjectProgramForm = (props: ChangeProjectProgramFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<ChangeProjectProgramFormValues>()
  const formValues = Form.useWatch([], form)

  const messageApi = useMessage()

  const [changeProjectProgram, { error: mutationError }] =
    useChangeProjectProgramMutation()

  const {
    data: programData,
    isLoading: programOptionsIsLoading,
    error: programOptionsError,
  } = useGetPortfolioProgramOptionsQuery(props.project.portfolio.id)

  const { hasPermissionClaim } = useAuth()
  const canUpdateProject = hasPermissionClaim('Permissions.Projects.Update')

  const mapToFormValues = useCallback(
    (project: ProjectDetailsDto) => {
      if (!project) {
        throw new Error('Project not found')
      }
      form.setFieldsValue({
        programId: project.program?.id ?? undefined,
      })
    },
    [form],
  )

  const changeProgram = async (
    values: ChangeProjectProgramFormValues,
    project: ProjectDetailsDto,
  ) => {
    try {
      const request: ChangeProjectProgramRequest = {
        programId: values.programId ?? null,
      }
      const response = await changeProjectProgram({
        id: project.id,
        request,
        cacheKey: project.key,
      })
      if (response.error) {
        throw response.error
      }
      messageApi.success(`Project program changed successfully.`)
      return true
    } catch (error) {
      console.error('change program error', error)
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.detail ??
            'An error occurred while changing the project program. Please try again.',
        )
      }
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (await changeProgram(values, props.project)) {
        setIsOpen(false)
        form.resetFields()
        props.onFormComplete()
      }
    } catch (error) {
      console.error('handleOk error', error)
      messageApi.error(
        'An error occurred while changing the project program. Please try again.',
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
    if (!props.project) return
    if (canUpdateProject) {
      setIsOpen(props.showForm)
      if (props.showForm) {
        mapToFormValues(props.project)
      }
    } else {
      props.onFormCancel()
      messageApi.error('You do not have permission to update projects.')
    }
  }, [canUpdateProject, mapToFormValues, messageApi, props])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  useEffect(() => {
    if (programOptionsError) {
      console.error(programOptionsError)
      messageApi.error(
        programOptionsError.detail ??
          'An error occurred while loading program options. Please try again.',
      )
    }
  }, [programOptionsError, messageApi])

  return (
    <>
      <Modal
        title="Change Project Program"
        open={isOpen}
        width={'40vw'}
        onOk={handleOk}
        okButtonProps={{ disabled: !isValid }}
        okText="Save"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        mask={{ blur: false }}
        maskClosable={false}
        keyboard={false}
        destroyOnHidden={true}
      >
        <Flex vertical gap="small">
          <Text type="secondary">
            Select a new program to assign this project to, or clear to remove
            the program assignment.
          </Text>
          <Form
            form={form}
            size="small"
            layout="vertical"
            name="change-project-program-form"
          >
            <Item name="programId" label="Program">
              <Select
                allowClear
                options={programData ?? []}
                placeholder="Select Program"
                loading={programOptionsIsLoading}
                optionFilterProp="label"
                filterOption={(input, option) =>
                  (option?.label?.toLowerCase() ?? '').includes(
                    input.toLowerCase(),
                  )
                }
              />
            </Item>
          </Form>
        </Flex>
      </Modal>
    </>
  )
}

export default ChangeProjectProgramForm
