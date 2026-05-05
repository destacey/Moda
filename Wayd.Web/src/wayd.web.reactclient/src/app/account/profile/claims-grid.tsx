import { useMemo } from 'react'
import WaydGrid from '../../../components/common/wayd-grid'
import useAuth, { Claim } from '../../../components/contexts/auth'
import { ColDef } from 'ag-grid-community'

const ClaimsGrid = () => {
  const { user } = useAuth()

  const columnDefs = useMemo<ColDef<Claim>[]>(
    () => [{ field: 'type' }, { field: 'value', width: 500 }],
    [],
  )

  return <WaydGrid columnDefs={columnDefs} rowData={user?.claims} />
}

export default ClaimsGrid
