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

import React, { useContext } from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import { LoadingContext } from 'LoadingContext';

function TestConsumer() {
  const { loading, setLoading } = useContext(LoadingContext);
  return (
    <div>
      <span data-testid="loading-state">{loading ? 'loading' : 'idle'}</span>
      <button onClick={() => setLoading(true)}>Start Loading</button>
      <button onClick={() => setLoading(false)}>Stop Loading</button>
    </div>
  );
}

describe('LoadingContext', () => {
  test('provides default loading state as false', () => {
    render(
      <LoadingContext.Provider value={{ loading: false, setLoading: vi.fn() }}>
        <TestConsumer />
      </LoadingContext.Provider>
    );
    expect(screen.getByTestId('loading-state')).toHaveTextContent('idle');
  });

  test('provides loading state as true when set', () => {
    render(
      <LoadingContext.Provider value={{ loading: true, setLoading: vi.fn() }}>
        <TestConsumer />
      </LoadingContext.Provider>
    );
    expect(screen.getByTestId('loading-state')).toHaveTextContent('loading');
  });

  test('setLoading can be called to update state', () => {
    const mockSetLoading = vi.fn();
    render(
      <LoadingContext.Provider value={{ loading: false, setLoading: mockSetLoading }}>
        <TestConsumer />
      </LoadingContext.Provider>
    );

    fireEvent.click(screen.getByText('Start Loading'));
    expect(mockSetLoading).toHaveBeenCalledWith(true);

    fireEvent.click(screen.getByText('Stop Loading'));
    expect(mockSetLoading).toHaveBeenCalledWith(false);
  });

  test('default context values work without provider', () => {
    render(<TestConsumer />);
    expect(screen.getByTestId('loading-state')).toHaveTextContent('idle');
  });

  test('setLoading default is a no-op function', () => {
    // Using default context - setLoading should be a no-op and not throw
    render(<TestConsumer />);
    expect(() => {
      fireEvent.click(screen.getByText('Start Loading'));
    }).not.toThrow();
  });
});
