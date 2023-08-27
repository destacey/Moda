'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { authorizePage } from '@/src/app/components/hoc'
import { UserDetailsDto } from '@/src/services/moda-api'
import { useState } from 'react'

const UserDetailsPage = ({ params }) => {
  const [user, setUser] = useState<UserDetailsDto | null>(null)

  return (
    <>
      <PageTitle
        title={user?.userName ?? `Test ${params.id}`}
        subtitle="User Details"
      />
    </>
  )
}

const UserDetailsPageWithAuthorization = authorizePage(
  UserDetailsPage,
  'Permission',
  'Permissions.Users.View',
)

export default UserDetailsPageWithAuthorization
