"use client";

import PageTitle from "@/src/app/components/common/page-title";
import { getProgramIncrementsClient } from "@/src/services/clients";
import { ProgramIncrementDetailsDto } from "@/src/services/moda-api";
import { Card } from "antd";
import { createElement, useEffect, useState } from "react";
import ProgramIncrementDetails from "./program-increment-details";

const Page = ({ params }) => {
    const [activeTab, setActiveTab] = useState("details");
    const [programIncrement, setProgramIncrement] =
        useState<ProgramIncrementDetailsDto | null>(null);

    const tabs = [
        {
            key: "details",
            tab: "Details",
            content: createElement(ProgramIncrementDetails, programIncrement),
        },
    ];

    useEffect(() => {
        const getProgramIncrement = async () => {
            const programIncrementsClient = await getProgramIncrementsClient();
            const programIncrementDto =
                await programIncrementsClient.getByLocalId(params.id);
            setProgramIncrement(programIncrementDto);
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
