'use client'

import {
  useDeleteEmployeeMutation,
  useGetEmployeeQuery,
} from '@/src/store/features/organizations/employee-api'
import { useMessage } from '@/src/components/contexts/messaging'
import { useEffect, useState } from 'react'
import useAuth from '@/src/components/contexts/auth'
import { Modal } from 'antd'

export interface DeleteEmployeeFormProps {
  employeeKey: number
  onFormComplete: () => void
  onFormCancel: () => void
}

const DeleteEmployeeForm = (props: DeleteEmployeeFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)

  const messageApi = useMessage()

  const { hasPermissionClaim } = useAuth()
  const canDeleteEmployee = hasPermissionClaim('Permissions.Employees.Delete')

  const { data: employeeData } = useGetEmployeeQuery(props.employeeKey)

  const [deleteEmployeeMutation, { error: mutationError }] =
    useDeleteEmployeeMutation()

  useEffect(() => {
    if (!employeeData) return
    if (canDeleteEmployee) {
      setIsOpen(true)
    } else {
      props.onFormCancel()
      messageApi.error('You do not have permission to delete employees.')
    }
  }, [employeeData, canDeleteEmployee, messageApi, props])

  const deleteEmployee = async (employeeId: string) => {
    try {
      await deleteEmployeeMutation(employeeId)
      return true
    } catch (error) {
      messageApi.error(
        'An unexpected error occurred while deleting the employee.',
      )
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      if (await deleteEmployee(employeeData.id)) {
        messageApi.success('Successfully deleted Employee.')
        props.onFormComplete()
        setIsOpen(false)
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
      messageApi.error(
        'An unexpected error occurred while deleting the employee.',
      )
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = () => {
    setIsOpen(false)
    props.onFormCancel()
  }

  return (
    <>
      <Modal
        title="Are you sure you want to delete this Employee?"
        open={isOpen}
        onOk={handleOk}
        okText="Delete"
        okType="danger"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        keyboard={false} // disable esc key to close modal
        destroyOnHidden={true}
      >
        {employeeData.key} - {employeeData.displayName}
      </Modal>
    </>
  )
}

export default DeleteEmployeeForm
