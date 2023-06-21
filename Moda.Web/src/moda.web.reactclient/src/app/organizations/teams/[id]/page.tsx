'use client'

import PageTitle from "@/src/app/components/common/page-title";
import { TeamDetailsDto } from "@/src/services/moda-api";
import { Card } from "antd";
import { createElement, useEffect, useState } from "react";
import TeamDetails from "./team-details";
import { getTeamsClient } from "@/src/services/clients";

const Page = ({ params }) => {
    const [activeTab, setActiveTab] = useState("details");
    const [team, setTeam] = useState<TeamDetailsDto | null>(null)
    const { id } = params;

    const tabs = [
        {
            key: "details",
            tab: "Details",
            content: createElement(TeamDetails, team),
        },
    ];

    useEffect(() => {
        const getTeam = async () => {
            const teamsClient = await getTeamsClient();
            const teamDto =
                await teamsClient.getById(id);
                setTeam(teamDto);
        };

        getTeam();
    }, [id]);

    return (
        <>
            <PageTitle
                title={team?.name}
                subtitle="Team Details"
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