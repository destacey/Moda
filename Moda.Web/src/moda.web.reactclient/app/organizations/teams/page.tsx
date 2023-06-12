'use client'

import { Metadata } from "next";
import PageTitle from "@/app/components/common/page-title";

export const metadata: Metadata = {
  title: 'Teams',
}

const Page = () => {
  return (
    <>
      <PageTitle title="Teams"/>
    </>
  );
}

export default Page;