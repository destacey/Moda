import { UserDetailsDto } from '@/src/services/moda-api'
import { Descriptions } from 'antd'

const { Item } = Descriptions

const ProfileForm = (profile: UserDetailsDto) => {
  return (
    <>
      <Descriptions>
        <Item label="First Name">{profile?.firstName}</Item>
        <Item label="Last Name">{profile?.lastName}</Item>
        <Item label="Email">{profile?.email}</Item>
      </Descriptions>
    </>
  )
}

export default ProfileForm
