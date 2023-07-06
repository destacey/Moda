'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { UserDetailsDto } from '@/src/services/moda-api'
import { useState } from 'react'

const Page = ({ params }) => {
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

export default Page
