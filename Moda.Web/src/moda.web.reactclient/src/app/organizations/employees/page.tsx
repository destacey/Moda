"use client";

import PageTitle from "@/src/app/components/common/page-title";
import ModaGrid from "../../components/common/moda-grid";
import { useCallback, useMemo, useState } from "react";
import { EmployeeListDto } from "@/src/services/moda-api";
import { getEmployeesClient } from "@/src/services/clients";
import { ItemType } from "antd/es/menu/hooks/useItems";
import { Space, Switch } from "antd";
import Link from "next/link";

const EmployeeLinkCellRenderer = ({ value, data }) => {
    return (
        <Link href={`/organizations/employees/${data.localId}`}>
            {value}
        </Link>
    );
};

const ManagerLinkCellRenderer = ({ value, data }) => {
    return (
        <Link href={`/organizations/employees/${data.managerLocalId}`}>
            {value}
        </Link>
    );
};

const Page = () => {
    const [employees, setEmployees] = useState<EmployeeListDto[]>([]);
    const [includeInactive, setIncludeInactive] = useState<boolean>(false);

    const columnDefs = useMemo(
        () => [
            { field: "localId", headerName: "#", width: 75 },
            { field: "displayName", headerName: "Name", cellRenderer: EmployeeLinkCellRenderer },
            { field: "jobTitle" },
            { field: "department" },
            { field: "managerName", headerName: "Manager", cellRenderer: ManagerLinkCellRenderer },
            { field: "officeLocation" },
            { field: "email" },
            { field: "isActive" }, // TODO: convert to yes/no
        ],
        []
    );

    const onIncludeInactiveChange = (checked: boolean) => {
        setIncludeInactive(checked);
    };

    const controlItems: ItemType[] = [
        {
            label: (
                <Space>
                    <Switch
                        size="small"
                        checked={includeInactive}
                        onChange={onIncludeInactiveChange}
                    />
                    Include Inactive
                </Space>
            ),
            key: "0",
        },
    ];

    const getEmployees = useCallback(async () => {
        const employeesClient = await getEmployeesClient();
        const employeeDtos = await employeesClient.getList(includeInactive);
        setEmployees(employeeDtos);
    }, [includeInactive]);

    return (
        <>
            <PageTitle title="Employees" />
            <ModaGrid
                columnDefs={columnDefs}
                gridControlMenuItems={controlItems}
                rowData={employees}
                loadData={getEmployees}
            />
        </>
    );
};

export default Page;
