import { useMsal } from "@azure/msal-react"
import { AgGridReact } from "ag-grid-react";
import { useMemo, useState } from "react";

import 'ag-grid-community/styles/ag-grid.css';
import 'ag-grid-community/styles/ag-theme-balham.css';

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

  const { accounts } = useMsal();
  const accountClaims = accounts[0].idTokenClaims as any;
  const claims = Object.keys(accountClaims).map(key => {
    return {
      key: key,
      value: accountClaims[key]
    }
  });

  const [rowData, setRowData] = useState(claims);

  const [columnDefs, setColumnDefs] = useState([
    { field: 'key', },
    { field: 'value', width: 500 }
  ]);

  const defaultColDef = useMemo(() => ({
    sortable: true,
    filter: true,
    resizable: true,
    
  }), []);

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