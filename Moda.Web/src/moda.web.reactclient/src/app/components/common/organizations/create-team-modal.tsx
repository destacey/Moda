'use client'

import { Modal } from 'antd'
import { useState } from 'react'

export interface CreateTeamModalProps {
  isOpen: boolean
}

const CreateTeamModal = ({ isOpen }: CreateTeamModalProps) => {
  const [open, setOpen] = useState(isOpen)
  const [confirmLoading, setConfirmLoading] = useState(false)

  console.log('modal open state:  ' + open)

  const handleOk = () => {
    setConfirmLoading(true)
    setTimeout(() => {
      setOpen(false)
      setConfirmLoading(false)
    }, 2000)
  }

  const handleCancel = () => {
    console.log('Clicked cancel button')
    setOpen(false)
  }

  return (
    <Modal
      title="Create Team"
      open={open}
      onOk={handleOk}
      confirmLoading={confirmLoading}
      onCancel={handleCancel}
    >
      <p>This is a test</p>
    </Modal>
  )
}

export default CreateTeamModal
