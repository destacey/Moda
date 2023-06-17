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
    <div style={{height:700, overflow:'auto'}}>
      <List
        bordered
        dataSource={permissions}
        renderItem={item => <List.Item>{item}</List.Item>}
      />
    </div>
  )
}

export default PermissionsList