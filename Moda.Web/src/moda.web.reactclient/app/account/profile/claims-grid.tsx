import { useMsal } from "@azure/msal-react"
import { AgGridReact } from "ag-grid-react";
import { useEffect, useState } from "react";
import jwt_decode from "jwt-decode";

import 'ag-grid-community/styles/ag-grid.css';
import 'ag-grid-community/styles/ag-theme-balham.css';
import { tokenRequest } from "@/authConfig";
import { ProfileClient } from "@/app/services/moda-api";
import axios from "axios";
import { acquireToken } from "@/app/services/auth";

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
    <div className="ag-theme-balham" style={{ height: 600, width: 700 }}>
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