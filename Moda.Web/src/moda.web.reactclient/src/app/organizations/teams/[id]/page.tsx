"use client";

import PageTitle from "@/src/app/components/common/page-title";
import { RiskListDto, TeamDetailsDto, TeamMembershipsDto } from "@/src/services/moda-api";
import { Card } from "antd";
import { createElement, useEffect, useState } from "react";
import TeamDetails from "./team-details";
import { getTeamsClient } from "@/src/services/clients";
import RisksGrid from "@/src/app/components/common/planning/risks-grid";
import TeamMembershipsGrid from "@/src/app/components/common/organizations/team-memberships-grid";

const Page = ({ params }) => {
    const [activeTab, setActiveTab] = useState("details");
    const [team, setTeam] = useState<TeamDetailsDto | null>(null);
    const [risks, setRisks] = useState<RiskListDto[]>([]);
    const [teamMemberships, setTeamMemberships] = useState<TeamMembershipsDto[]>([]);
    const { id } = params;

    const tabs = [
        {
            key: "details",
            tab: "Details",
            content: createElement(TeamDetails, team),
        },
        {
            key: "risk-management",
            tab: "Risk Management",
            content: createElement(RisksGrid, { risks: risks, hideTeamColumn: true }),
        },
        {
            key: "team-memberships",
            tab: "Team Memberships",
            content: createElement(TeamMembershipsGrid, { teamMemberships: teamMemberships }),
        },
    ];

    useEffect(() => {
        const getTeam = async () => {
            const teamsClient = await getTeamsClient();
            const teamDto = await teamsClient.getById(id);
            setTeam(teamDto);

            // TODO: move these to an onclick event based on when the user clicks the tab
            // TODO: setup the ability to change whether or not to show risks that are closed
            const riskDtos = await teamsClient.getRisks(teamDto.id, true);
            setRisks(riskDtos);

            const teamMembershipDtos = await teamsClient.getTeamMemberships(teamDto.id);
            setTeamMemberships(teamMembershipDtos);
        };

        getTeam();
    }, [id]);

    return (
        <>
            <PageTitle title={team?.name} subtitle="Team Details" />
            <Card
                style={{ width: "100%" }}
                tabList={tabs}
                activeTabKey={activeTab}
                onTabChange={(key) => setActiveTab(key)}
            >
                {tabs.find((t) => t.key === activeTab)?.content}
            </Card>
        </>
    );
};

export default Page;
