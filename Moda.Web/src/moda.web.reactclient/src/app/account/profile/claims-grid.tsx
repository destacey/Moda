import { useEffect, useMemo, useState } from 'react'
import ModaGrid from '../../components/common/moda-grid'
import useAuth, { Claim } from '../../components/contexts/auth'

const ClaimsGrid = () => {
  const { user } = useAuth()
  const [rowData, setRowData] = useState<Claim[]>([])

  const columnDefs = useMemo(
    () => [{ field: 'type' }, { field: 'value', width: 500 }],
    []
  )

  useEffect(() => {
    setRowData(user.claims)
  }, [user])

  return <ModaGrid columnDefs={columnDefs} rowData={rowData} width={700} />
}

export default ClaimsGrid
