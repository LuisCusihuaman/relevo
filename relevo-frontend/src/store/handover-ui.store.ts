import { create } from 'zustand';
import type { ExpandedSections, FullscreenEditingState, SyncStatus } from '@/common/types';

interface HandoverUIState {
  layoutMode: 'single' | 'columns';
  expandedSections: ExpandedSections;
  syncStatus: SyncStatus;
  showHistory: boolean;
  showComments: boolean;
  showCollaborators: boolean;
  showMobileMenu: boolean;
  fullscreenEditing: FullscreenEditingState | null;
  currentSaveFunction: (() => void) | null;
  
  // Actions
  setLayoutMode: (mode: 'single' | 'columns') => void;
  setExpandedSections: (sections: ExpandedSections | ((prev: ExpandedSections) => ExpandedSections)) => void;
  toggleSection: (section: keyof ExpandedSections) => void;
  setSyncStatus: (status: SyncStatus) => void;
  setShowHistory: (show: boolean) => void;
  setShowComments: (show: boolean) => void;
  setShowCollaborators: (show: boolean) => void;
  setShowMobileMenu: (show: boolean) => void;
  setFullscreenEditing: (state: FullscreenEditingState | null) => void;
  setCurrentSaveFunction: (fn: (() => void) | null) => void;
  reset: () => void;
}

const defaultExpandedSections: ExpandedSections = {
  illness: true,
  patient: false,
  actions: false,
  awareness: false,
  synthesis: false,
};

export const useHandoverUIStore = create<HandoverUIState>((set) => ({
  layoutMode: 'columns',
  expandedSections: defaultExpandedSections,
  syncStatus: 'synced',
  showHistory: false,
  showComments: false,
  showCollaborators: false,
  showMobileMenu: false,
  fullscreenEditing: null,
  currentSaveFunction: null,

  setLayoutMode: (mode) => set({ layoutMode: mode }),
  setExpandedSections: (sections) => set((state) => ({
    expandedSections: typeof sections === 'function' ? sections(state.expandedSections) : sections
  })),
  toggleSection: (section) => set((state) => ({
    expandedSections: { ...state.expandedSections, [section]: !state.expandedSections[section] }
  })),
  setSyncStatus: (status) => set({ syncStatus: status }),
  setShowHistory: (show) => set({ showHistory: show }),
  setShowComments: (show) => set({ showComments: show }),
  setShowCollaborators: (show) => set({ showCollaborators: show }),
  setShowMobileMenu: (show) => set({ showMobileMenu: show }),
  setFullscreenEditing: (state) => set({ fullscreenEditing: state }),
  setCurrentSaveFunction: (fn) => set({ currentSaveFunction: fn }),
  
  reset: () => set({
    layoutMode: 'columns',
    expandedSections: defaultExpandedSections,
    syncStatus: 'synced',
    showHistory: false,
    showComments: false,
    showCollaborators: false,
    showMobileMenu: false,
    fullscreenEditing: null,
    currentSaveFunction: null,
  })
}));
