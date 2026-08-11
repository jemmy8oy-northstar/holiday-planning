import { createContext, useContext } from 'react';

export type Theme = 'light' | 'dark';

export interface ThemeContextType {
    theme: Theme;
    toggleTheme: () => void;
}

// The context object and its hook live here (not in ThemeContext.tsx) so that the
// component file exports *only* the ThemeProvider component. React Fast Refresh
// (react-refresh/only-export-components) requires a module to export components
// exclusively for HMR to work reliably.
export const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

export const useTheme = (): ThemeContextType => {
    const context = useContext(ThemeContext);
    if (context === undefined) {
        throw new Error('useTheme must be used within a ThemeProvider');
    }
    return context;
};
