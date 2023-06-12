'use client'

import { Metadata } from "next";
import PageTitle from "./components/common/page-title";

export const metadata: Metadata = {
  title: 'Home',
}

const Page = () => {
  return (
    <>
      <PageTitle title="Home"/>
    </>
  );
}

export default Page;