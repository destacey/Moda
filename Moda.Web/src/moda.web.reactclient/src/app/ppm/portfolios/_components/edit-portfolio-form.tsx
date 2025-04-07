'use client'

import { MarkdownEditor } from '@/src/components/common/markdown'
import { EmployeeSelect } from '@/src/components/common/organizations'
import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import {
  ProjectPortfolioDetailsDto,
  UpdatePortfolioRequest,
} from '@/src/services/moda-api'
import { useGetEmployeeOptionsQuery } from '@/src/store/features/organizations/employee-api'
import {
  useGetPortfolioQuery,
  useUpdatePortfolioMutation,
} from '@/src/store/features/ppm/portfolios-api'
import { toFormErrors } from '@/src/utils'
import { Form, Input, Modal } from 'antd'
import { BaseOptionType } from 'antd/es/select'
import { useCallback, useEffect, useState } from 'react'

const { Item } = Form

export interface EditPortfolioFormProps {
  portfolioKey: number
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
}

interface UpdatePortfolioFormValues {
  name: string
  description: string
  sponsorIds: string[]
  ownerIds: string[]
  managerIds: string[]
}

const mapToRequestValues = (
  values: UpdatePortfolioFormValues,
  portfolioId: string,
): UpdatePortfolioRequest => {
  return {
    id: portfolioId,
    name: values.name,
    description: values.description,
    sponsorIds: values.sponsorIds,
    ownerIds: values.ownerIds,
    managerIds: values.managerIds,
  } as UpdatePortfolioRequest
}

const EditPortfolioForm = (props: EditPortfolioFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<UpdatePortfolioFormValues>()
  const formValues = Form.useWatch([], form)
  const [employees, setEmployees] = useState<BaseOptionType[]>([])

  const messageApi = useMessage()

  const {
    data: portfolioData,
    isLoading,
    error,
  } = useGetPortfolioQuery(props.portfolioKey)

  const [updatePortfolio, { error: mutationError }] =
    useUpdatePortfolioMutation()

  const {
    data: employeeData,
    isLoading: employeeOptionsIsLoading,
    error: employeeOptionsError,
  } = useGetEmployeeOptionsQuery(false)

  const { hasPermissionClaim } = useAuth()
  const canUpdatePortfolio = hasPermissionClaim(
    'Permissions.ProjectPortfolios.Update',
  )

  const mapToFormValues = useCallback(
    (portfolio: ProjectPortfolioDetailsDto) => {
      if (!portfolio) {
        throw new Error('Portfolio not found')
      }
      form.setFieldsValue({
        name: portfolio.name,
        description: portfolio.description,
        sponsorIds: portfolio.portfolioSponsors.map((p) => p.id),
        ownerIds: portfolio.portfolioOwners.map((p) => p.id),
        managerIds: portfolio.portfolioManagers.map((p) => p.id),
      })
    },
    [form],
  )

  const update = async (values: UpdatePortfolioFormValues) => {
    try {
      const request = mapToRequestValues(values, portfolioData.id)
      const response = await updatePortfolio({
        request,
        cacheKey: portfolioData.key,
      })
      if (response.error) {
        throw response.error
      }
      messageApi.success('Portfolio updated successfully.')

      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.detail ??
            'An error occurred while updating the Portfolio. Please try again.',
        )
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
        props.onFormComplete()
      }
    } catch (error) {
      console.error('update error', error)
      messageApi.error(
        'An error occurred while updating the Portfolio. Please try again.',
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
    if (!portfolioData) return

    if (canUpdatePortfolio) {
      setIsOpen(props.showForm)
      if (props.showForm) {
        mapToFormValues(portfolioData)
      }
    } else {
      props.onFormCancel()
      messageApi.error('You do not have permission to update Portfolios.')
    }
  }, [
    canUpdatePortfolio,
    mapToFormValues,
    props,
    portfolioData,
    employeeData,
    messageApi,
  ])

  useEffect(() => {
    // TODO: how do we make this reusable?  Inside EmployeeSelect? or a custom hook?

    if (!employeeData) return
    const employeeOptions = [...employeeData]

    if (portfolioData) {
      const assignedEmployees = [
        ...portfolioData.portfolioSponsors,
        ...portfolioData.portfolioOwners,
        ...portfolioData.portfolioManagers,
      ]

      if (employeeOptions && employeeOptions.length > 0) {
        const missingEmployees = assignedEmployees.filter(
          (e) => !employeeOptions.find((emp) => emp.value === e.id),
        )
        if (missingEmployees.length > 0) {
          employeeOptions.push(
            ...missingEmployees.map((e) => ({
              value: e.id,
              label: `${e.name} (Inactive)`,
            })),
          )
        }
      }
    }

    setEmployees(employeeOptions.sort((a, b) => a.label.localeCompare(b.label)))
  }, [employeeData, portfolioData])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  useEffect(() => {
    if (error) {
      messageApi.error(
        error.detail ??
          'An error occurred while loading the Portfolio. Please try again.',
      )
    }
  }, [error, messageApi])

  return (
    <>
      <Modal
        title="Edit Portfolio"
        open={isOpen}
        width={'60vw'}
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
          name="edit-portfolio-form"
        >
          <Item
            label="Name"
            name="name"
            rules={[
              { required: true, message: 'Name is required' },
              { max: 128 },
            ]}
          >
            <Input maxLength={128} showCount />
          </Item>
          <Item
            name="description"
            label="Description"
            rules={[
              { required: true, message: 'Description is required' },
              { max: 1024 },
            ]}
          >
            <MarkdownEditor
              value={form.getFieldValue('description')}
              onChange={(value) =>
                form.setFieldValue('description', value || '')
              }
              maxLength={1024}
            />
          </Item>
          <Item name="sponsorIds" label="Sponsors">
            <EmployeeSelect
              employees={employees}
              allowMultiple={true}
              placeholder="Select Sponsors"
            />
          </Item>
          <Item name="ownerIds" label="Owners">
            <EmployeeSelect
              employees={employees}
              allowMultiple={true}
              placeholder="Select Owners"
            />
          </Item>
          <Item name="managerIds" label="Managers">
            <EmployeeSelect
              employees={employees}
              allowMultiple={true}
              placeholder="Select Managers"
            />
          </Item>
        </Form>
      </Modal>
    </>
  )
}

export default EditPortfolioForm
