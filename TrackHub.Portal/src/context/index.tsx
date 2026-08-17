/**
* Copyright (c) 2025 Sergio Hernandez. All rights reserved.
*
*  Licensed under the Apache License, Version 2.0 (the "License").
*  You may not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" BASIS,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*  See the License for the specific language governing permissions and
*  limitations under the License.
*/

/**
=========================================================
* Argon Dashboard 2 MUI - v3.0.1
=========================================================

* Product Page: https://www.creative-tim.com/product/argon-dashboard-material-ui
* Copyright 2023 Creative Tim (https://www.creative-tim.com)

Coded by www.creative-tim.com

 =========================================================

* The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
*/

/**
  This file is used for controlling the global states of the components,
  you can customize the states for the different components here.
*/

import { createContext, useContext, useReducer, useMemo } from "react";
import type { Dispatch, ReactNode } from "react";

// Global UI state controlled by the Argon dashboard shell.
export interface ArgonControllerState {
  miniSidenav: boolean;
  darkSidenav: boolean;
  transparentNavbar: boolean;
  openConfigurator: boolean;
  layout: string;
  darkMode: boolean;
}

// Reducer actions — one per settable field.
export type ArgonControllerAction =
  | { type: "MINI_SIDENAV"; value: boolean }
  | { type: "DARK_SIDENAV"; value: boolean }
  | { type: "TRANSPARENT_NAVBAR"; value: boolean }
  | { type: "OPEN_CONFIGURATOR"; value: boolean }
  | { type: "LAYOUT"; value: string }
  | { type: "DARK_MODE"; value: boolean };

export type ArgonControllerDispatch = Dispatch<ArgonControllerAction>;

// Tuple exposed by the context / useArgonController hook.
export type ArgonControllerValue = [ArgonControllerState, ArgonControllerDispatch];

// The Argon Dashboard 2 MUI main context
const Argon = createContext<ArgonControllerValue | null>(null);

// Setting custom name for the context which is visible on react dev tools
Argon.displayName = "ArgonContext";

// Argon Dashboard 2 MUI reducer
function reducer(state: ArgonControllerState, action: ArgonControllerAction): ArgonControllerState {
  switch (action.type) {
    case "MINI_SIDENAV": {
      return { ...state, miniSidenav: action.value };
    }
    case "DARK_SIDENAV": {
      return { ...state, darkSidenav: action.value };
    }
    case "TRANSPARENT_NAVBAR": {
      return { ...state, transparentNavbar: action.value };
    }
    case "OPEN_CONFIGURATOR": {
      return { ...state, openConfigurator: action.value };
    }
    case "LAYOUT": {
      return { ...state, layout: action.value };
    }
    case "DARK_MODE": {
      return { ...state, darkMode: action.value };
    }
    default: {
      throw new Error(`Unhandled action type: ${(action as { type?: string }).type}`);
    }
  }
}

interface ArgonControllerProviderProps {
  children: ReactNode;
}

// Argon Dashboard 2 MUI context provider
function ArgonControllerProvider({ children }: ArgonControllerProviderProps) {
  const initialState: ArgonControllerState = {
    miniSidenav: false,
    darkSidenav: false,
    transparentNavbar: true,
    openConfigurator: false,
    layout: "dashboard",
    darkMode: false,
  };

  const [controller, dispatch] = useReducer(reducer, initialState);

  const value = useMemo<ArgonControllerValue>(
    () => [controller, dispatch],
    [controller, dispatch]
  );

  return <Argon.Provider value={value}>{children}</Argon.Provider>;
}

// Argon Dashboard 2 MUI custom hook for using context
function useArgonController(): ArgonControllerValue {
  const context = useContext(Argon);

  if (!context) {
    throw new Error("useArgonController should be used inside the ArgonControllerProvider.");
  }

  return context;
}

// Context module functions
const setMiniSidenav = (dispatch: ArgonControllerDispatch, value: boolean) =>
  dispatch({ type: "MINI_SIDENAV", value });
const setDarkSidenav = (dispatch: ArgonControllerDispatch, value: boolean) =>
  dispatch({ type: "DARK_SIDENAV", value });
const setTransparentNavbar = (dispatch: ArgonControllerDispatch, value: boolean) =>
  dispatch({ type: "TRANSPARENT_NAVBAR", value });
const setOpenConfigurator = (dispatch: ArgonControllerDispatch, value: boolean) =>
  dispatch({ type: "OPEN_CONFIGURATOR", value });
const setLayout = (dispatch: ArgonControllerDispatch, value: string) =>
  dispatch({ type: "LAYOUT", value });
const setDarkMode = (dispatch: ArgonControllerDispatch, value: boolean) =>
  dispatch({ type: "DARK_MODE", value });

export {
  ArgonControllerProvider,
  useArgonController,
  setMiniSidenav,
  setDarkSidenav,
  setTransparentNavbar,
  setOpenConfigurator,
  setLayout,
  setDarkMode,
};
