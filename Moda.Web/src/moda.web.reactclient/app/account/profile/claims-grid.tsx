import { useMsal } from "@azure/msal-react"
import { useEffect, useState } from "react";
import jwt_decode from "jwt-decode";
import { acquireToken } from "@/app/services/auth";
import ModaGrid from "@/app/components/common/moda-grid";

interface Claim {
  key: string;
  value: string;
}

const columnDefs = [
  { field: 'key', },
  { field: 'value', width: 500 }
]

const ClaimsGrid = () => {
  const { instance } = useMsal()
  const [rowData, setRowData] = useState<Claim[]>([])

  useEffect(() => {
    const getTokenClaims = async () => {
      const token = await acquireToken()
      const decodedClaims = jwt_decode(token ?? '') as { [key: string]: string }
      const claims = Object.keys(decodedClaims).map(key => {
        return {
          key: key,
          value: decodedClaims[key]
        }
      })
      setRowData(claims)
    }

    getTokenClaims()
  }, [instance])

  return (
    <ModaGrid columnDefs={columnDefs}
      rowData={rowData}
      width={700} />
  )
}

export default ClaimsGrid