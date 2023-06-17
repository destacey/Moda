'use client'

import { Skeleton } from "antd";
import PageTitle from "../../components/common/page-title";

export default function Loading() {
    return (
        <>
            <PageTitle title="Account" />
            <Skeleton />
        </>
    ) 
  }