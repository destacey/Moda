'use client'

import PageTitle from "@/src/app/components/common/page-title";
import { TeamOfTeamsDetailsDto } from "@/src/services/moda-api";
import { Card } from "antd";
import { createElement, useEffect, useState } from "react";
import TeamOfTeamsDetails from "./team-of-teams-details";
import { getTeamsOfTeamsClient } from "@/src/services/clients";

const Page = ({ params }) => {
    const [activeTab, setActiveTab] = useState("details");
    const [team, setTeam] = useState<TeamOfTeamsDetailsDto | null>(null)
    const { id } = params;

    const tabs = [
        {
            key: "details",
            tab: "Details",
            content: createElement(TeamOfTeamsDetails, team),
        },
    ];

    useEffect(() => {
        const getTeam = async () => {
            const teamsOfTeamsClient = await getTeamsOfTeamsClient();
            const teamDto =
                await teamsOfTeamsClient.getById(id);
                setTeam(teamDto);
        };

        getTeam();
    }, [id]);

    return (
        <>
            <PageTitle
                title={team?.name}
                subtitle="Team of Teams Details"
            />
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