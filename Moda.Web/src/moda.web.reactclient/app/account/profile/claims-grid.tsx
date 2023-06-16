import { useMsal } from "@azure/msal-react"
import { AgGridReact } from "ag-grid-react";
import { useContext, useEffect, useState } from "react";
import jwt_decode from "jwt-decode";
import { acquireToken } from "@/app/services/auth";
import { ThemeContext } from "@/app/components/contexts/theme-context";

import 'ag-grid-community/styles/ag-grid.css';
import 'ag-grid-community/styles/ag-theme-balham.css';

interface Claim {
  key: string;
  value: string;
}

const columnDefs = [
  { field: 'key', },
  { field: 'value', width: 500 }
]

const defaultColDef = {
  sortable: true,
  filter: true,
  resizable: true,
}

const ClaimsGrid = () => {
  const [currentThemeName, setCurrentThemeName, appBarColor, agGridTheme] = useContext(ThemeContext)
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
    <div className={agGridTheme} style={{ height: 600, width: 700 }}>
        <AgGridReact
          rowData={rowData}
          columnDefs={columnDefs}
          defaultColDef={defaultColDef}
          animateRows={true}>
        </AgGridReact>
      </div>
  )
}

export default ClaimsGrid