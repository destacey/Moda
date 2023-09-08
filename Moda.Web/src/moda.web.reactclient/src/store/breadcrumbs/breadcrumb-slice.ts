import { PayloadAction, createSlice } from "@reduxjs/toolkit";
import { generateRoute } from "./routes";
import { BreadcrumbItem, BreadcrumbState } from "./types";

const initialState: BreadcrumbState = {
    items: [],
    isVisible: false,
    forPath: '',
}

const pageSlice = createSlice({
  name: 'page',
  initialState,
  reducers: {
    disableBreadcrumb(state, action: PayloadAction<string>) {
      state.forPath = action.payload;
      state.items = [];
      state.isVisible = false;
    },
    setBreadcrumbTitle(state, action: PayloadAction<{title: string, pathname: string}>) {
      state.forPath = action.payload.pathname;
      state.isVisible = true;
      state.items = generateRoute(action.payload.pathname, action.payload.title);
    },
    setBreadcrumbRoute(state, action: PayloadAction<{route: BreadcrumbItem[], pathname: string}>) {
      state.forPath = action.payload.pathname;
      state.items = [...action.payload.route];
      state.isVisible = true;
    }
  }
})

export const { setBreadcrumbTitle, setBreadcrumbRoute, disableBreadcrumb } = pageSlice.actions;

export const selectBreadcrumb = (state: { breadcrumb: BreadcrumbState }) => state.breadcrumb;

export default pageSlice.reducer;