'use client'

import PageTitle from "@/src/app/components/common/page-title";
import { getRisksClient } from "@/src/services/clients";
import { RiskDetailsDto } from "@/src/services/moda-api";
import { useEffect, useState } from "react";
import RiskDetails from "./risk-details";
import React from "react";
import { Card } from "antd";

const Page = ({ params }) => {
    const [activeTab, setActiveTab] = React.useState('details')
    const [risk, setRisk] = useState<RiskDetailsDto | null>(null)

    const tabs = [
        { key: 'details', tab: 'Details', content: React.createElement(RiskDetails, risk) },
    ]

    useEffect(() => {
        const getRisk = async () => {
            const risksClient = await getRisksClient()
            const riskDto = await risksClient.getByLocalId(params.id)
            setRisk(riskDto) // TODO: add sorting: by state: active, future, completed, then by start date
        }

        getRisk()
    }, [])

    return (
        <>
            <PageTitle title={risk?.summary} subtitle="Risk Details" />
            <Card style={{ width: '100%' }}
                tabList={tabs}
                activeTabKey={activeTab}
                onTabChange={key => setActiveTab(key)}
            >
                {tabs.find(t => t.key === activeTab)?.content}
            </Card>
        </>
    );
}

export default Page;