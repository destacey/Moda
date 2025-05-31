'use client'

import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { ProjectPortfolioDetailsDto } from '@/src/services/moda-api'
import { useDeletePortfolioMutation } from '@/src/store/features/ppm/portfolios-api'
import { Modal } from 'antd'
import { useEffect, useState } from 'react'

export interface DeletePortfolioFormProps {
  portfolio: ProjectPortfolioDetailsDto
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
}

const DeletePortfolioForm = (props: DeletePortfolioFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)

  const messageApi = useMessage()

  const [deletePortfolioMutation, { error: mutationError }] =
    useDeletePortfolioMutation()

  const { hasPermissionClaim } = useAuth()
  const canDeletePortfolio = hasPermissionClaim(
    'Permissions.ProjectPortfolios.Delete',
  )

  const deletePortfolio = async (portfolio: ProjectPortfolioDetailsDto) => {
    try {
      const response = await deletePortfolioMutation({
        portfolioId: portfolio.id,
        cacheKey: portfolio.key,
      })

      if (response.error) {
        throw response.error
      }

      return true
    } catch (error) {
      messageApi.error(
        error.detail ??
          'An unexpected error occurred while deleting the portfolio.',
      )
      console.log(error)
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      if (await deletePortfolio(props.portfolio)) {
        // TODO: not working because the parent page is gone
        messageApi.success('Successfully deleted Portfolio.')
        props.onFormComplete()
        setIsOpen(false)
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
      messageApi.error(
        'An unexpected error occurred while deleting the portfolio.',
      )
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = () => {
    setIsOpen(false)
    props.onFormCancel()
  }

  useEffect(() => {
    if (!props.portfolio) return
    if (canDeletePortfolio) {
      setIsOpen(props.showForm)
    } else {
      props.onFormCancel()
    }
  }, [canDeletePortfolio, props])

  return (
    <>
      <Modal
        title="Are you sure you want to delete this Portfolio?"
        open={isOpen}
        onOk={handleOk}
        okText="Delete"
        okType="danger"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnHidden={true}
      >
        {props.portfolio?.key} - {props.portfolio?.name}
      </Modal>
    </>
  )
}

export default DeletePortfolioForm
