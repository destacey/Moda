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

export enum PortfolioStateAction {
  Activate = 'Activate',
  Close = 'Close',
  Archive = 'Archive',
}

const stateActionToPastTense = (stateAction: PortfolioStateAction) => {
  switch (stateAction) {
    case PortfolioStateAction.Activate:
      return 'activated'
    case PortfolioStateAction.Close:
      return 'closed'
    case PortfolioStateAction.Archive:
      return 'archived'
    default:
      return stateAction
  }
}

const stateActionToPresentTense = (stateAction: PortfolioStateAction) => {
  switch (stateAction) {
    case PortfolioStateAction.Activate:
      return 'activating'
    case PortfolioStateAction.Close:
      return 'closing'
    case PortfolioStateAction.Archive:
      return 'archiving'
    default:
      return stateAction
  }
}

export interface ChangePortfolioStateFormProps {
  portfolio: ProjectPortfolioDetailsDto
  stateAction: PortfolioStateAction
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
  messageApi: MessageInstance
}

const ChangePortfolioStateForm = (props: ChangePortfolioStateFormProps) => {
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

  const changeState = async (
    id: string,
    cacheKey: number,
    stateAction: PortfolioStateAction,
  ) => {
    try {
      const request = { id: id, cacheKey: cacheKey }
      let response = null
      if (stateAction === PortfolioStateAction.Activate) {
        response = await activatePortfolioMutation(request)
      } else if (stateAction === PortfolioStateAction.Close) {
        response = await closePortfolioMutation(request)
      } else if (stateAction === PortfolioStateAction.Archive) {
        response = await archivePortfolioMutation(request)
      }

      if (response.error) {
        throw response.error
      }

      return true
    } catch (error) {
      props.messageApi.error(
        error.detail ??
          `An unexpected error occurred while ${stateActionToPresentTense(stateAction)} the portfolio.`,
      )
      console.log(error)
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      if (
        await changeState(
          props.portfolio.id,
          props.portfolio.key,
          props.stateAction,
        )
      ) {
        props.messageApi.success(
          `Successfully ${stateActionToPastTense(props.stateAction)} Portfolio.`,
        )
        props.onFormComplete()
        setIsOpen(false)
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
      props.messageApi.error(
        `An unexpected error occurred while ${stateActionToPresentTense(props.stateAction)} the portfolio.`,
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
    }
  }, [canUpdatePortfolio, props])

  return (
    <>
      <Modal
        title={`Are you sure you want to ${props.stateAction} this Portfolio?`}
        open={isOpen}
        onOk={handleOk}
        okText={props.stateAction}
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

export default ChangePortfolioStateForm
