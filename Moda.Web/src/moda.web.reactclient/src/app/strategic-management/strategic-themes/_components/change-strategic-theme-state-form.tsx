'use client'

import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { StrategicThemeDetailsDto } from '@/src/services/moda-api'
import {
  useActivateStrategicThemeMutation,
  useArchiveStrategicThemeMutation,
} from '@/src/store/features/strategic-management/strategic-themes-api'
import { Modal, Space } from 'antd'
import { useEffect, useState } from 'react'

export enum StrategicThemeStateAction {
  Activate = 'Activate',
  Archive = 'Archive',
}

export interface ChangeStrategicThemeStateFormProps {
  strategicTheme: StrategicThemeDetailsDto
  stateAction: StrategicThemeStateAction
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
}

const ChangeStrategicThemeStateForm = (
  props: ChangeStrategicThemeStateFormProps,
) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)

  const messageApi = useMessage()

  const [activateStrategicThemeMutation, { error: activateError }] =
    useActivateStrategicThemeMutation()
  const [archiveStrategicThemeMutation, { error: archiveError }] =
    useArchiveStrategicThemeMutation()

  const { hasPermissionClaim } = useAuth()
  const canUpdateStrategicTheme = hasPermissionClaim(
    'Permissions.StrategicThemes.Update',
  )

  const changeState = async (
    id: string,
    cacheKey: number,
    stateAction: StrategicThemeStateAction,
  ) => {
    try {
      let response = null
      if (stateAction === StrategicThemeStateAction.Activate) {
        response = await activateStrategicThemeMutation({
          id: id,
          cacheKey: cacheKey,
        })
      } else if (stateAction === StrategicThemeStateAction.Archive) {
        response = await archiveStrategicThemeMutation({
          id: id,
          cacheKey: cacheKey,
        })
      }

      if (response.error) {
        throw response.error
      }

      return true
    } catch (error) {
      messageApi.error(
        error.detail ??
          `An unexpected error occurred while ${stateAction}ing the strategic theme.`,
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
          props.strategicTheme.id,
          props.strategicTheme.key,
          props.stateAction,
        )
      ) {
        messageApi.success(
          `Successfully ${props.stateAction}d Strategic Theme.`,
        )
        props.onFormComplete()
        setIsOpen(false)
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
      messageApi.error(
        `An unexpected error occurred while ${props.stateAction}ing the strategic theme.`,
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
    if (canUpdateStrategicTheme) {
      setIsOpen(props.showForm)
    } else {
      props.onFormCancel()
    }
  }, [canUpdateStrategicTheme, props])

  return (
    <>
      <Modal
        title={`Are you sure you want to ${props.stateAction} this Strategic Theme?`}
        open={isOpen}
        onOk={handleOk}
        okText={props.stateAction}
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnHidden={true}
      >
        <Space vertical>
          <div>
            {props.strategicTheme?.key} - {props.strategicTheme?.name}
          </div>
          {'This action cannot be undone.'}
        </Space>
      </Modal>
    </>
  )
}

export default ChangeStrategicThemeStateForm
