'use client'

import PageTitle from "@/src/app/components/common/page-title";
import { useCallback, useMemo, useState } from "react";
import ModaGrid from "../../components/common/moda-grid";
import { RoleListDto } from "@/src/services/moda-api";
import { getRolesClient } from "@/src/services/clients";
import { withAuthorization } from "../../components/hoc";

// TODO: check permissions
const Page = () => {
  const [roles, setRoles] = useState<RoleListDto[]>([])

  const columnDefs = useMemo(() => [
    { field: 'name' },
    { field: 'description' },
  ], []);

  const getRoles = useCallback(async () => {
    const rolesClient = await getRolesClient()
    const roleDtos = await rolesClient.getList()
    setRoles(roleDtos)
  },[])

  return (
    <>
      <PageTitle title="Roles" />

      <ModaGrid columnDefs={columnDefs}
        rowData={roles} loadData={getRoles}/>
    </>
  );
}

const PageWithAuthorization = withAuthorization(Page, "Permission", "Permissions.Roles.View")

export default PageWithAuthorization;