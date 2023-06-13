import { ProfileClient } from "@/app/services/moda-api"
import { Table } from "antd"
import { useEffect, useState } from "react"

interface Claim {
  key: string
  value: string
}

const columns = [
  {
    title: 'Claim',
    dataIndex: 'key',
    key: 'key',
  },
  {
    title: 'Value',
    dataIndex: 'value',
    key: 'value',
  }
]

const ClaimsGrid = () => {

  const [claims, setClaims] = useState<Claim[]>([])
  useEffect(() => {
    const fetchClaims = async () => {
      const profileClient = new ProfileClient('https://localhost:44317')
      const claimsResponse = await profileClient.getPermissions()
      setClaims(claimsResponse.map(claim => {
        return {
          key: claim,
          value: claim
        }
      }))
    }
    fetchClaims()
  }, [setClaims])

  return (
    <Table dataSource={claims} columns={columns}/>
  )
}

export default ClaimsGrid