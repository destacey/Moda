"use client";

import PageTitle from "@/src/app/components/common/page-title";
import { getProgramIncrementsClient } from "@/src/services/clients";
import { ProgramIncrementDetailsDto, ProgramIncrementObjectiveListDto } from "@/src/services/moda-api";
import { Card } from "antd";
import { createElement, useEffect, useState } from "react";
import ProgramIncrementDetails from "./program-increment-details";
import ProgramIncrementObjectivesGrid from "@/src/app/components/common/program-increment-objectives-grid";

const Page = ({ params }) => {
    const [activeTab, setActiveTab] = useState("details");
    const [programIncrement, setProgramIncrement] =
        useState<ProgramIncrementDetailsDto | null>(null);
    const [objectives, setObjectives] = useState<ProgramIncrementObjectiveListDto[]>([]);

    const tabs = [
        {
            key: "details",
            tab: "Details",
            content: createElement(ProgramIncrementDetails, programIncrement),
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
    ];

    useEffect(() => {
        const getProgramIncrement = async () => {
            const programIncrementsClient = await getProgramIncrementsClient();
            const programIncrementDto = await programIncrementsClient.getByLocalId(params.id);
            setProgramIncrement(programIncrementDto);

            if (!programIncrementDto) return;

            // TODO: move this to an onclick event based on when the user clicks the tab
            const objectiveDtos = await programIncrementsClient.getObjectives(programIncrementDto.id, null);
            setObjectives(objectiveDtos);
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

export default Page;
