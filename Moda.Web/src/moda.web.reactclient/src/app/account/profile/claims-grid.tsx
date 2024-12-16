import { useMemo } from 'react'
import ModaGrid from '../../components/common/moda-grid'
import useAuth, { Claim } from '../../components/contexts/auth'
import { ColDef } from 'ag-grid-community'

const ClaimsGrid = () => {
  const { user } = useAuth()

  const columnDefs = useMemo<ColDef<Claim>[]>(
    () => [{ field: 'type' }, { field: 'value', width: 500 }],
    [],
  )

  return <ModaGrid columnDefs={columnDefs} rowData={user.claims} />
}

export default ClaimsGrid
