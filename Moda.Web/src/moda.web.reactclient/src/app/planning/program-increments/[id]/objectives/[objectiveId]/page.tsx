"use client";

import PageTitle from "@/src/app/components/common/page-title";
import { getProgramIncrementsClient } from "@/src/services/clients";
import { ProgramIncrementObjectiveDetailsDto } from "@/src/services/moda-api";
import { Card } from "antd";
import { createElement, useEffect, useState } from "react";
import ProgramIncrementObjectiveDetails from "./program-increment-objective-details";
import { useDocumentTitle } from "@/src/app/hooks/use-document-title";

const ObjectiveDetailsPage = ({ params }) => {
    useDocumentTitle("PI Objective Details");
    const [activeTab, setActiveTab] = useState("details");
    const [objective, setObjective] =
        useState<ProgramIncrementObjectiveDetailsDto | null>(null);

    const tabs = [
        {
            key: "details",
            tab: "Details",
            content: createElement(ProgramIncrementObjectiveDetails, objective),
        },
    ];

    useEffect(() => {
        const getObjective = async () => {
            const programIncrementsClient = await getProgramIncrementsClient();
            const objectiveDto =
                await programIncrementsClient.getObjectiveByLocalId(params.id, params.objectiveId);
            setObjective(objectiveDto);
        };

        getObjective();
    }, [params.id, params.objectiveId]);

    return (
        <>
            <PageTitle
                title={objective?.name}
                subtitle="PI Objective Details"
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

export default ObjectiveDetailsPage;
