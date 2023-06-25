import Link from "next/link";
import { useMemo, useState } from "react";
import ModaGrid from "../moda-grid";
import { RiskListDto } from "@/src/services/moda-api";
import { ItemType } from "antd/es/menu/hooks/useItems";
import { Space, Switch } from "antd";

export interface RisksGridProps {
    risks: RiskListDto[];
    hideTeamColumn?: boolean;
}

const RiskLinkCellRenderer = ({ value, data }) => {
    return (
        <Link href={`/planning/risks/${data.localId}`}>
            {value}
        </Link>
    );
};

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

const RisksGrid = ({ risks, hideTeamColumn = false }: RisksGridProps) => {
    const [hideTeam, setHideTeam] = useState<boolean>(hideTeamColumn);

    const onHideTeamChange = (checked: boolean) => {
        setHideTeam(checked);
    };

    const controlItems: ItemType[] = [
        {
            label: (
                <>
                    <Space direction="vertical" size="small">
                        <Space>
                            <Switch
                                size="small"
                                checked={hideTeam}
                                onChange={onHideTeamChange}
                            />
                            Hide Team
                        </Space>
                    </Space>
                </>
            ),
            key: "0",
        },
    ];

    // TODO: dates are formatted correctly and filter, but the filter is string based, not date based
    const columnDefs = useMemo(
        () => [
            { field: "localId", headerName: "#", width: 90 },
            { field: "summary", width: 300, cellRenderer: RiskLinkCellRenderer },
            { field: "team.name", cellRenderer: TeamLinkCellRenderer, hide: hideTeam },
            { field: "category", width: 125 },
            { field: "exposure", width: 125 },
            { field: "followUp", valueGetter: (params) => params.data.followUpDate ? new Date(params.data.followUpDate).toLocaleDateString() : null }, 
            { field: "assignee.name", cellRenderer: AssigneeLinkCellRenderer },
            { field: "reportedOn", valueGetter: (params) => new Date(params.data.reportedOn).toLocaleDateString()},
        ],
        [hideTeam]
    );

    return (
        <>
            <ModaGrid
                columnDefs={columnDefs}
                rowData={risks}
                gridControlMenuItems={controlItems}
            />
        </>
    );
};

export default RisksGrid;
