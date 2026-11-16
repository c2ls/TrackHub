import { render, screen } from '@testing-library/react';
import { TestWrapper } from '../components/testHelpers';
import OperatorFormDialog from 'layouts/gpsintegration/components/operators/OperatorDialog';

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string, options?: { seconds?: number }) => (options?.seconds ? `${key}:${options.seconds}` : key),
  }),
}));

const provider = (protocolTypeId: number, displayName: string, minPositionIntervalSeconds: number) => ({
  protocolTypeId,
  protocol: displayName,
  displayName,
  realTimePositions: true,
  positionHistory: true,
  deviceCatalog: true,
  connectivityPing: true,
  minPositionIntervalSeconds,
  credentialFields: [],
  defaultUri: null,
});

vi.mock('queries/router', () => ({
  useProviderCapabilities: () => ({ data: [provider(2, 'Traccar', 0), provider(16, 'RedGPS', 60)] }),
}));

function renderDialog(protocolTypeId: number) {
  render(
    <TestWrapper>
      <OperatorFormDialog
        open
        setOpen={vi.fn()}
        handleSubmit={vi.fn()}
        values={{ name: 'Operator', protocolTypeId, syncIntervalMinutes: 30 }}
        handleChange={vi.fn()}
        errors={{}}
      />
    </TestWrapper>
  );
}

describe('OperatorFormDialog', () => {
  test('shows the polling limit of the selected provider', () => {
    renderDialog(16);
    expect(screen.getByText('operator.providerPositionLimit:60')).toBeInTheDocument();
  });

  test('shows no limit for a provider that declares none', () => {
    renderDialog(2);
    expect(screen.queryByText(/operator\.providerPositionLimit/)).not.toBeInTheDocument();
  });
});
