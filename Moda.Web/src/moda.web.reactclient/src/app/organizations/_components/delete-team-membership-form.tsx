import { TeamMembershipDto } from '@/src/services/moda-api'
import { TeamTypeName } from '../types'
import { useCallback, useEffect, useState } from 'react'
import { Descriptions, Modal } from 'antd'
import dayjs from 'dayjs'
import { useDeleteTeamMembershipMutation } from '@/src/store/features/organizations/team-api'
import useAuth from '../../../components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'

const { Item } = Descriptions

interface DeleteTeamMembershipFormProps {
  showForm: boolean
  membership: TeamMembershipDto
  teamType: TeamTypeName
  onFormSave: () => void
  onFormCancel: () => void
}

const mapToRequestValues = (
  membership: TeamMembershipDto,
  teamType: TeamTypeName,
) => {
  return {
    teamMembershipId: membership.id,
    teamId: membership.child.id,
    parentTeamId: membership.parent.id,
    teamType,
  }
}

const DeleteTeamMembershipForm = (props: DeleteTeamMembershipFormProps) => {
  const [isOpen, setIsOpen] = useState(props.showForm)
  const [isSaving, setIsSaving] = useState(false)
  const messageApi = useMessage()

  const [deleteTeamMembershipMutation] = useDeleteTeamMembershipMutation()

  const { hasClaim } = useAuth()
  const canManageTeamMemberships = hasClaim(
    'Permission',
    'Permissions.Teams.ManageTeamMemberships',
  )

  const deleteTeamMembership = async (
    membership: TeamMembershipDto,
  ): Promise<boolean> => {
    try {
      const request = mapToRequestValues(props.membership, props.teamType)
      await deleteTeamMembershipMutation(request).unwrap()
      return true
    } catch (error) {
      messageApi.error(
        'An unexpected error occurred while deleting the team membership.',
      )
      console.log(error)
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      if (await deleteTeamMembership(props.membership)) {
        setIsOpen(false)
        props.onFormSave()
        messageApi.success('Successfully deleted team membership.')
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = useCallback(() => {
    setIsOpen(false)
    props.onFormCancel()
  }, [props])

  useEffect(() => {
    if (!props.membership || !props.teamType) return
    if (canManageTeamMemberships) {
      setIsOpen(props.showForm)
    } else {
      handleCancel()
      messageApi.error('You do not have permission to manage team memberships.')
    }
  }, [
    canManageTeamMemberships,
    handleCancel,
    messageApi,
    props.membership,
    props.showForm,
    props.teamType,
  ])

  return (
    <Modal
      title="Are you sure you want to delete this team membership?"
      open={isOpen}
      onOk={handleOk}
      okText="Delete"
      okType="danger"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false} // disable esc key to close modal
      destroyOnHidden={true}
    >
      <Descriptions size="small" column={1}>
        <Item label="Team">{props.membership?.child.name}</Item>
        <Item label="Parent Team">{props.membership?.parent.name}</Item>
        <Item label="Start">
          {dayjs(props.membership?.start).format('M/D/YYYY')}
        </Item>
        <Item label="End">
          {props.membership?.end
            ? dayjs(props.membership?.end).format('M/D/YYYY')
            : null}
        </Item>
      </Descriptions>
    </Modal>
  )
}

export default DeleteTeamMembershipForm
