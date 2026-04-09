// Copyright (c) 2025 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using TrackHubMobile.Interfaces.Helpers;
using TrackHubMobile.Interfaces.Services;
using TrackHubMobile.Messages;
using TrackHubMobile.Models;

namespace TrackHubMobile.Services;

public class DataRefresh(IRouter router, ILocalizationResourceManager localization) : IAsyncDisposable, IDataRefresh
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan RestartDelay = TimeSpan.FromMilliseconds(500);

    private Timer? _timer;
    private bool _isActiveScreen;
    private bool _isAppActive = true;
    private int _isRefreshing;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly object _timerLock = new();

    public IEnumerable<PositionVm> Transporters { get; private set; } = [];

    public void SetScreenActive(bool isActive)
    {
        _isActiveScreen = isActive;
        CheckTimerStatus();
    }

    public async Task SetAppActive(bool isActive, bool forceRefresh = false)
    {
        _isAppActive = isActive;
        CheckTimerStatus();
        if (forceRefresh && _isActiveScreen)
        {
            await ForceRefreshAsync();
        }
    }

    public async Task ForceRefreshAsync()
    {
        var cts = _cancellationTokenSource;
        if (cts == null || cts.IsCancellationRequested) return;

        try
        {
            await RefreshDataAsync(cts.Token);
        }
        catch (ObjectDisposedException)
        {
            // CTS was disposed during navigation — safe to ignore
        }
    }

    private void CheckTimerStatus()
    {
        lock (_timerLock)
        {
            if (_isActiveScreen && _isAppActive)
            {
                if (_timer == null)
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                    // Use a short delay instead of TimeSpan.Zero to avoid
                    // racing with a still-running OnTick from the previous timer
                    var startDelay = Transporters.Any() ? RestartDelay : TimeSpan.Zero;
                    _timer = new Timer(OnTick, null, startDelay, RefreshInterval);
                }
            }
            else
            {
                StopTimer();
            }
        }
    }

    private void StopTimer()
    {
        // Cancel first so in-flight requests stop
        var oldCts = _cancellationTokenSource;
        _cancellationTokenSource = null;
        oldCts?.Cancel();

        _timer?.Dispose();
        _timer = null;

        // Reset reentrancy guard so the next timer start isn't blocked
        Interlocked.Exchange(ref _isRefreshing, 0);

        // Dispose CTS after resetting the guard to avoid ObjectDisposedException
        // while OnTick is still reading the token
        oldCts?.Dispose();
    }

    private async void OnTick(object? state)
    {
        if (Interlocked.CompareExchange(ref _isRefreshing, 1, 0) != 0)
            return;

        try
        {
            var cts = _cancellationTokenSource;
            if (cts == null || cts.IsCancellationRequested) return;

            await RefreshDataAsync(cts.Token);
        }
        catch (ObjectDisposedException)
        {
            // CTS was disposed during navigation — safe to ignore
        }
        finally
        {
            Interlocked.Exchange(ref _isRefreshing, 0);
        }
    }

    private async Task RefreshDataAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested) return;

            var result = await router.GetDevicePositionsByUserAsync(cancellationToken);

            if (!cancellationToken.IsCancellationRequested && result.Any())
            {
                Transporters = result;
                WeakReferenceMessenger.Default.Send(new DataRefreshedMessage(Transporters));
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when screen/app goes inactive during a request
        }
        catch
        {
            // Only show error if we have no cached data at all
            if (!Transporters.Any())
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    WeakReferenceMessenger.Default.Send(new ToastMessage(localization["Error"], true));
                });
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        lock (_timerLock)
        {
            StopTimer();
        }
        GC.SuppressFinalize(this);
        await Task.CompletedTask;
    }
}
