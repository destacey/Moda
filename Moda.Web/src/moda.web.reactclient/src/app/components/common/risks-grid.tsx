import Link from "next/link";
import { useMemo, useState } from "react";
import ModaGrid from "./moda-grid";
import { RiskListDto } from "@/src/services/moda-api";

// props
export interface RisksGridProps {
    risks: RiskListDto[];
}

const TeamLinkCellRenderer = ({ value, data }) => {
    const teamRoute = data.team?.type === "Team" ? "teams" : "team-of-teams";
    return (
        <Link href={`/organizations/${teamRoute}/${data.team?.localId}`}>
            {value}
        </Link>
    );
};

const AssigneeLinkCellRenderer = ({ value, data }) => {
    return (
        <Link href={`/organizations/employees/${data.assignee?.localId}`}>
            {value}
        </Link>
    );
};

const RisksGrid = ({ risks }: RisksGridProps) => {

    // TODO: dates are formatted correctly and filter, but the filter is string based, not date based
    const columnDefs = useMemo(
        () => [
            { field: "localId", headerName: "#", width: 90 },
            { field: "summary", width: 300 },
            { field: "team.name", cellRenderer: TeamLinkCellRenderer },
            { field: "category", width: 125 },
            { field: "exposure", width: 125 },
            { field: "followUp", valueGetter: (params) => params.data.followUpDate ? new Date(params.data.followUpDate).toLocaleDateString() : null }, 
            { field: "assignee.name", cellRenderer: AssigneeLinkCellRenderer },
            { field: "reportedOn", valueGetter: (params) => new Date(params.data.reportedOn).toLocaleDateString()},
        ],
        []
    );

    return (
        <>
            <ModaGrid
                columnDefs={columnDefs}
                rowData={risks}
            />
        </>
    );
};

export default RisksGrid;
