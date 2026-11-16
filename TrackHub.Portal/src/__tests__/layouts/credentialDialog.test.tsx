import { render, screen } from '@testing-library/react';
import { TestWrapper } from '../components/testHelpers';
import CredentialFormDialog from 'layouts/gpsintegration/components/operators/CredentialDialog';
import type { CredentialFormValues } from 'layouts/gpsintegration/data/operatorsTableData';

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

const provider = (protocolTypeId: number, protocol: string, fields: string[], defaultUri: string | null = null) => ({
  protocolTypeId,
  protocol,
  displayName: protocol,
  realTimePositions: true,
  positionHistory: true,
  deviceCatalog: true,
  connectivityPing: true,
  minPositionIntervalSeconds: 0,
  credentialFields: fields,
  defaultUri,
});

vi.mock('queries/router', () => ({
  useProviderCapabilities: () => ({
    data: [
      provider(12, 'Motive', ['KEY'], 'https://api.gomotive.com'),
      provider(13, 'Cartrack', ['USERNAME', 'PASSWORD']),
      provider(2, 'Traccar', ['USERNAME', 'PASSWORD']),
    ],
  }),
}));

function renderDialog(values: CredentialFormValues) {
  const handleChange = vi.fn();
  render(
    <TestWrapper>
      <CredentialFormDialog open setOpen={vi.fn()} handleSubmit={vi.fn()} values={values} handleChange={handleChange} errors={{}} />
    </TestWrapper>
  );
  return handleChange;
}

describe('CredentialFormDialog', () => {
  test('asks only for the fields the provider uses, under the provider’s own names', () => {
    renderDialog({ operatorId: 'op', protocolTypeId: 12 });

    expect(screen.getByText('credential.providers.Motive.key')).toBeInTheDocument();
    expect(screen.queryByText('credential.username')).not.toBeInTheDocument();
    expect(screen.queryByText('credential.password')).not.toBeInTheDocument();
    expect(screen.queryByText('credential.key2')).not.toBeInTheDocument();
  });

  test('fills in the provider’s endpoint when the credential has no URL yet', () => {
    const handleChange = renderDialog({ operatorId: 'op', protocolTypeId: 12 });

    expect(handleChange).toHaveBeenCalledWith({ target: { name: 'uri', value: 'https://api.gomotive.com' } });
  });

  test('keeps a stored URL', () => {
    const handleChange = renderDialog({ operatorId: 'op', protocolTypeId: 12, uri: 'https://custom.example' });

    expect(handleChange).not.toHaveBeenCalled();
  });

  test('explains a per-customer URL and falls back to generic names', () => {
    renderDialog({ operatorId: 'op', protocolTypeId: 13 });

    expect(screen.getByText('credential.providers.Cartrack.uriHelp')).toBeInTheDocument();
    expect(screen.getByText('credential.providers.Cartrack.password')).toBeInTheDocument();
    expect(screen.getByText('credential.username')).toBeInTheDocument();
  });

  test('shows every field when the provider is unknown', () => {
    renderDialog({ operatorId: 'op' });

    for (const key of ['credential.username', 'credential.password', 'credential.key', 'credential.key2']) {
      expect(screen.getByText(key)).toBeInTheDocument();
    }
  });
});
