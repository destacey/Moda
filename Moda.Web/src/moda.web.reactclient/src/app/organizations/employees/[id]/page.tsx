"use client";

import PageTitle from "@/src/app/components/common/page-title";
import { EmployeeDetailsDto } from "@/src/services/moda-api";
import { createElement, useEffect, useState } from "react";
import EmployeeDetails from "./employee-details";
import { getEmployeesClient } from "@/src/services/clients";
import { Card } from "antd";
import { useDocumentTitle } from "@/src/app/hooks/use-document-title";

const EmployeeDetailsPage = ({ params }) => {
    useDocumentTitle('Employee Details')
    const [activeTab, setActiveTab] = useState("details");
    const [employee, setEmployee] = useState<EmployeeDetailsDto | null>(null);
    const { id } = params;

    const tabs = [
        {
            key: "details",
            tab: "Details",
            content: createElement(EmployeeDetails, employee),
        },
    ];

    useEffect(() => {
        const getEmployee = async () => {
            const employeesClient = await getEmployeesClient();
            const employeeDto = await employeesClient.getById(id);
            setEmployee(employeeDto);
        };

        getEmployee();
    }, [id]);

    return (
        <>
            <PageTitle
                title={employee?.displayName}
                subtitle="Employee Details"
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

export default EmployeeDetailsPage;
