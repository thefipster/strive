# Export guide — Oral-B

**The path in the Oral-B app, current as of 2026-08-11:**
**More** → **Legal** → **EU Data Act** → **Export**
(German UI: *Mehr* → *Rechtliches* → *EU-Datengesetz* → *Exportieren*)

*In-app export path verified 2026-08-11 on a German-locale app. Background verified 2026-08-10
against [Oral-B — The European Data Act](https://www.oralb.co.uk/en-gb/the-european-data-act), the
[P&G privacy policy](https://privacypolicy.pg.com/en-US/) and its [data-request preference
centre](https://preferencecenter.pg.com/en-us/datarequests/), [Where can I find my brushing history
in the Oral-B app?](https://oralb.com/en-us/where-can-i-find-my-past-brushing-data-in-the-new-app/),
and the [Oral-B App Store listing](https://apps.apple.com/us/app/oral-b/id698092608).*

**Set expectations first.** Oral-B used to be the outlier in this set — no self-service export, only
a formal data request with a 30-day turnaround. That is no longer true: under **EU Data Act** the
app hands you a **JSON file** on the spot, via the normal share sheet. Route A below is now the
everyday route, and it is as quick as Polar's or Garmin's.

Two caveats keep the slower routes alive. The in-app export carries the data **held locally by the
app and the brush**, so anything the app has aged out or never held locally will not be in it. And
the entry sits under *EU Data Act*, so it may be absent outside the EU/EEA — check the menu before
assuming.

## Route A — in-app export (fast, JSON)

1. Open the Oral-B app and tap **More** (*Mehr*).
2. Tap **Legal** (*Rechtliches*).
3. Tap **EU Data Act** (*EU-Datengesetz*).
4. Tap **Export** (*Exportieren*).
5. The app produces a **JSON file** and opens the share sheet — save it to Files/Downloads, or share
   it to yourself. Anywhere you can get at it later is fine.

No mail, no waiting, no verification step. If your app shows the EU Data Act section but no export
action, you are on an older build — update the app first, then fall back to Route B.

## Route B — formal data request to P&G (the complete record)

Worth doing **once**, to establish a baseline beyond whatever the app holds locally, and again only
if you need a gap filled.

Oral-B is a P&G brand and **P&G is the data controller** for brushing data (in the EU/UK,
represented by Procter & Gamble Ireland, Dublin, under Art. 27 GDPR). The request goes to P&G, not
to Oral-B support.

1. Use the P&G privacy request form at <https://preferencecenter.pg.com/en-us/datarequests/> (the
   region-appropriate variant is linked from <https://privacypolicy.pg.com/>) and submit an
   **access** request for the account e-mail your Oral-B app uses.
2. **Verify by e-mail.** P&G confirm requests by mail — especially if the request was started on
   your behalf. **Not replying gets the request denied**, so watch for it, spam folder included.
3. Expect the statutory window: up to **30 days**.

What comes back is a subject-access response, not a curated developer export. Format is not
documented in advance and varies by region and request type — plan on CSV/JSON attachments or a
download link, and keep whatever arrives verbatim.

If you also want the data handed to a third party, that is a separate option on the same request
form; the same mail verification applies.

## Route C — via Apple Health (iOS only, and useful on its own)

The Oral-B app writes brushing sessions into **Apple Health**, which does have a real export. Since
Route A exists this is no longer the shortcut it once was — but an Apple Health export is the single
most useful iPhone-side export you can make for Strive in general, so it is still worth the two
taps.

1. In the Oral-B app, make sure the **Apple Health** connection is enabled and has been granted
   write permission for brushing/toothbrushing data.
2. On the iPhone: open **Health** → **Summary** → tap your **picture or initials** at the top right.
3. Tap **Export All Health Data**.
4. Wait — preparing takes a few minutes on a well-used Health database — then choose where to send
   the resulting **`export.zip`** (Files, mail, a note, or any share target).

The archive contains Apple's `export.xml` (all of Health, not just brushing) in XML.

**Android**: the Oral-B app's documented health-platform integration is the Apple Health one. If
your app version offers a Health Connect toggle, the equivalent trick is to export from Health
Connect (Settings → Health Connect → *Export data*).

## Hand it to Strive

- **Route A JSON**: Strive's upload page accepts **`.zip` only**, so put the JSON in a ZIP yourself
  first. Keep the original file name inside the archive — it is a classifier hint — and date the
  **archive**, since consecutive exports overlap heavily and identical content collapses to one
  catalog entry.

  ```bash
  zip oralb-2026-08-11.zip <the-exported-file>.json
  ```

  There is no Oral-B classifier yet, so it lands in the unknown/deferred queue — cataloged, hashed
  and stored, waiting for a reader. That is the intended path for a new format
  ([step 3](../roadmap/step-3-classification.md), [step 4](../roadmap/step-4-extraction.md)).
- **Route B response**: ZIP it as it arrived (`oralb-dsr-2026-08-10.zip`) and upload. Same
  unknown/deferred path.
- **Route C `export.zip`**: upload directly, it is already a ZIP. Note it is an Apple Health export
  containing Oral-B data, not an Oral-B export — worth saying in the package note, since the
  brushing records are a small part of a large archive.

## Rhythm

Route A monthly — it is four taps and costs nothing. Because it only covers what the app and brush
hold locally, exporting regularly is what stops old sessions falling off the end; a yearly export
is not equivalent to twelve monthly ones. Route B once for the baseline. Route C monthly if you are
on iOS, for Health as a whole rather than for Oral-B specifically.
