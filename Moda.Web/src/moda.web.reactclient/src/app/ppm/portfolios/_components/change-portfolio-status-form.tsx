'use client'

import useAuth from '@/src/components/contexts/auth'
import { ProjectPortfolioDetailsDto } from '@/src/services/moda-api'
import {
  useActivatePortfolioMutation,
  useArchivePortfolioMutation,
  useClosePortfolioMutation,
} from '@/src/store/features/ppm/portfolios-api'
import { Modal, Space } from 'antd'
import { MessageInstance } from 'antd/es/message/interface'
import { useEffect, useState } from 'react'

export enum PortfolioStatusAction {
  Activate = 'Activate',
  Close = 'Close',
  Archive = 'Archive',
}

const statusActionToPastTense = (statusAction: PortfolioStatusAction) => {
  switch (statusAction) {
    case PortfolioStatusAction.Activate:
      return 'activated'
    case PortfolioStatusAction.Close:
      return 'closed'
    case PortfolioStatusAction.Archive:
      return 'archived'
    default:
      return statusAction
  }
}

const statusActionToPresentTense = (statusAction: PortfolioStatusAction) => {
  switch (statusAction) {
    case PortfolioStatusAction.Activate:
      return 'activating'
    case PortfolioStatusAction.Close:
      return 'closing'
    case PortfolioStatusAction.Archive:
      return 'archiving'
    default:
      return statusAction
  }
}

export interface ChangePortfolioStatusFormProps {
  portfolio: ProjectPortfolioDetailsDto
  statusAction: PortfolioStatusAction
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
  messageApi: MessageInstance
}

const ChangePortfolioStatusForm = (props: ChangePortfolioStatusFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)

  const [activatePortfolioMutation, { error: activateError }] =
    useActivatePortfolioMutation()
  const [closePortfolioMutation, { error: closeError }] =
    useClosePortfolioMutation()
  const [archivePortfolioMutation, { error: archiveError }] =
    useArchivePortfolioMutation()

  const { hasPermissionClaim } = useAuth()
  const canUpdatePortfolio = hasPermissionClaim(
    'Permissions.ProjectPortfolios.Update',
  )

  const changeStatus = async (
    id: string,
    cacheKey: number,
    statusAction: PortfolioStatusAction,
  ) => {
    try {
      const request = { id: id, cacheKey: cacheKey }
      let response = null
      if (statusAction === PortfolioStatusAction.Activate) {
        response = await activatePortfolioMutation(request)
      } else if (statusAction === PortfolioStatusAction.Close) {
        response = await closePortfolioMutation(request)
      } else if (statusAction === PortfolioStatusAction.Archive) {
        response = await archivePortfolioMutation(request)
      }

      if (response.error) {
        throw response.error
      }

      return true
    } catch (error) {
      props.messageApi.error(
        error.detail ??
          `An unexpected error occurred while ${statusActionToPresentTense(statusAction)} the portfolio.`,
      )
      console.log(error)
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      if (
        await changeStatus(
          props.portfolio.id,
          props.portfolio.key,
          props.statusAction,
        )
      ) {
        props.messageApi.success(
          `Successfully ${statusActionToPastTense(props.statusAction)} Portfolio.`,
        )
        props.onFormComplete()
        setIsOpen(false)
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
      props.messageApi.error(
        `An unexpected error occurred while ${statusActionToPresentTense(props.statusAction)} the portfolio.`,
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
    if (canUpdatePortfolio) {
      setIsOpen(props.showForm)
    } else {
      props.onFormCancel()
      props.messageApi.error('You do not have permission to update portfolios.')
    }
  }, [canUpdatePortfolio, props])

  return (
    <>
      <Modal
        title={`Are you sure you want to ${props.statusAction} this Portfolio?`}
        open={isOpen}
        onOk={handleOk}
        okText={props.statusAction}
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnClose={true}
      >
        <Space direction="vertical">
          <div>
            {props.portfolio?.key} - {props.portfolio?.name}
          </div>
          {'This action cannot be undone.'}
        </Space>
      </Modal>
    </>
  )
}

export default ChangePortfolioStatusForm
