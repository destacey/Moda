import { useSession } from "next-auth/react";
import { ExtendedNextPage } from "../../types/extended-next-types";

const EmployeesList: ExtendedNextPage = () => {
  const {data:session} = useSession()
  return (
    <>
      Hello {session?.user?.email}!
    </>
  )
}

EmployeesList.auth = true;

export default EmployeesList;