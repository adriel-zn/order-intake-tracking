# order-intake-app

Angular 18 frontend for the Order Intake & Tracking app.

See the [repository root README](../README.md) for prerequisites and how to
run this alongside the backend, and [SOLUTION.md](../SOLUTION.md) for the
frontend's architecture notes.

Quick reference:

```bash
npm install
npm start   # ng serve on http://localhost:4200
npm test    # Karma/Jasmine unit tests
npm run build -- --configuration production
npm run format        # format the frontend with Prettier
npm run format:check  # check formatting without changing files
```

The workspace opens at `/dashboard`, showing order counts, a seven-day chart,
and the latest order updates from the API. The chart groups orders by creation
date; its fulfilled series counts those orders that are currently fulfilled.
The other screens are `/orders` for searching and filtering, `/orders/new` for
order entry, and `/orders/:id` for details and status updates. Header search opens
the order directory with a search query. The collapsible sidebar becomes
horizontal navigation on smaller screens.

When hosting the production build, configure the web server to return `index.html`
for application routes so bookmarked order URLs also work on refresh.
