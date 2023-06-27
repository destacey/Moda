"use client";

import PageTitle from "@/src/app/components/common/page-title";
import { getProgramIncrementsClient } from "@/src/services/clients";
import { ProgramIncrementDetailsDto, ProgramIncrementObjectiveListDto, RiskListDto } from "@/src/services/moda-api";
import { Card } from "antd";
import { createElement, useEffect, useState } from "react";
import ProgramIncrementDetails from "./program-increment-details";
import ProgramIncrementObjectivesGrid from "@/src/app/components/common/planning/program-increment-objectives-grid";
import { TeamListItem } from "@/src/app/organizations/types";
import TeamsGrid from "@/src/app/components/common/organizations/teams-grid";
import RisksGrid from "@/src/app/components/common/planning/risks-grid";
import { useDocumentTitle } from "@/src/app/hooks/use-document-title";

const ProgramIncrementDetailsPage = ({ params }) => {
    useDocumentTitle("PI Details");
    const [activeTab, setActiveTab] = useState("details");
    const [programIncrement, setProgramIncrement] =
        useState<ProgramIncrementDetailsDto | null>(null);
    const [teams, setTeams] = useState<TeamListItem[]>([]);
    const [objectives, setObjectives] = useState<ProgramIncrementObjectiveListDto[]>([]);
    const [risks, setRisks] = useState<RiskListDto[]>([]);

    const tabs = [
        {
            key: "details",
            tab: "Details",
            content: createElement(ProgramIncrementDetails, programIncrement),
        },
        {
            key: "teams",
            tab: "Teams",
            content: createElement(TeamsGrid, {teams: teams}),
        },
        {
            key: "objectives",
            tab: "Objectives",
            content: createElement(ProgramIncrementObjectivesGrid,
                {
                    objectives: objectives,
                    hideProgramIncrementColumn: true,
                    hideTeamColumn: false
                }),
        },
        {
            key: "risk-management",
            tab: "Risk Management",
            content: createElement(RisksGrid, {risks: risks}),
        },
    ];

    useEffect(() => {
        const getProgramIncrement = async () => {
            const programIncrementsClient = await getProgramIncrementsClient();
            const programIncrementDto = await programIncrementsClient.getByLocalId(params.id);
            setProgramIncrement(programIncrementDto);

            if (!programIncrementDto) return;

            // TODO: move these to an onclick event based on when the user clicks the tab
            const teamDtos = await programIncrementsClient.getTeams(programIncrementDto.id);
            setTeams(teamDtos as TeamListItem[]);

            const objectiveDtos = await programIncrementsClient.getObjectives(programIncrementDto.id, null);
            setObjectives(objectiveDtos);

            // TODO: setup the ability to change whether or not to show risks that are closed
            const riskDtos = await programIncrementsClient.getRisks(programIncrementDto.id, true);
            setRisks(riskDtos);
        };

        getProgramIncrement();
    }, [params.id]);

    return (
        <>
            <PageTitle
                title={programIncrement?.name}
                subtitle="Program Increment Details"
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

export default ProgramIncrementDetailsPage;
