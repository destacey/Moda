import Link from "next/link";
import { useMemo, useState } from "react";
import ModaGrid from "../moda-grid";
import { TeamListItem } from "../../../organizations/types";

export interface TeamsGridProps {
    teams: TeamListItem[];
}

const TeamLinkCellRenderer = ({ value, data }) => {
    const teamRoute = data.type === "Team" ? "teams" : "team-of-teams";
    return (
        <Link href={`/organizations/${teamRoute}/${data.localId}`}>
            {value}
        </Link>
    );
};

const TeamOfTeamsLinkCellRenderer = ({ value, data }) => {
    return (
        <Link
            href={`/organizations/team-of-teams/${data.teamOfTeams?.localId}`}
        >
            {value}
        </Link>
    );
};

const TeamsGrid = ({ teams }: TeamsGridProps) => {

    const columnDefs = useMemo(
        () => [
            { field: "localId", headerName: "#", width: 90 },
            { field: "name", cellRenderer: TeamLinkCellRenderer },
            { field: "code", width: 125 },
            { field: "type" },
            {
                field: "teamOfTeams.name",
                headerName: "Team of Teams",
                cellRenderer: TeamOfTeamsLinkCellRenderer,
            },
            { field: "isActive" }, // TODO: convert to yes/no
        ],
        []
    );

    return (
        <>
            <ModaGrid
                columnDefs={columnDefs}
                rowData={teams}
            />
        </>
    );
};

export default TeamsGrid;
