import { getProfileClient } from "@/src/services/clients"
import { List } from "antd"
import { useEffect, useState } from "react"

const PermissionsList = () => {
  const [permissions, setPermissions] = useState<string[]>([])

  useEffect(() => {
    const getPermissions = async () => {
      const profileClient = await getProfileClient()
      const permissions = await profileClient.getPermissions()
      setPermissions(permissions.sort())
    }

    getPermissions()
  },[])

  return (
    <List
      bordered
      dataSource={permissions}
      renderItem={item => <List.Item>{item}</List.Item>}
    />
  )
}

export default PermissionsList