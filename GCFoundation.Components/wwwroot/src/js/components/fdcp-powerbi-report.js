class FDCPPowerBiReport {
    constructor(element) {
        this.container = element;
        this.report = null;
        this.embedReport(this.container);
    }

    embedReport(container) {
        const models = window['powerbi-client'].models;

        const embedUrl = container.dataset.embeddedUrl;
        const accessToken = container.dataset.embeddedToken;
        const reportId = container.dataset.reportId;
        const refreshUrl = container.dataset.tokenRefreshUrl;

        const config = {
            type: 'report',
            id: reportId,
            embedUrl: embedUrl,
            accessToken: accessToken,
            tokenType: models.TokenType.Embed,
            settings: {
                panes: {
                    filters: { visible: false },
                    pageNavigation: { visible: false }
                }
            }
        };

        this.report = powerbi.embed(container, config);

        this.report.on('error', (event) => {
            console.error('Power BI report error:', event.detail);
        });

        if (refreshUrl) {
            const expiry = this.resolveExpiry(container, accessToken);
            if (expiry) {
                this.scheduleRefresh(container, this.report, expiry, refreshUrl);
            } else {
                console.warn('Power BI: could not determine token expiry, refresh not scheduled');
            }
        }
    }

    // Prefer the server-supplied tokenExpiry; fall back to decoding the JWT's exp claim
    resolveExpiry(container, accessToken) {
        const datasetExpiry = container.dataset.tokenExpiry;
        if (datasetExpiry) {
            const parsed = new Date(datasetExpiry).getTime();
            if (!isNaN(parsed)) return parsed;
        }

        try {
            const payload = JSON.parse(atob(accessToken.split('.')[1]));
            if (payload.exp) return payload.exp * 1000; // exp is in seconds
        } catch (err) {
            console.error('Power BI: failed to decode token for expiry', err);
        }

        return null;
    }

    scheduleRefresh(container, report, expiryTimeMs, refreshUrl) {
        const msUntilRefresh = expiryTimeMs - Date.now() - (2 * 60 * 1000);

        if (msUntilRefresh <= 0) {
            this.refreshToken(container, report);
            return;
        }

        setTimeout(() => this.refreshToken(container, report), msUntilRefresh);
    }

    async refreshToken(container, report) {
        const refreshUrl = container.dataset.tokenRefreshUrl;
        const reportId = container.dataset.reportId;

        if (!refreshUrl) return;

        try {
            const res = await fetch(`${refreshUrl}?reportId=${reportId}`);
            if (!res.ok) throw new Error('Token refresh failed');

            const { accessToken, tokenExpiry } = await res.json();

            await report.setAccessToken(accessToken);
            container.dataset.embeddedToken = accessToken;
            container.dataset.tokenExpiry = tokenExpiry;

            const expiry = this.resolveExpiry(container, accessToken);
            if (expiry) {
                this.scheduleRefresh(container, report, expiry, refreshUrl);
            }
        } catch (err) {
            console.error('Power BI token refresh failed:', err);
        }
    }
}

// Initialize one instance per container on the page
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.fdcp-powerbi-container').forEach((container) => {
        new FDCPPowerBiReport(container);
    });
});