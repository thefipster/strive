# Export day — all providers in one sitting

The happy path only: the primary route from each vendor guide, in the order that wastes the least
time. Every step here is the *first* route of its guide — follow the link when something does not
match, when you need a fallback, or when you want to know what is in the package.

The trick to doing all five in one sitting is that they split into two kinds:

- **Ask now, download later** — Polar, Garmin, Withings. Each mails you a link, minutes to days
  later. Fire all three off first.
- **Instant, on the device** — Google Maps Timeline, Oral-B. A file in your hand within a minute.
  Do these while the first three are cooking.

Budget ten minutes of clicking, then a few days of waiting for the Garmin mail.

## Phase 1 — fire off the slow ones

### Polar → [full guide](polar.md#1-the-full-account-export)

1. Sign in at <https://account.polar.com> — the account site, **not** `flow.polar.com`.
2. Left-hand menu → **Download your data**.
3. Click **Download** to queue the request.

Nothing downloads yet. The mail arrives in hours to a couple of days, and its link is valid for
**two weeks**.

### Garmin → [full guide](garmin.md#1-request-the-export)

1. Sign in at <https://connect.garmin.com>.
2. Click your **avatar** (top right) → **Account Information**.
3. Click **Data Management in your Garmin Account** — this leaves Connect for
   <https://www.garmin.com/en-US/account/datamanagement>, same account, expected.
4. Open **Export Your Data** → **Request Data Export**.

Mail subject: **"Action Required: Download Your Data"**, usually within 24–48 hours (ceiling: 30
days). **The prepared file is deleted after 3 days** — this is the one to watch for.

### Withings → [full guide](withings.md#route-a--the-withings-app-ios--android)

1. Open the Withings app → **Profile**.
2. **Settings** icon, top right.
3. **Export All Health Data**.
4. Pick the user profile → **Start my archive**.

Exports are **per user profile** — repeat for each person sharing the account. Mail arrives in
minutes to hours.

## Phase 2 — grab the instant ones

### Google Maps Timeline (Android) → [full guide](google-maps-timeline.md#android)

1. **Settings** → **Location** → **Location services** → **Timeline** → **Export Timeline data**.
   (On older builds **Timeline** sits directly under **Location**.)
2. **Continue** → **Save**, and choose where the file goes.

You get `Timeline.json` on the spot. On iPhone the route is inside Google Maps instead — see the
guide. This is the export that only exists as long as the phone does, so never skip it.

### Oral-B → [full guide](oral-b.md#route-a--in-app-export-fast-json)

1. Oral-B app → **More** (*Mehr*).
2. **Legal** (*Rechtliches*).
3. **EU Data Act** (*EU-Datengesetz*).
4. **Export** (*Exportieren*).
5. Save the JSON from the share sheet.

The entry sits under *EU Data Act*, so it may be missing outside the EU/EEA.

## Phase 3 — collect and upload

| Provider | Arrives | Deadline | Package |
|---|---|---|---|
| Google Maps Timeline | immediately | — | single JSON |
| Oral-B | immediately | — | single JSON |
| Withings | minutes to hours | none stated | ZIP of CSV |
| Polar | hours to days | link valid **2 weeks** | ZIP of JSON |
| Garmin | 24–48 h, up to 30 days | file deleted after **3 days** | ZIP of FIT + JSON |

Upload each package to Strive **unmodified** — the vendor's own archive layout is a classification
hint, and dedup is by content hash, so overlap with earlier exports costs nothing.

The two bare JSON files are the exception: the upload page accepts **`.zip` only**, so wrap them,
keeping the original name inside and dating the archive.

```bash
zip timeline-2026-08-11.zip Timeline.json
zip oralb-2026-08-11.zip <the-exported-file>.json
```

## Checklist

- [ ] Polar requested at `account.polar.com`
- [ ] Garmin export requested
- [ ] Withings archive started (once **per profile**)
- [ ] Timeline exported from the phone
- [ ] Oral-B exported from the app
- [ ] Garmin mail caught within 3 days
- [ ] Polar mail caught within 2 weeks
- [ ] Withings mail caught (check spam — it is a bare link)
- [ ] All five uploaded to Strive

## Rhythm

Quarterly for the whole set is a sensible default. Timeline and Oral-B deserve more — both are
device-local and lose old data over time, so monthly is worth the two minutes. Details and the
per-provider reasoning are in each guide.
