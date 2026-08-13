# Export guide — Polar

*Verified 2026-08-10 against Polar support: [How to download all your data from the Polar
ecosystem](https://support.polar.com/en/how-to-download-all-your-data-from-polar-flow),
[How do I export individual training sessions from Polar Flow web
service?](https://support.polar.com/en/export-training-sessions-flow), [Privacy
FAQ](https://www.polar.com/en/legal/faq).*

Polar has two separate exports, and you want both eventually:

1. **The account-wide package** (`account.polar.com`) — everything at once, asynchronous, arrives by
   mail as a ZIP of JSON. This is the one to upload to Strive.
2. **Per-session files** (`flow.polar.com`) — a single training session as GPX/TCX/CSV/FIT,
   downloaded immediately. Useful for a one-off, and the only way to get a session as a *route*
   file.

## 1. The full account export

1. Go to <https://account.polar.com> and sign in with your Polar account e-mail and password.
   This is the account site, **not** `flow.polar.com` — the bulk export does not live in Flow.
2. In the left-hand menu click **Download your data** (or scroll down the page to the same section).
3. Click **Download** to start the request. Nothing downloads yet; this only queues the collection.
4. Wait for the mail. Polar collects the data asynchronously and the wait scales with how much
   history you have and how busy the export queue is — hours to a couple of days is normal.
5. When the mail arrives, download the ZIP. **The link is valid for two weeks**; after that you have
   to request a new export.

### What is in the package

A ZIP of JSON files — one file per training session plus account-level files (account information,
calendar, favourite routes, registered products, sport definitions, and similar).

Two limitations worth knowing before you go looking for missing numbers:

- Polar states the export contains the data **you** provided plus most data coming from your Polar
  devices and apps, but **not values derived by Polar's own algorithms**. In practice that means the
  daily-activity and sleep summaries you see in Flow are not reliably part of the package.
- Anything that only ever existed in an older Polar ecosystem (ProTrainer, personaltrainer.polar.fi,
  RCX-era software) is not in here. Those files live in your own old backups — upload them
  separately, Strive has classifiers for the Polar ProTrainer formats too.

### Hand it to Strive

Upload the ZIP unmodified on the upload page. Strive's Polar Flow classifiers key off the JSON
shapes and the archive layout, so repacking or flattening the folders only makes classification
harder.

## 2. Exporting a single training session

For a one-off session (or to grab a route in a format another service wants):

1. Sign in at <https://flow.polar.com>.
2. Open **Diary** and click the session to open it.
3. Click the **Export** drop-down at the top right of the session page.
4. Pick the format:

   | Format | Contains |
   |---|---|
   | GPX | route only |
   | FIT | route plus training data |
   | TCX | training data — heart rate, calories, cadence — plus route |
   | CSV | training data as a flat table |

   There is also a *zip the files* option, which is faster for large sessions.

These per-session files are perfectly good uploads too — they just overlap heavily with the account
export, which is fine: duplicate content collapses to one catalog entry.

## Rhythm

Request the full account export every few months, and after any period where you switched devices.
Keep the mails; they date the snapshots.
