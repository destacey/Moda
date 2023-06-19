import { useEffect, useState } from "react";
import ModaGrid from "../../components/common/moda-grid";
import useAuth, { Claim } from "../../components/contexts/auth";

const columnDefs = [
  { field: 'type', },
  { field: 'value', width: 500 }
]

const ClaimsGrid = () => {
  const { user } = useAuth()
  const [rowData, setRowData] = useState<Claim[]>([])

  useEffect(() => {
    setRowData(user.claims)
  }, [user])

  return (
    <ModaGrid columnDefs={columnDefs}
      rowData={rowData}
      width={700} />
  )
}

export default ClaimsGrid