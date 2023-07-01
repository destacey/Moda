'use client'

import { Modal } from 'antd'
import { useEffect, useState } from 'react'

export interface CreateTeamModalProps {
  showModal: boolean
  onModalClose: () => void
}

const CreateTeamModal = ({ showModal, onModalClose }: CreateTeamModalProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [confirmLoading, setConfirmLoading] = useState(false)

  console.log('modal open state:  ' + isOpen)

  useEffect(() => {
    setIsOpen(showModal)
  }, [showModal])

  const handleOk = () => {
    setConfirmLoading(true)
    setTimeout(() => {
      setIsOpen(false)
      onModalClose()
      setConfirmLoading(false)
    }, 2000)
  }

  const handleCancel = () => {
    console.log('Clicked cancel button')
    setIsOpen(false)
    onModalClose()
  }

  return (
    <Modal
      title="Create Team"
      open={isOpen}
      onOk={handleOk}
      confirmLoading={confirmLoading}
      onCancel={handleCancel}
    >
      <p>This is a test</p>
    </Modal>
  )
}

export default CreateTeamModal
