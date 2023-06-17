'use client'

import PageTitle from "@/app/components/common/page-title";
import { Skeleton } from "antd";

export default function Loading() {
    return (
        <>
            <PageTitle title="Users" />
            <Skeleton />
        </>
    ) 
  }