# Export guide — Withings

*Verified 2026-08-10 against Withings support: [Online Dashboard — Exporting my
data](https://support.withings.com/hc/en-us/articles/201491377-Withings-App-Online-Dashboard-Exporting-my-data),
[Android — Exporting your
data](https://support.withings.com/hc/en-us/articles/31647944317201-Withings-App-Android-Exporting-your-data),
[iOS — Exporting your
data](https://support.withings.com/hc/en-us/articles/360001399167-Withings-App-iOS-Exporting-your-data),
[Privacy — How can I export my
data?](https://support.withings.com/hc/en-us/articles/360001391287-Privacy-How-can-I-export-my-data).*

Withings exports **per user profile**, not per account. If several people share the scale under one
account, run the export once per profile — each one is a separate archive.

## Route A — the web dashboard (recommended)

The dashboard is the more reliable of the two routes; when the app export mail fails to arrive,
this is the fallback that usually works.

1. Open the Withings health dashboard at <https://account.withings.com> and sign in.
2. Click your **avatar** in the top-right corner → **Settings**.
3. Select the **user profile** whose data you want.
4. Click **Download my data**.
5. On the page that opens, start the export and confirm the mail address.
6. You get a mail with a download link; the archive is a ZIP of CSV files.

## Route B — the Withings app (iOS / Android)

1. Open the Withings app and go to your **Profile**.
2. Tap the **Settings** icon at the top right.
3. Tap **Export All Health Data**.
4. Pick the user profile, then tap **Start my archive**.
5. Wait for the mail containing the download link.

If the mail never shows up: check *All Mail* and *Spam* (the mail is nothing but a link, so filters
like it), confirm the address on the account is one you can actually read, and then retry through
the web dashboard.

## What is in the package

A ZIP of CSV files, one per measurement family. Roughly:

| File | Columns you care about |
|---|---|
| `weight.csv` | weight, fat mass, bone mass, muscle mass, hydration, comments |
| `activity.csv` | date, steps, distance, elevation, active calories |
| `sleep.csv` | from/to, light, deep, REM, awake, wake-up |
| `bp.csv` | systolic, diastolic, heart rate, comments |
| others | height, SpO₂, calories, body temperature, environment (temperature, luminosity) |

Exact file names vary a little by which devices you own — treat the table as a shape, not a
contract.

### Blood-pressure EKG

If you own a BPM Core / ScanWatch with EKG, the raw EKG payloads are the awkward part of this
export: Strive **classifies and stores them but defers parsing**, deliberately
([spec §deferred formats](../wearable-data-platform-spec.md)). They will sit in the unknown/deferred
queue. That is expected, not a failed import.

## Hand it to Strive

Upload the ZIP untouched — do not open the CSVs in Excel and re-save them, which silently rewrites
decimal separators and date formats. If you exported several profiles, upload each archive
separately; the per-package manifest keeps them apart even where rows overlap.

## Rhythm

Withings CSVs are cumulative full history, so a single fresh export always supersedes the previous
one in content. Re-export a few times a year; the duplicate rows cost nothing because the identical
files dedupe by hash and only genuinely new content creates catalog entries.
