# Export guide — Oral-B

*Verified 2026-08-10 against [Oral-B — The European Data
Act](https://www.oralb.co.uk/en-gb/the-european-data-act), the [P&G privacy
policy](https://privacypolicy.pg.com/en-US/) and its [data-request preference
centre](https://preferencecenter.pg.com/en-us/datarequests/), [Where can I find my brushing history
in the Oral-B app?](https://oralb.com/en-us/where-can-i-find-my-past-brushing-data-in-the-new-app/),
and the [Oral-B App Store listing](https://apps.apple.com/us/app/oral-b/id698092608).*

**Set expectations first.** Oral-B is the outlier in this set: there is **no self-service "download
my data" button** in the app comparable to Polar's or Garmin's. Brushing history is visible in the
app (filtered by week, month, year) but the app has no documented bulk export to file. Getting the
data out means either a formal data request to P&G, or capturing the data via a platform the app
syncs into. Do both — they return different things.

## Route A — formal data request to P&G (the complete record)

Oral-B is a P&G brand and **P&G is the data controller** for brushing data (in the EU/UK,
represented by Procter & Gamble Ireland, Dublin, under Art. 27 GDPR). The request goes to P&G, not
to Oral-B support.

1. **From the app**: open the **Legal** menu and look for **EU Data Act**. That section carries the
   current instructions and the request link for accessing (or erasing) your brushing data. This is
   the route Oral-B itself points at, so prefer it — it arrives pre-associated with your account.
2. **From the web**, if the app has no such entry in your region: use the P&G privacy request form
   at <https://preferencecenter.pg.com/en-us/datarequests/> (the region-appropriate variant is
   linked from <https://privacypolicy.pg.com/>) and submit an **access** request for the account
   e-mail your Oral-B app uses.
3. **Verify by e-mail.** P&G confirm requests by mail — especially if the request was started on
   your behalf. **Not replying gets the request denied**, so watch for it, spam folder included.
4. Expect the statutory window: up to **30 days**.

What comes back is a subject-access response, not a curated developer export. Format is not
documented in advance and varies by region and request type — plan on CSV/JSON attachments or a
download link, and keep whatever arrives verbatim.

If you also want the data handed to a third party, that is a separate option on the same request
form; the same mail verification applies.

## Route B — via Apple Health (fast, partial, iOS only)

The Oral-B app writes brushing sessions into **Apple Health**, which does have a real export. This
gives you session-level history in minutes instead of weeks — but only what the app chose to write
to Health, and only from the point the integration was enabled.

1. In the Oral-B app, make sure the **Apple Health** connection is enabled and has been granted
   write permission for brushing/toothbrushing data.
2. On the iPhone: open **Health** → **Summary** → tap your **picture or initials** at the top right.
3. Tap **Export All Health Data**.
4. Wait — preparing takes a few minutes on a well-used Health database — then choose where to send
   the resulting **`export.zip`** (Files, mail, a note, or any share target).

The archive contains Apple's `export.xml` (all of Health, not just brushing) in XML. That is the
whole of Health, so it is also the single most useful iPhone-side export you can make for Strive in
general.

**Android**: the Oral-B app's documented health-platform integration is the Apple Health one. If
your app version offers a Health Connect toggle, the equivalent trick is to export from Health
Connect (Settings → Health Connect → *Export data*); if it does not, Route A is your only option.

## Route C — screenshots, honestly

For a handful of specific sessions, the in-app history view (week/month/year filters, coverage map,
pressure data) plus screenshots is a legitimate stopgap. It is not machine-readable, so it is
outside what Strive ingests — but it beats losing the record while a Route A request is pending.

## Hand it to Strive

- Route A response: ZIP it as it arrived (`oralb-dsr-2026-08-10.zip`) and upload. There is no
  Oral-B classifier yet, so it will land in the unknown/deferred queue — cataloged, hashed and
  stored, waiting for a reader. That is the intended path for a new format
  ([step 3](../roadmap/step-3-classification.md), [step 4](../roadmap/step-4-extraction.md)).
- Route B `export.zip`: upload directly, it is already a ZIP. Note it is an Apple Health export
  containing Oral-B data, not an Oral-B export — worth saying in the package note, since the
  brushing records are a small part of a large archive.

## Rhythm

Route B monthly if you are on iOS (it is two taps and covers all of Health). Route A once, to
establish the baseline history, and again only if you need a gap filled — a 30-day turnaround does
not suit a routine backup.
