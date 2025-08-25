import { create } from "zustand";

type State = {
    pageNumber: number;
    pageSize: number;
    pageCount: number;
    searchTerm: string;
    orderBy: string;
    filterBy: string;
}

type Actions = {
    setParams: (params: Partial<State>) => void;
    reset: () => void;
}

const initialState: State = {
    pageNumber: 1,
    pageSize: 12,
    pageCount: 1,
    searchTerm: "",
    orderBy: 'make',
    filterBy: 'live',
}

export const useParamsStore = create<State & Actions>((set) => ({
    ...initialState,

    setParams: (newwParams: Partial<State>) => {
        set((state) => {
            if (newwParams.pageNumber) {
                return {...state, pageNumber: newwParams.pageNumber}
            }
            else {
                return {...state, ...newwParams, pageNumber: 1}
            }
        })
    },

    reset: () => set(initialState)
}))